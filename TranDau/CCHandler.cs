using System.Collections;
using UnityEngine;

public class CCHandler : MonoBehaviour
{
    [Header("CC Effects")]
    public GameObject bleedEffectPrefab;
    public GameObject stunEffectPrefab;
    public GameObject slowEffectPrefab;
    public GameObject knockUpEffectPrefab;

    public bool IsStunned { get; private set; }
    public bool IsKnockedUp { get; private set; }
    public bool IsSlowed { get; private set; }
    public bool IsSilenced { get; private set; }
    public bool IsBleeding { get; private set; }

    private Coroutine _coStun;
    private Coroutine _coKnockUp;
    private Coroutine _coSlow;
    private Coroutine _coSilence;
    private Coroutine _coBleed;

    private PlayerMove _playerMove;
    private PlayerOther _playerOther;
    private Animator _animator;

    private float _originalMoveSpeed;
    private float _originalOtherMoveSpeed;
    private const float SLOW_FACTOR = 0.5f;
    private const float KNOCKUP_HEIGHT = 120f;

    private GameObject _bleedInstance;
    private GameObject _stunInstance;
    private GameObject _slowInstance;
    private GameObject _knockUpInstance;

    private void Awake()
    {
        var all = GetComponents<CCHandler>();
        if (all.Length > 1)
            Debug.LogError($"[CCHandler] DUPLICATE! Có {all.Length} CCHandler trên {gameObject.name}!");

        _playerMove = GetComponent<PlayerMove>();
        _playerOther = GetComponent<PlayerOther>();

        if (_playerMove != null) _animator = _playerMove.animator;
        if (_playerOther != null) _animator = _playerOther.animator;

        Debug.Log($"[CCHandler] Awake | go={gameObject.name} | playerMove={_playerMove != null} | playerOther={_playerOther != null} | animator={_animator != null}");
    }

    // ═══════════════════════════════════════════════════════════
    //  PUBLIC API
    // ═══════════════════════════════════════════════════════════

