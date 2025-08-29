using UnityEngine;
using System.Collections;

public class AudioFade : MonoBehaviour
{
    public float fadeDuration = 3.0f;
    public float targetVolume = 0.5f;
    private AudioSource audioSource;

    void Awake() // Cambiamos Start por Awake para asegurar que la referencia esté lista antes
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.volume = 0f; // Asegurarse de que el volumen inicial es 0
        }
    }

    // Método para iniciar la música y subir el volumen
    public void FadeIn()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play(); // <--- CAMBIO CLAVE: Inicia la reproducción
            StartCoroutine(StartFade(0f, targetVolume));
        }
    }

    // Método para bajar el volumen y detener la música
    public void FadeOut()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            StartCoroutine(StartFade(audioSource.volume, 0f));
        }
    }

    private IEnumerator StartFade(float startVolume, float endVolume)
    {
        float currentTime = 0f;

        // No necesitamos StopAllCoroutines() aquí si la lógica de isPlaying nos protege

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            float newVolume = Mathf.Lerp(startVolume, endVolume, currentTime / fadeDuration);
            audioSource.volume = newVolume;
            yield return null;
        }

        audioSource.volume = endVolume;

        // <--- CAMBIO CLAVE: Si el fade out terminó, detenemos la música
        if (endVolume == 0f)
        {
            audioSource.Stop();
        }
    }
}