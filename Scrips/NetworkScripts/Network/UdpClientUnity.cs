using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// UDP Client cho game MOBA
/// Format: [2 bytes size big-endian][{"cmd":X,"data":{...}}]
/// </summary>
public class UdpClientUnity : MonoBehaviour
{
    public static UdpClientUnity Instance { get; private set; }

    private UdpClient udp;
    private IPEndPoint serverEndPoint;
    private Thread receiveThread;
    private volatile bool isRunning;
    private readonly ConcurrentQueue<Message> receiveQueue = new ConcurrentQueue<Message>();

    private const int SOCKET_TIMEOUT_MS = 5000;
    private const int THREAD_JOIN_TIMEOUT_MS = 100;

    #region Unity Lifecycle

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        InitSocket();
    }

    void Update()
    {
        DispatchReceiveMessages();
    }

    void OnDestroy()
    {
        CloseSocket();
        Instance = null;
    }

    void OnApplicationQuit()
    {
        CloseSocket();
    }

    #endregion

    #region Khởi tạo Socket

    /// <summary>
    /// Khởi tạo UDP socket và bắt đầu thread nhận dữ liệu
    /// </summary>
    private void InitSocket()
    {
        try
        {
            serverEndPoint = new IPEndPoint(
                IPAddress.Parse(B.Instance.udpIp),
                B.Instance.udpPort
            );

            udp = new UdpClient(0);
            udp.Client.ReceiveTimeout = SOCKET_TIMEOUT_MS;
            udp.Client.SendTimeout = SOCKET_TIMEOUT_MS;
            // udp.Connect(serverEndPoint);

            isRunning = true;

            receiveThread = new Thread(ReceiveLoop)
            {
                Name = "UDP-Receive",
                IsBackground = true
            };
            receiveThread.Start();

            Debug.Log($"[UDP] Đã kết nối tới {serverEndPoint}");

            Invoke(nameof(SendTestHandshake), 1f);
        }
        catch (Exception e)
        {
            Debug.LogError($"[UDP] Lỗi khởi tạo: {e.Message}");
            isRunning = false;
        }
    }

    #endregion

    #region Gửi Message

    /// <summary>
    /// Gửi message tới server qua UDP
    /// Thread-safe, có thể gọi từ bất kỳ đâu
    /// </summary>
    public void Send(Message msg)
    {
        if (udp == null || !isRunning)
        {
            Debug.LogWarning("[UDP] Không thể gửi: socket chưa sẵn sàng");
            return;
        }

        try
        {
            /// Build JSON đầy đủ: {"cmd":X,"data":{...}}
            string fullJson = JsonConvert.SerializeObject(new
            {
                cmd = msg.cmd,
                data = msg.data ?? new Dictionary<string, object>()
            });

            byte[] jsonBytes = Encoding.UTF8.GetBytes(fullJson);

            /// Build packet nhị phân: [size:2 bytes][json]
            byte[] packet = new byte[2 + jsonBytes.Length];

            /// Ghi size (2 bytes, big-endian)
            short size = (short)jsonBytes.Length;
            packet[0] = (byte)((size >> 8) & 0xFF);
            packet[1] = (byte)(size & 0xFF);

            /// Copy JSON data
            Buffer.BlockCopy(jsonBytes, 0, packet, 2, jsonBytes.Length);

            /// Gửi tới server
            udp.Send(packet, packet.Length, serverEndPoint);

            Debug.Log($"[UDP] Đã gửi: cmd={msg.cmd}, size={size}");
        }
        catch (ObjectDisposedException)
        {
            Debug.LogWarning("[UDP] Socket đã đóng khi gửi");
            isRunning = false;
        }
        catch (Exception e)
        {
            Debug.LogError($"[UDP] Lỗi gửi: {e.Message}");
        }
    }

    #endregion

    #region Nhận Message (Background Thread)

    /// <summary>
    /// VÒNG LẶP NHẬN DỮ LIỆU - CHẠY Ở THREAD RIÊNG
    /// 
    /// Thread này thực hiện:
    /// 1. Nhận packet nhị phân từ server
    /// 2. Parse binary → JSON string
    /// 3. Deserialize JSON → Message object
    /// 4. Đưa message vào queue để main thread xử lý
    /// 
    /// Format: [2 bytes size][{"cmd":999,"data":{...}}]
    /// </summary>
    private void ReceiveLoop()
    {
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
        Debug.Log("[UDP] Thread nhận dữ liệu đã bắt đầu");

        while (isRunning)
        {
            try
            {
                /// BƯỚC 1: Nhận packet nhị phân
                byte[] packet = udp.Receive(ref remoteEP);

                if (packet == null || packet.Length < 2)
                {
                    Debug.LogWarning($"[UDP] Packet không hợp lệ: length={packet?.Length ?? 0}");
                    continue;
                }

                /// BƯỚC 2: Parse size header (2 bytes)
                int size = (packet[0] << 8) | packet[1];

                if (size <= 0 || size > packet.Length - 2)
                {
                    Debug.LogError($"[UDP] Size không hợp lệ: {size} (packet length: {packet.Length})");
                    continue;
                }

                /// BƯỚC 3: Trích xuất JSON string
                string fullJson = Encoding.UTF8.GetString(packet, 2, size);

                /// BƯỚC 4: Deserialize JSON → Dictionary
                Dictionary<string, object> jsonObj;
                try
                {
                    jsonObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(fullJson);
                }
                catch (JsonException je)
                {
                    Debug.LogError($"[UDP] Lỗi parse JSON: {je.Message}\nJSON: {fullJson}");
                    continue;
                }

                if (jsonObj == null || !jsonObj.ContainsKey("cmd") || !jsonObj.ContainsKey("data"))
                {
                    Debug.LogError($"[UDP] Cấu trúc JSON không hợp lệ: {fullJson}");
                    continue;
                }

                /// BƯỚC 5: Trích xuất cmd và data
                int cmd = Convert.ToInt32(jsonObj["cmd"]);
                JObject dataObj = jsonObj["data"] as JObject;

                /// BƯỚC 6: Tạo Message object
                Message msg = new Message(cmd);

                if (dataObj != null)
                {
                    msg.data = dataObj.ToObject<Dictionary<string, object>>();
                }
                else
                {
                    msg.data = new Dictionary<string, object>();
                }

                /// BƯỚC 7: Đưa vào queue cho main thread
                receiveQueue.Enqueue(msg);

                Debug.Log($"[UDP] Đã nhận: cmd={cmd}, size={size}");
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode == SocketError.TimedOut)
                {
                    // timeout là bình thường, tiếp tục loop
                    continue;
                }

                Debug.LogWarning($"[UDP] Socket error: {se.SocketErrorCode}");
                continue;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception e)
            {
                Debug.LogError($"[UDP] Lỗi nhận: {e.GetType().Name}: {e.Message}");
            }
        }

        Debug.Log("[UDP] Thread nhận dữ liệu đã dừng");
    }

    #endregion

    #region Xử lý Message (Main Thread)

    /// <summary>
    /// XỬ LÝ MESSAGES - CHẠY Ở MAIN THREAD (Unity Update)
    /// 
    /// Xử lý tất cả messages mà background thread đã nhận.
    /// Tại đây, messages đã được parse hoàn toàn và sẵn sàng sử dụng.
    /// 
    /// Message object chứa:
    ///   - msg.cmd (int): Mã lệnh
    ///   - msg.data (Dictionary): Dữ liệu
    ///   - msg.GetInt(key), msg.GetLong(key), v.v.: Các phương thức hỗ trợ
    /// </summary>
    private void DispatchReceiveMessages()
    {
        int processedCount = 0;
        const int MAX_MESSAGES_PER_FRAME = 50;

        while (processedCount < MAX_MESSAGES_PER_FRAME && receiveQueue.TryDequeue(out Message msg))
        {
            processedCount++;

            AgentUnity.LogWarning($"[UDP] Xử lý cmd={msg.GetJson()}");
            /// TẠI ĐÂY: MESSAGE ĐÃ ĐƯỢC PARSE HOÀN TOÀN
            /// msg.cmd = Mã lệnh (vd: 999)
            /// msg.data = Dictionary<string, object>
            /// Truy cập qua: msg.GetInt("key"), msg.GetLong("key"), v.v.

            try
            {
                /// Tùy chọn 1: Chuyển sang Entitas (mặc định)
                DispatchToEntitas(msg);

                /// Tùy chọn 2: Xử lý trực tiếp (uncomment để dùng)
                HandleMessage(msg);
            }
            catch (Exception e)
            {
                Debug.LogError($"[UDP] Lỗi xử lý cmd={msg.cmd}: {e.Message}\n{e.StackTrace}");
            }
        }

        /// Cảnh báo nếu queue đang tăng quá nhanh
        if (receiveQueue.Count > 100)
        {
            Debug.LogWarning($"[UDP] Queue tràn: {receiveQueue.Count} messages đang chờ");
        }
    }

    /// <summary>
    /// Chuyển message sang Entitas system
    /// </summary>
    private void DispatchToEntitas(Message msg)
    {
        NetworkEntity entity = Contexts.sharedInstance.network.CreateEntity();
        entity.AddMessageData(msg);
    }

    /// <summary>
    /// TÙY CHỌN: Xử lý messages trực tiếp
    /// Uncomment và sử dụng method này nếu muốn tự xử lý
    /// </summary>

    private void HandleMessage(Message msg)
    {

        switch (msg.cmd)
        {
            case CMD.SEND_START_GAME: /// UDP_HANDSHAKE
                HandleUdpHandshake(msg);
                break;

            case CMD.GAME_SNAPSHOT:
                HandleGameSnapshot(msg);
                break;

            case CMD.DAMAGE_DEALT:
                HandleDamageDealt(msg);
                break;

            case CMD.DEATH:
                HandlePlayerDeath(msg);
                break;

            case CMD.PUT_TRU_BAN:
                HandleTruBan(msg);
                break;

            default:
                Debug.LogWarning($"[UDP] Chưa xử lý cmd: {msg.cmd}");
                break;
        }
    }

    private void HandleUdpHandshake(Message msg)
    {
        int status = msg.GetInt("status");
        string message = msg.GetString("message");

        if (status == 1)
        {
            Debug.LogError($"[UDP] Handshake thành công: {message}");
        }
        else
        {
            Debug.LogError($"[UDP] Handshake thất bại: {message}");
        }
    }

    private void HandleTruBan(Message msg)
    {
        try
        {
            // AgentUnity.LogError("CMD: PUT_TRU_BAN = 36" + msg.GetJson());
            long turretId = msg.GetLong("turretId");
            long targetId = msg.GetLong("targetId");
            int targetType = msg.GetInt("targetType");
            int turretTeam = msg.GetInt("turretTeam");
            TranDauControl.Instance.PutTruBan(turretId, targetId, targetType, turretTeam);
        }
        catch (Exception e)
        {
            Debug.LogError($"   Message: {e}");
        }
    }

    private void HandleGameSnapshot(Message msg)
    {
        try
        {
            AgentUnity.LogError("CMD: GAME_SNAPSHOT = 50" + msg.GetJson());
            List<PlayerOutPutSv> players = msg.GetClassList<PlayerOutPutSv>("players");
            List<MinionOutPutSv> minions = msg.GetClassList<MinionOutPutSv>("minions");
            List<JungleMonsterOutPutSv> monsters = msg.GetClassList<JungleMonsterOutPutSv>("monsters");

            if (TranDauControl.Instance != null)
            {
                TranDauControl.Instance.Init(players);
                TranDauControl.Instance.InitMinions(minions);
                TranDauControl.Instance.InitMonster(monsters);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"   Message: {e.Message}");
            Debug.LogError($"   Stack: {e.StackTrace}");
        }
    }

    private void HandleDamageDealt(Message msg)
    {
        try
        {
            // AgentUnity.LogError("CMD: DAMAGE_DEALT = " + msg.GetJson());
            long targetId = msg.GetLong("targetId");
            long attackerId = msg.GetLong("attackerId");
            int damage = msg.GetInt("damage");
            int remainingHp = msg.GetInt("remainingHp");
            int skillId = msg.GetInt("skillId");
            int targetType = msg.GetInt("targetType");

            switch (targetType)
            {
                case 0:
                    if (attackerId != UserData.Instance.UserID)
                    {
                        if (targetId != UserData.Instance.UserID)
                        {
                            if (skillId == 0)
                            {
                                TranDauControl.Instance.SetAttackState(true, false);
                            }
                            else
                            {
                                TranDauControl.Instance.SetCastSkillState(skillId, false);
                            }
                        }
                        else
                        {

                            if (skillId == 0)
                            {
                                TranDauControl.Instance.SetAttackState(true, true);
                            }
                            else
                            {
                                TranDauControl.Instance.SetCastSkillState(skillId, true);
                            }
                        }
                    }
                    break;
                case 1:
                case 4:
                    if (attackerId != UserData.Instance.UserID)
                    {
                        if (skillId == 0)
                        {
                            TranDauControl.Instance.SetAttackState(true, false);
                        }
                        else
                        {
                            TranDauControl.Instance.SetCastSkillState(skillId, false);
                        }
                    }
                    break;
                default:
                    if (attackerId != UserData.Instance.UserID)
                    {
                        if (skillId == 0)
                        {
                            TranDauControl.Instance.SetAttackState(true, false);
                        }
                        else
                        {
                            TranDauControl.Instance.SetCastSkillState(skillId, false);
                        }
                    }
                    break;
            }
        }
        catch (Exception e)
        {
            AgentUnity.LogError(e);
        }
    }

    private void HandlePlayerDeath(Message msg)
    {
        try
        {
            AgentUnity.LogError("CMD: DEATH = 32" + msg.GetJson());
            int victimType = msg.GetInt("victimType");
            long killerId = msg.GetLong("killerId");
            string killerName = msg.GetString("killerName");
            long victimId = msg.GetLong("victimId");
            string victimName = msg.GetString("victimName");
            int timeReBorn = msg.GetInt("timeReBorn");

            switch (victimType)
            {
                case 0:
                    if (victimId == UserData.Instance.UserID)
                    {
                        TranDauControl.Instance.playerMove.onDeath();
                    }
                    else
                    {
                        TranDauControl.Instance.playerOther.onDeath();
                    }
                    break;
                case 1:
                    TranDauControl.Instance.MinionDeath(victimId);
                    break;
                case 2:
                    TranDauControl.Instance.MonterDeath(victimId);
                    break;
                case 4:
                    TranDauControl.Instance.TruLinhDeath((int)victimId);
                    break;
            }
        }
        catch (Exception e)
        {
            AgentUnity.LogError(e);
        }
    }


    #endregion

    #region Cleanup

    /// <summary>
    /// Đóng UDP socket và dọn dẹp resources
    /// </summary>
    private void CloseSocket()
    {
        Debug.Log("[UDP] Đang đóng socket...");

        isRunning = false;

        /// Đóng UDP client
        try
        {
            udp?.Close();
            udp?.Dispose();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[UDP] Lỗi đóng socket: {e.Message}");
        }
        finally
        {
            udp = null;
        }

        /// Đợi receive thread kết thúc
        if (receiveThread != null && receiveThread.IsAlive)
        {
            bool joined = receiveThread.Join(THREAD_JOIN_TIMEOUT_MS);

            if (!joined)
            {
                Debug.LogWarning("[UDP] Thread nhận dữ liệu không kết thúc kịp");
            }

            receiveThread = null;
        }

        /// Xóa message queue
        while (receiveQueue.TryDequeue(out _)) { }

        Debug.Log("[UDP] Socket đã đóng");
    }

    #endregion

    #region Test Methods

    /// <summary>
    /// Gửi test UDP handshake tới server
    /// </summary>
    private void SendTestHandshake()
    {
        Debug.Log("[UDP] Gửi test handshake...");

        Message msg = new Message(999);
        // msg.PutLong("userId", UserData.Instance.UserID);
        // msg.PutString("keyhash", B.Instance.Keyhash);
        msg.PutLong("userId", 5);
        msg.PutString("keyhash", "");

        Send(msg);
    }

    #endregion

    #region Public Helper Methods

    /// <summary>
    /// Kiểm tra UDP client có đang kết nối không
    /// </summary>
    public bool IsConnected()
    {
        return isRunning && udp != null;
    }

    /// <summary>
    /// Lấy số lượng messages đang chờ xử lý
    /// </summary>
    public int GetPendingMessageCount()
    {
        return receiveQueue.Count;
    }

    /// <summary>
    /// Đóng và kết nối lại
    /// </summary>
    public void Reconnect()
    {
        CloseSocket();
        Invoke(nameof(InitSocket), 0.5f);
    }

    #endregion
}