using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinionMove : MonoBehaviour
{
    [Header("HP Tween")]
    public float hpTweenDuration = 0.2f;
    private Tween hpTween;

    public long minionId;
    public int teamId;
    public int laneId;

    public TMP_Text txtHP;
    public Image imgFill;
    public Sprite[] sprMau;

    private Vector3 targetPos;
    private bool isAlive = true;

    public float moveSmooth = 10f;
    public float stopThreshold = 10f;

    public Animator aniLinhDo, aniLinhXanh;
    private Animator aniLinh;

    [SerializeField] float rotateSpeed = 10f;

    [SerializeField] private float attackHoldSeconds = 0.18f;
    private float _attackHoldUntil = 0f;
    private bool _atkWanted = false;

    private Vector3 _fromPos;
    private Vector3 _toPos;
    private float _interpT;
    private float _snapshotInterval = 0.1f;

    void Update()
    {
        if (!isAlive || aniLinh == null) return;

        _interpT = Mathf.Clamp01(_interpT + Time.deltaTime / _snapshotInterval);
        transform.position = Vector3.Lerp(_fromPos, _toPos, _interpT);

        Vector3 dir = _toPos - _fromPos;
        dir.y = 0f;
        float sqrDist = dir.sqrMagnitude;

        bool atkNow = _atkWanted || (Time.time < _attackHoldUntil);

        if (atkNow)
        {
            SetAnimatorSpeed(0f);
            SetAnimatorAttack(true);
            if (sqrDist > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
            }
        }
        else
        {
            SetAnimatorAttack(false);
            if (sqrDist > 0.01f)
            {
                SetAnimatorSpeed(1f);
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
            }
            else
            {
                SetAnimatorSpeed(0f);
            }
        }
    }

    public void SetData(long id, int team, int lane)
    {
        minionId = id;
        teamId = team;
        laneId = lane;

        if (teamId == 1)
        {
            aniLinh = aniLinhXanh;
            if (aniLinhDo != null) aniLinhDo.gameObject.SetActive(false);
            if (aniLinhXanh != null) aniLinhXanh.gameObject.SetActive(true);

            int layer1 = LayerMask.NameToLayer("player1");
            if (layer1 != -1) gameObject.layer = layer1;
        }
        else
        {
            aniLinh = aniLinhDo;
            if (aniLinhXanh != null) aniLinhXanh.gameObject.SetActive(false);
            if (aniLinhDo != null) aniLinhDo.gameObject.SetActive(true);

            int layer2 = LayerMask.NameToLayer("player2");
            if (layer2 != -1) gameObject.layer = layer2;
        }

        if (imgFill != null && sprMau != null && sprMau.Length >= 2)
            imgFill.sprite = (teamId == B.Instance.teamId) ? sprMau[0] : sprMau[1];
    }

    float lastHpPercent = -1f;

    public void UpdateFromServer(float x, float y, int hp, int maxHp)
    {
        targetPos = new Vector3(x, 0, y);

        if (maxHp <= 0) return;

        if (imgFill != null && !imgFill.enabled)
            imgFill.enabled = true;
        if (txtHP != null && !txtHP.enabled)
            txtHP.enabled = true;

        float percent = Mathf.Clamp01((float)hp / maxHp);

        if (Mathf.Abs(lastHpPercent - percent) > 0.01f)
        {
            lastHpPercent = percent;

            if (imgFill != null)
            {
                hpTween?.Kill();
                hpTween = imgFill
                    .DOFillAmount(percent, hpTweenDuration)
                    .SetEase(Ease.OutQuad);
            }
        }

        if (txtHP != null)
            txtHP.text = $"{hp}/{maxHp}";

        if (hp <= 0 && isAlive)
            OnDeath();
    }

    float lastSpeed = -1f;
    bool lastAttack = false;

    void SetAnimatorSpeed(float speed)
    {
        if (aniLinh == null) return;
        if (Mathf.Abs(lastSpeed - speed) < 0.01f) return;

        lastSpeed = speed;
        aniLinh.SetFloat("Speed", speed);
    }

    void SetAnimatorAttack(bool val)
    {
        if (aniLinh == null) return;
        if (lastAttack == val) return;

        lastAttack = val;
        aniLinh.SetBool("isAttack", val);
    }

    public void OnDeath()
    {
        isAlive = false;

        if (imgFill != null) imgFill.enabled = false;
        if (txtHP != null) txtHP.enabled = false;

        gameObject.SetActive(false);
    }

    public void ApplySnapshot(float x, float y, int isAttack)
    {
        Vector3 newTarget = new Vector3(x, 0f, y);

        _fromPos = transform.position;
        _toPos = newTarget;
        _interpT = 0f;

        bool atk = (isAttack != 0);
        if (atk)
        {
            _atkWanted = true;
            _attackHoldUntil = Time.time + attackHoldSeconds;
        }
        else
        {
            if (Time.time >= _attackHoldUntil)
                _atkWanted = false;
        }
    }
}
