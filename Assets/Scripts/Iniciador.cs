using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class Iniciador : MonoBehaviour
{
    [SerializeField] private GameObject objetoActivar;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private float delay = 3f;
    [SerializeField] private OpacityTransition transicion;

    private void Start()
    {
        if (videoPlayer != null)
        {
            StartCoroutine(IniciarConDelay());
        }
    }

    private IEnumerator IniciarConDelay()
    {
        // Espera el delay configurado
        yield return new WaitForSeconds(delay);

        transicion.FadeOut();

        // Se suscribe al evento de fin del video
        videoPlayer.loopPointReached += OnVideoEnd;

        // Reproduce el video
        videoPlayer.Play();
    }

    // Este método se ejecuta automáticamente cuando el video termina
    private void OnVideoEnd(VideoPlayer vp)
    {
        ActivarMenu();
    }

    // Método que activa/desactiva objetos
    public void ActivarMenu()
    {
        transicion.FadeIn();
        transicion.FadeOut();
        videoPlayer.gameObject.SetActive(false);
        objetoActivar.SetActive(true);
    }
}
