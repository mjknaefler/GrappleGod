using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public static Transform Current;
    public bool makeCurrentOnStart;

    void Start()
    {
        if (makeCurrentOnStart) Current = transform;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Current = transform;
        }
    }


}
