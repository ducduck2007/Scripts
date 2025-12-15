using UnityEngine;

public class ScaleScreen : MonoBehaviour
{
    public GameObject bg;
    public Transform tranScale;
    public Transform contentScale;
    
    protected virtual void OnEnable()
    {
        Scale();
    }

    protected virtual void Start()
    {
        // Nếu muốn chỉ scale khi enable thì có thể bỏ Start
    }

    protected void Scale()
    {
        if (bg != null)
            AgentUnity.ScaleBg(bg);

        if (tranScale != null)
            AgentUnity.ScaleTranform(tranScale);

        if (contentScale != null)
            AgentUnity.ScaleContent(contentScale);
    }
}
