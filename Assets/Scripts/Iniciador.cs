using UnityEngine;

public class Iniciador : MonoBehaviour
{
    [SerializeField] private GameObject objetoActivar;
    [SerializeField] private GameObject objetoDesactivar;

    public void ActivarMenu()
    {
        objetoDesactivar.SetActive(false);
        objetoActivar.SetActive(true);
    }
}
