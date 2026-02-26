using UnityEngine;

public class DestroyGameObject : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float timeDt;
    void Start()
    {
        Invoke("vvDesTroy", timeDt);
    }

    private void vvDesTroy()
    {
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
