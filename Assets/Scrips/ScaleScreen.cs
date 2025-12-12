using UnityEngine;

public class ScaleScreen : MonoBehaviour
{
    public GameObject bg;
    public Transform tranScale;
    public Transform contentScale;
    
    public virtual void Start()
    {
        if (bg != null)
        {
            AgentUnity.ScaleBg(bg);
        }
        
        if (tranScale != null)
        {
            AgentUnity.ScaleTranform(tranScale);
        }
        
        if (contentScale != null)
        {
            AgentUnity.ScaleContent(tranScale);
        }
    }
}
