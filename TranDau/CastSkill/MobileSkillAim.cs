using UnityEngine;

public class MobileSkillAim : MonoBehaviour
{
    [Header("Camera")]
    public Camera cam;

    [Header("Rotate (quạt/line)")]
    public bool enableRotateVisual = false;
    public Transform rotateVisual;
    public float rotateOffsetY = 0f;

    [Header("Ground-target AOE (vòng tròn đặt điểm)")]
    public bool enableGroundMove = false;
    public RectTransform skillShot;
    public RectTransform skillShotSmall;
    public bool autoRangeFromSkillShot = true;
    public float maxRangeWorld = 185f;
    public float innerPaddingWorld = 0f;

    [Header("Anti-jitter")]
    public float deadzone = 0.08f;
    public float smoothTime = 0.05f;
    public float maxMoveSpeed = 9999f;

    public static Vector3 LastRotateDirWorld = Vector3.forward;

    float lastY;

    Vector3 vel;
    Vector3 targetPos;
    Vector2 lastDir = Vector2.up;

    void Awake()
    {
        if (skillShotSmall != null) targetPos = skillShotSmall.position;
    }

    public void UpdateAim(Vector2 inputDir, float power01)
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Vector2 dir = inputDir;

        if (dir.sqrMagnitude > deadzone * deadzone)
            lastDir = dir.normalized;
        else
            dir = lastDir;

        if (inputDir.sqrMagnitude < deadzone * deadzone && power01 < 0.01f)
        {
            if (enableGroundMove) SetAOECenter();
            return;
        }

        lastY = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;

        Vector3 worldDir = new Vector3(dir.x, 0f, dir.y);
        if (worldDir.sqrMagnitude > 0.0001f)
        {
            worldDir.Normalize();
            LastRotateDirWorld = worldDir;
        }

        if (enableRotateVisual && rotateVisual != null)
        {
            rotateVisual.rotation = Quaternion.Euler(0f, lastY + rotateOffsetY, 0f);

            Vector3 f = rotateVisual.forward;
            f.y = 0f;
            if (f.sqrMagnitude > 0.0001f)
            {
                f.Normalize();
                LastRotateDirWorld = f;
            }
        }

        if (enableGroundMove && skillShotSmall != null)
        {
            float range = GetRangeWorld();
            float dist = Mathf.Clamp01(power01) * range;

            Vector3 center = GetCenter();

            Vector3 right = cam.transform.right; right.y = 0f; right.Normalize();
            Vector3 up = cam.transform.up; up.y = 0f; up.Normalize();

            Vector3 offset = (right * dir.x + up * dir.y) * dist;
            targetPos = center + offset;
        }
    }

    void LateUpdate()
    {
        if (!enableGroundMove) return;
        if (skillShotSmall == null) return;

        skillShotSmall.position = Vector3.SmoothDamp(
            skillShotSmall.position,
            targetPos,
            ref vel,
            smoothTime,
            maxMoveSpeed,
            Time.unscaledDeltaTime
        );
    }

    void SetAOECenter()
    {
        if (skillShotSmall == null) return;
        targetPos = GetCenter();
    }

    Vector3 GetCenter()
    {
        return (skillShot != null) ? skillShot.position : transform.position;
    }

    float GetRangeWorld()
    {
        if (!autoRangeFromSkillShot || skillShot == null)
            return Mathf.Max(0f, maxRangeWorld - innerPaddingWorld);

        float worldW = skillShot.rect.width * skillShot.lossyScale.x;
        float worldH = skillShot.rect.height * skillShot.lossyScale.y;
        float r = Mathf.Min(worldW, worldH) * 0.5f;

        return Mathf.Max(0f, r - innerPaddingWorld);
    }

    public void ResetSmallToCenter()
    {
        if (skillShotSmall == null) return;

        Vector3 center = GetCenter();
        targetPos = center;
        skillShotSmall.position = center;
        vel = Vector3.zero;
    }

    public float GetAngleY() => lastY;

    public void SetAutoRotateWorldDir(Vector3 worldDir)
    {
        if (cam == null) cam = Camera.main;

        worldDir.y = 0f;
        if (worldDir.sqrMagnitude < 0.0001f) return;

        worldDir.Normalize();
        LastRotateDirWorld = worldDir;

        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        // Project worldDir lên basis camera (ground-plane)
        Vector3 right = cam.transform.right; right.y = 0f;
        Vector3 up = cam.transform.up; up.y = 0f;

        if (right.sqrMagnitude < 0.0001f || up.sqrMagnitude < 0.0001f) return;

        right.Normalize();
        up.Normalize();

        // worldDir -> (x,y) theo camera right/up
        float x = Vector3.Dot(worldDir, right);
        float y = Vector3.Dot(worldDir, up);

        Vector2 dir2 = new Vector2(x, y);
        if (dir2.sqrMagnitude > 0.0001f)
            lastDir = dir2.normalized;

        lastY = Mathf.Atan2(lastDir.x, lastDir.y) * Mathf.Rad2Deg;

        if (enableRotateVisual && rotateVisual != null)
        {
            rotateVisual.rotation = Quaternion.Euler(0f, lastY + rotateOffsetY, 0f);

            Vector3 f = rotateVisual.forward;
            f.y = 0f;
            if (f.sqrMagnitude > 0.0001f)
            {
                f.Normalize();
                LastRotateDirWorld = f;
            }
        }
    }

    public void SetAutoAOETargetWorld(Vector3 worldPos, bool snapImmediately = true)
    {
        if (skillShotSmall == null) return;

        worldPos.y = skillShotSmall.position.y;

        targetPos = worldPos;

        if (snapImmediately)
        {
            skillShotSmall.position = worldPos;
            vel = Vector3.zero;
        }
    }
}
