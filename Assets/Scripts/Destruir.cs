using UnityEngine;

public class Destruir : MonoBehaviour
{
    [SerializeField] private float delay = 2f;

    void Start()
    {
        Destroy(gameObject, delay);
    }
}
