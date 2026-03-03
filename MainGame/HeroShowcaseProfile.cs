using UnityEngine;

[System.Serializable]
public class HeroShowcaseProfile
{
    [System.Serializable]
    public struct Pose
    {
        public Vector3 localPosition;
        public Vector3 localEuler;
        public float uniformScale;
    }

    [System.Serializable]
    public struct ActionStep
    {
        [Tooltip("Thời điểm bắt đầu step (giây)")]
        public float atTime;

        [Header("Hero Transform")]
        public Pose heroPose;

        [Header("Camera Transform")]
        public Pose cameraPose;

        [Tooltip("Thời gian lerp sang pose này")]
        public float lerpTime;
    }

    [Header("Default Hero Pose")]
    public Pose defaultHeroPose;

    [Header("Default Camera Pose")]
    public Pose defaultCameraPose;

    [Header("Sequence Settings")]
    public float enterDelay;
    public float returnAtTime;
    public float returnLerpTime;

    public bool loop = true;

    public ActionStep[] steps;
}