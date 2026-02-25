using UnityEngine;

public static class SkillCastProtocol33
{
    public const float SERVER_TO_UNITY_POS = 0.5f;
    public const float UNITY_TO_SERVER_POS = 2f;
    public const int DIR_INT_SCALE = 100;

    public const int TYPE_CIRCLE_AOE_1 = 2;
    public const int TYPE_CIRCLE_AOE_2 = 3;

    public const int TYPE_LINE_1 = 4;
    public const int TYPE_LINE_2 = 5;

    public const int TYPE_FAN_1 = 6;
    public const int TYPE_FAN_2 = 11;

    public static bool UsesDirection(int typeSkill)
        => typeSkill == TYPE_LINE_1 || typeSkill == TYPE_LINE_2 || typeSkill == TYPE_FAN_1 || typeSkill == TYPE_FAN_2;

    public static bool UsesTargetPos(int typeSkill)
        => typeSkill == TYPE_CIRCLE_AOE_1 || typeSkill == TYPE_CIRCLE_AOE_2;

    public static bool UsesRadius(int typeSkill) => UsesTargetPos(typeSkill);
    public static bool UsesMaxRange(int typeSkill) => UsesDirection(typeSkill);
    public static bool UsesAngle(int typeSkill) => typeSkill == TYPE_FAN_1 || typeSkill == TYPE_FAN_2;

    public static Vector3 ServerToUnityPos(int x, int y) => new Vector3(x * SERVER_TO_UNITY_POS, 0f, y * SERVER_TO_UNITY_POS);

    public static void UnityToServerPos(Vector3 pos, out int x, out int y)
    {
        x = Mathf.RoundToInt(pos.x * UNITY_TO_SERVER_POS);
        y = Mathf.RoundToInt(pos.z * UNITY_TO_SERVER_POS);
    }

    public static void DirToInt(Vector3 dir, out int dirX, out int dirY)
    {
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f)
        {
            dirX = 0; dirY = 0;
            return;
        }

        dir.Normalize();
        dirX = Mathf.Clamp(Mathf.RoundToInt(dir.x * DIR_INT_SCALE), -DIR_INT_SCALE, DIR_INT_SCALE);
        dirY = Mathf.Clamp(Mathf.RoundToInt(dir.z * DIR_INT_SCALE), -DIR_INT_SCALE, DIR_INT_SCALE);
    }

    public static Vector3 IntToDir(float rawDirX, float rawDirY)
    {
        bool looksNormalized = Mathf.Abs(rawDirX) <= 1.5f && Mathf.Abs(rawDirY) <= 1.5f;
        float dx = looksNormalized ? rawDirX : (rawDirX / DIR_INT_SCALE);
        float dz = looksNormalized ? rawDirY : (rawDirY / DIR_INT_SCALE);

        Vector3 d = new Vector3(dx, 0f, dz);
        if (d.sqrMagnitude < 0.0001f) return Vector3.zero;
        return d.normalized;
    }
}
