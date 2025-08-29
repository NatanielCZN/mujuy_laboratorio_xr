using UnityEngine;
using System.Collections; // Necesario para las Coroutines

public class OpacityTransition : MonoBehaviour
{
    [SerializeField] private float transitionDuration = 2.0f; // Duración de la transición en segundos
    private Renderer sphereRenderer;
    private Material sphereMaterial;

    void Start()
    {
        sphereRenderer = GetComponent<Renderer>();
        sphereMaterial = sphereRenderer.material; // Obtener la instancia del material del objeto
    }

    // Método para hacer que la esfera aparezca (de 0 a 1 de opacidad)
    public void FadeIn()
    {
        // Detener cualquier coroutine de fade que esté activa para evitar conflictos
        StopAllCoroutines();
        StartCoroutine(ChangeOpacity(0f, 1f));
    }

    // Método para hacer que la esfera desaparezca (de 1 a 0 de opacidad)
    public void FadeOut()
    {
        // Detener cualquier coroutine de fade que esté activa
        StopAllCoroutines();
        StartCoroutine(ChangeOpacity(1f, 0f));
    }

    // Coroutine genérica para cambiar la opacidad
    IEnumerator ChangeOpacity(float startOpacity, float endOpacity)
    {
        float currentTime = 0f;
        Color materialColor = sphereMaterial.color; // Obtener el color actual del material

        while (currentTime < transitionDuration)
        {
            currentTime += Time.deltaTime;
            float normalizedTime = currentTime / transitionDuration; // Valor de 0 a 1

            // Interpolamos la opacidad
            materialColor.a = Mathf.Lerp(startOpacity, endOpacity, normalizedTime);
            sphereMaterial.color = materialColor; // Asignar el nuevo color con la opacidad actualizada

            yield return null; // Esperar al siguiente frame
        }

        // Asegurarse de que la opacidad final sea exactamente la deseada
        materialColor.a = endOpacity;
        sphereMaterial.color = materialColor;
    }
}