    public void ApplyCC(int ccType, float duration)
    {
        Debug.Log($"[CCHandler] ApplyCC | go={gameObject.name} | ccType={ccType} | duration={duration}");
        switch (ccType)
        {
            case 1: ApplyStun(duration); break;
            case 2: ApplyKnockUp(duration); break;
            case 3: ApplySlow(duration); break;
            case 4: ApplySilence(duration); break;
            case 5: break;
            case 6: ApplyBleed(duration); break;
            default:
                Debug.LogWarning($"[CCHandler] Unknown ccType={ccType}");
                break;
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  STUN
    // ═══════════════════════════════════════════════════════════

    private void ApplyStun(float duration)
    {
        if (_coStun != null) StopCoroutine(_coStun);
        _coStun = StartCoroutine(CoStun(duration));
    }

    private IEnumerator CoStun(float duration)
    {
        Debug.Log($"[CCHandler] CoStun START | go={gameObject.name} | duration={duration}");
        IsStunned = true;

        if (_playerMove != null)
        {
            _playerMove.isInputLocked = true;
            _animator?.SetFloat("Speed", 0f);
        }

        if (_stunInstance != null) Destroy(_stunInstance);

        Debug.Log($"[CCHandler] stunEffectPrefab={stunEffectPrefab} | transform={transform.position}");

        if (stunEffectPrefab != null)
        {
            _stunInstance = Instantiate(stunEffectPrefab, transform.position, Quaternion.identity, transform);
            Debug.Log($"[CCHandler] _stunInstance spawned={_stunInstance != null} | active={_stunInstance?.activeInHierarchy}");
        }

        yield return new WaitForSeconds(duration);

        Debug.Log($"[CCHandler] CoStun END | go={gameObject.name}");
        IsStunned = false;

        if (_playerMove != null)
            _playerMove.isInputLocked = false;

        if (_stunInstance != null)
        {
            Destroy(_stunInstance);
            _stunInstance = null;
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  KNOCK UP
    // ═══════════════════════════════════════════════════════════

    private void ApplyKnockUp(float duration)
    {
        if (_coKnockUp != null) StopCoroutine(_coKnockUp);
        _coKnockUp = StartCoroutine(CoKnockUp(duration));
    }

    private IEnumerator CoKnockUp(float duration)
    {
        Debug.Log($"[CCHandler] CoKnockUp START | go={gameObject.name} | duration={duration}");
        IsKnockedUp = true;

        if (_playerMove != null)
            _playerMove.isInputLocked = true;

        _animator?.SetFloat("Speed", 0f);

        // Spawn effect
        if (_knockUpInstance != null) Destroy(_knockUpInstance);
        if (knockUpEffectPrefab != null)
        {
            _knockUpInstance = Instantiate(knockUpEffectPrefab, transform.position, Quaternion.identity, transform);
            Debug.Log($"[CCHandler] _knockUpInstance spawned={_knockUpInstance != null} | active={_knockUpInstance?.activeInHierarchy}");
        }

        float elapsed = 0f;
        float startY = transform.position.y;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float yOff = KNOCKUP_HEIGHT * 4f * t * (1f - t);

            Vector3 pos = transform.position;
            pos.y = startY + yOff;
            transform.position = pos;

            yield return null;
        }

        Vector3 final = transform.position;
        final.y = startY;
        transform.position = final;

        Debug.Log($"[CCHandler] CoKnockUp END | go={gameObject.name}");
        IsKnockedUp = false;

        if (_playerMove != null)
            _playerMove.isInputLocked = false;

        if (_knockUpInstance != null)
        {
            Destroy(_knockUpInstance);
            _knockUpInstance = null;
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  SLOW
    // ═══════════════════════════════════════════════════════════

    private void ApplySlow(float duration)
    {
        Debug.Log($"[CCHandler] ApplySlow called | go={gameObject.name} | IsSlowed={IsSlowed} | _coSlow={_coSlow != null}");
        if (_coSlow != null) StopCoroutine(_coSlow);
        _coSlow = StartCoroutine(CoSlow(duration));
    }

    private IEnumerator CoSlow(float duration)
    {
        Debug.Log($"[CCHandler] CoSlow START | go={gameObject.name} | duration={duration}");

        if (_playerMove == null && _playerOther == null)
        {
            Debug.LogWarning($"[CCHandler] CoSlow SKIP — không tìm thấy PlayerMove hoặc PlayerOther trên {gameObject.name}");
            yield break;
        }

        if (_playerMove != null)
        {
            if (!IsSlowed)
                _originalMoveSpeed = _playerMove.moveSpeed;
            _playerMove.moveSpeed = _originalMoveSpeed * SLOW_FACTOR;
        }

        if (_playerOther != null)
        {
            if (!IsSlowed)
                _originalOtherMoveSpeed = _playerOther.camGiacDiChuyen;
            _playerOther.camGiacDiChuyen = _originalOtherMoveSpeed * SLOW_FACTOR;
        }

        IsSlowed = true;

        // Destroy instance cũ trước khi spawn mới — tránh leak khi server gửi nhiều lần
        if (_slowInstance != null)
        {
            Debug.Log($"[CCHandler] CoSlow — destroy instance cũ trước khi spawn mới");
            Destroy(_slowInstance);
            _slowInstance = null;
        }

        if (slowEffectPrefab != null)
        {
            _slowInstance = Instantiate(slowEffectPrefab, transform.position, Quaternion.identity, transform);
            Debug.Log($"[CCHandler] _slowInstance spawned={_slowInstance != null} | active={_slowInstance?.activeInHierarchy} | pos={_slowInstance?.transform.position}");
        }
        else
        {
            Debug.LogWarning($"[CCHandler] slowEffectPrefab chưa được gán trên {gameObject.name}!");
        }

        yield return new WaitForSeconds(duration);

        Debug.Log($"[CCHandler] CoSlow END | go={gameObject.name}");

        if (_playerMove != null)
            _playerMove.moveSpeed = _originalMoveSpeed;

        if (_playerOther != null)
            _playerOther.camGiacDiChuyen = _originalOtherMoveSpeed;

        IsSlowed = false;

        if (_slowInstance != null)
        {
            Destroy(_slowInstance);
            _slowInstance = null;
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  SILENCE
    // ═══════════════════════════════════════════════════════════

    private void ApplySilence(float duration)
    {
        if (_coSilence != null) StopCoroutine(_coSilence);
        _coSilence = StartCoroutine(CoSilence(duration));
    }

    private IEnumerator CoSilence(float duration)
    {
        IsSilenced = true;
        yield return new WaitForSeconds(duration);
        IsSilenced = false;
    }

    // ═══════════════════════════════════════════════════════════
    //  BLEED
    // ═══════════════════════════════════════════════════════════

    private void ApplyBleed(float duration)
    {
        if (_coBleed != null) StopCoroutine(_coBleed);
        _coBleed = StartCoroutine(CoBleed(duration));
    }

    private IEnumerator CoBleed(float duration)
    {
        Debug.Log($"[CCHandler] CoBleed START | go={gameObject.name} | duration={duration}");
        IsBleeding = true;

        if (_bleedInstance != null) Destroy(_bleedInstance);

        Debug.Log($"[CCHandler] bleedEffectPrefab={bleedEffectPrefab} | transform={transform.position}");

        if (bleedEffectPrefab != null)
        {
            _bleedInstance = Instantiate(bleedEffectPrefab, transform.position, Quaternion.identity, transform);
            Debug.Log($"[CCHandler] _bleedInstance spawned={_bleedInstance != null} | active={_bleedInstance?.activeInHierarchy}");
        }

        yield return new WaitForSeconds(duration);

        Debug.Log($"[CCHandler] CoBleed END | go={gameObject.name}");
        IsBleeding = false;

        if (_bleedInstance != null)
        {
            Destroy(_bleedInstance);
            _bleedInstance = null;
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  CLEANUP
    // ═══════════════════════════════════════════════════════════

    private void OnDisable()
    {
        if (IsSlowed)
        {
            if (_playerMove != null)
                _playerMove.moveSpeed = _originalMoveSpeed;
            if (_playerOther != null)
                _playerOther.camGiacDiChuyen = _originalOtherMoveSpeed;
            IsSlowed = false;
        }

        if (_stunInstance != null) { Destroy(_stunInstance); _stunInstance = null; }
        if (_slowInstance != null) { Destroy(_slowInstance); _slowInstance = null; }
        if (_bleedInstance != null) { Destroy(_bleedInstance); _bleedInstance = null; }
        if (_knockUpInstance != null) { Destroy(_knockUpInstance); _knockUpInstance = null; }
    }
}