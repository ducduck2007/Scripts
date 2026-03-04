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
        public float atTime;
        public Pose pose;
        public float lerpTime;
    }

    public Pose defaultPose;

    public float enterDelay;
    public float returnAtTime;
    public float returnLerpTime;

    public bool loop = true;

    public ActionStep[] steps;
}