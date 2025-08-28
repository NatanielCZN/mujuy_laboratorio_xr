using UnityEngine;
using UnityEngine.SceneManagement; // 👈 necesario para manejar escenas

public class Scenes : MonoBehaviour
{
    // Método para volver al menú principal (suponiendo que está en el índice 0 del Build Settings)
    public void VolverAlMenu()
    {
        SceneManager.LoadScene(0);
    }

    // Método para volver a cargar una escena pasada por parámetro (por índice en Build Settings)
    public void VolverAEscena(int indiceEscena)
    {
        SceneManager.LoadScene(indiceEscena);
    }
}
