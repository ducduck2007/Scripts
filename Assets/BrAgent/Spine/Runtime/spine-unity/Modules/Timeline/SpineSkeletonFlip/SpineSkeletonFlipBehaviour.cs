#if UNITY_2017 || UNITY_2018
using System;
using UnityEngine.Playables;

[Serializable]
public class SpineSkeletonFlipBehaviour : PlayableBehaviour {
    public bool flipX, flipY;
}
#endif