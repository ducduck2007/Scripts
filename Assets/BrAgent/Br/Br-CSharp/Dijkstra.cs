using System.Collections.Generic;

namespace Br
{
    public class Dijkstra
    {
        public int[,] _maTranTrongSo { get; private set; }
        List<bool> _listPointDaXet { get; set; }
        /// <summary>
        /// Danh sách các con đường đang tìm
        /// </summary>
        List<Route> _listRoute { get; set; }
        int _firstPoint { get; set; }
        int _destinationPoint { get; set; }

        public Dijkstra(int[,] mapIndex)
        {
            if (mapIndex != null)
            {
                this._maTranTrongSo = mapIndex;
                Init();
            }
        }
        private void Init()
        {
            _listRoute = new List<Route>();
            _listPointDaXet = new List<bool>();
            for (int i = 0; i < _maTranTrongSo.GetLength(0); i++) _listPointDaXet.Add(false);
            DanhDauVoCung();
        }
        /// <summary>
        /// Xử lý ma trận trọng số truyền vào chương trình. Nếu giữa hai điểm không tồn tại đường đi thì đánh dấu trong ma trận trọng số là -1
        /// </summary>
        private void DanhDauVoCung()
        {
            if (_maTranTrongSo == null) return;

            for (int i = 0; i < _maTranTrongSo.GetLength(0); i++)
            {
                for (int j = 0; j < _maTranTrongSo.GetLength(1); j++)
                    if (i != j && _maTranTrongSo[i, j] <= 0) _maTranTrongSo[i, j] = -1;
            }
        }
        /// <summary>
        /// Tim đường đi giữa đi hai điểm truyền vào
        /// </summary>
        public List<int> SearchRoute(int firstPoint, int destinationPoint)
        {
            Init();

            firstPoint--;
            destinationPoint--;
            if (firstPoint < 0) firstPoint = 0;
            if (destinationPoint < 0) destinationPoint = 0;
            this._firstPoint = firstPoint;
            this._destinationPoint = destinationPoint;
            if (firstPoint < 0 || firstPoint > _maTranTrongSo.GetLength(0) || destinationPoint < 0 || destinationPoint > _maTranTrongSo.GetLength(0)) return null;

            List<int> duong = new List<int>();

            Route route = new Route();
            route.ListDiemDaDi.Add(firstPoint);

            this._listRoute.Add(route);

            int dangXet = firstPoint;
            _listPointDaXet[firstPoint] = true;

            while (!CheckEnd(destinationPoint))
            {
                Route timDuong = this._listRoute[0];
                int diemMoi = -1;

                foreach (Route item in this._listRoute)
                {
                    int s = TimDuong(item);
                    if (s != -1)
                    {
                        if (diemMoi == -1)
                        {
                            diemMoi = s;
                            timDuong = item;
                        }
                        else
                        {
                            if (s < diemMoi)
                            {
                                diemMoi = s;
                                timDuong = item;
                            }
                        }
                    }
                }

                int soLuongThem = FindTheNextPoint(timDuong.ListDiemDaDi[timDuong.ListDiemDaDi.Count - 1]).Count;
                for (int i = 0; i < soLuongThem - 1; i++)
                {
                    Route ddn = new Route();
                    foreach (int item in timDuong.ListDiemDaDi) ddn.ListDiemDaDi.Add(item);

                    this._listRoute.Add(ddn);
                }

                if (diemMoi != -1)
                {
                    timDuong.ListDiemDaDi.Add(diemMoi);
                    _listPointDaXet[diemMoi] = true;
                }

                if (diemMoi == destinationPoint) duong = timDuong.ListDiemDaDi;
            }

            return duong;
        }
        /// <summary>
        /// Tìm danh sách điểm kề từ một điểm
        /// </summary>
        /// <returns></returns>
        private List<int> FindTheNextPoint(int diem)
        {
            List<int> listKq = new List<int>();

            for (int i = 0; i < _maTranTrongSo.GetLength(0); i++)
            {
                if (diem != i && _maTranTrongSo[diem, i] > 0 && !_listPointDaXet[i]) listKq.Add(i);
            }

            return listKq;
        }
        /// <summary>
        /// Kiểm tra xem có phải tất cả các con đường đã đi tới đường cụt hay không (isDie=true). Nếu tất cả đã tới đường cụt thì không tồn tại đường đi giữa hai điểm truyền vào hoặc đã tìm được đường đi giữa hai điểm.
        /// </summary>
        private bool CheckEnd(int diemCuoi)
        {
            bool kt = true;

            foreach (Route item in _listRoute)
            {
                if (!item.IsDie)
                {
                    kt = false;
                }
            }

            foreach (Route item in _listRoute)
            {
                if (!item.IsDie)
                {
                    if (item.ListDiemDaDi[item.ListDiemDaDi.Count - 1] == diemCuoi) return true;
                }
            }

            return kt;
        }

        private int TimDuong(Route dd)
        {
            int le = dd.ListDiemDaDi.Count;
            int diem = dd.ListDiemDaDi[le - 1];
            List<int> list = FindTheNextPoint(diem);

            if (list.Count == 0)
            {
                dd.IsDie = true;
                return -1;
            }

            int min = _maTranTrongSo[diem, list[0]];
            int diemToi = list[0];
            foreach (int item in list) if (min >= _maTranTrongSo[diem, item] && _maTranTrongSo[diem, item] > 0)
                {
                    if (diem == this._destinationPoint && min > _maTranTrongSo[diem, item]) continue;
                    min = _maTranTrongSo[diem, item];
                    diemToi = item;
                }

            return diemToi;
        }
        /// <summary>
        /// Tính khoảng cách giữa hai điểm nhờ con đường được truyền vào
        /// </summary>
        public int CaculateSugar(List<int> duongDi)
        {
            if (_maTranTrongSo != null)
            {
                if (duongDi.Count == 0)
                {
                    return 0;
                }
                else
                {
                    int quangDuong = 0;
                    for (int i = 0; i < duongDi.Count - 1; i++)
                    {
                        quangDuong += _maTranTrongSo[duongDi[i], duongDi[i + 1]];
                    }

                    return quangDuong;
                }
            }
            else Log("Chưa có dữ liệu ma trận kề");
            return 0;
        }
        public string GetLogDetailRoute(int pointA, int pointB, List<int> listPoint)
        {
            if (listPoint != null && listPoint.Count > 0)
            {
                string detaiRoute = "Quãng đường đi từ " + pointA + " tới " + pointB + " là: " + CaculateSugar(listPoint);
                detaiRoute += "\nĐường đi: ";
                for (int i = 0; i < listPoint.Count; i++)
                {
                    detaiRoute += (listPoint[i] + 1).ToString();
                    if (i != listPoint.Count - 1) detaiRoute += " ----> ";
                }
                return detaiRoute;
            }
            return string.Empty;
        }
        private void Log(object data)
        {
            System.Console.WriteLine(data);
        }
    }
    public class Route
    {
        public List<int> ListDiemDaDi { get; private set; }
        /// <summary>
        /// Đường cụt
        /// </summary>
        public bool IsDie { get; set; }
        public Route()
        {
            ListDiemDaDi = new List<int>();
            this.IsDie = false;

        }
        public bool AddPoint(int point)
        {
            foreach (int item in this.ListDiemDaDi) if (item == point) return false;
            this.ListDiemDaDi.Add(point);
            return true;
        }
    }
}