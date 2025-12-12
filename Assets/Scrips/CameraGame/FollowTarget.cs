using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    [SerializeField]
    private Transform Target;
    [SerializeField]
    private Vector3 Offset;

    private void LateUpdate()
    {
        transform.position = Target.position + Offset;
    }
}
