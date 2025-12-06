using UnityEngine;
using System;
using DG.Tweening;

public class MathParabola :MonoBehaviour
{
    public static bool isMoving;
    
    public static Vector3 Parabola(Vector3 start, Vector3 end, float height,float t)
    {
        Func<float, float> f = x => -4 * height * x * x + 4 * height * x;
        var mid = Vector3.Lerp(start, end, t);
        if (t >= 0.998F)
        {
            isMoving = false;
            return start;
        }

        isMoving = true;
        return  new Vector3(mid.x,f(t)+Mathf.Lerp(start.y,end.y,t),mid.z);
    }
    
}