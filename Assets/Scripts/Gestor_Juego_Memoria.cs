using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
// AÑADIDO: Necesario para poder usar TextMeshPro.
using TMPro;

public class Gestor_Juego_Memoria : MonoBehaviour
{
    [Header("Configuración de Tarjetas por Nivel")]
    [SerializeField] List<Target> tarjetasNivel1;
    [SerializeField] List<Target> tarjetasNivel2;
    [SerializeField] List<Target> tarjetasNivel3;

    private List<Target> tarjetasActivasEnJuego = new List<Target>();

    [Header("Configuración de Movimiento")]
    [Tooltip("Debe tener suficientes posiciones para el nivel más alto.")]
    [SerializeField] Transform[] posicionesFinales;
    [SerializeField] Transform posicionInicial;
    [SerializeField] Transform puntoDeReferenciaEspalda;
    [SerializeField] float delayInicial = 1f;
    [SerializeField] float delayEntreMovimientos = 0.5f;
    [SerializeField] float duracionMovimiento = 1f;
    [SerializeField] Vector3 rotacionInicialTarjetas = new Vector3(0, 90, 0);

    [Header("Lógica del Juego")]
    [SerializeField] float delayParaComprobar = 1f;
    [SerializeField] GameObject skyboxNivel1;
    [SerializeField] GameObject skyboxNivel2;
    [SerializeField] GameObject skyboxNivel3;
    [SerializeField] GameObject objetoVictoria;
    [SerializeField] GameObject audioNivelCompletado;
    [SerializeField] GameObject audioMatchFound;
    [SerializeField] GameObject audioInicial;
    [SerializeField] GameObject audioRayo;
    [SerializeField] AudioFade musicaJuego;
    // AÑADIDO: Tiempos para cada nivel y elementos de derrota.
    [SerializeField] private float[] tiempoPorNivel = { 60f, 120f, 180f };
    [SerializeField] private GameObject objetoDerrota;
    [SerializeField] private GameObject audioDerrota;

    // OBJETOS QUE SE DESACTIVAN
    [SerializeField] private GameObject menuActivable;
    [SerializeField] private OpacityTransition transicion;

    // AÑADIDO: Referencias para los elementos de la UI.
    [Header("Interfaz de Usuario (UI)")]
    [SerializeField] private TextMeshProUGUI textoTiempo;
    [SerializeField] private TextMeshProUGUI textoNivel;
    [SerializeField] private TextMeshProUGUI textoPares;
    [SerializeField] private float delayNivelCompletado = 3f;


    private Target primeraSeleccion;
    private Target segundaSeleccion;

    private int paresEncontrados = 0;
    public bool puedeSeleccionar = false;
    private int totalPares;
    private int nivelActual = 1;

    // MODIFICADO: La variable ahora es para el tiempo restante.
    private float tiempoRestante;
    private bool cronometroActivo = false;

    void Start()
    {
        if (objetoVictoria != null) objetoVictoria.SetActive(false);
        // AÑADIDO: Aseguramos que los objetos de derrota estén desactivados al inicio.
        if (objetoDerrota != null) objetoDerrota.SetActive(false);
        DesactivarTodasLasTarjetas();
        ConfigurarNivel(nivelActual);
        StartCoroutine(AudioInicial());
        if (musicaJuego != null)
        {
            musicaJuego.FadeIn();
        }
    }

    // MODIFICADO: El método Update ahora maneja una cuenta regresiva.
    void Update()
    {
        if (cronometroActivo)
        {
            tiempoRestante -= Time.deltaTime;

            // Si el tiempo llega a cero, se maneja la derrota.
            if (tiempoRestante <= 0)
            {
                tiempoRestante = 0;
                ManejarDerrota();
            }

            if (textoTiempo != null)
            {
                // MODIFICADO: Ahora solo muestra los segundos restantes, redondeados hacia arriba.
                textoTiempo.text = Mathf.CeilToInt(tiempoRestante).ToString();
            }
        }
    }

    private IEnumerator AudioInicial()
    {
        yield return new WaitForSeconds(3f);

        Instantiate(audioInicial, transform.position, Quaternion.identity);
    }

    void ConfigurarNivel(int nivel)
    {
        if (skyboxNivel1 != null) skyboxNivel1.SetActive(false);
        if (skyboxNivel2 != null) skyboxNivel2.SetActive(false);
        if (skyboxNivel3 != null) skyboxNivel3.SetActive(false);

        switch (nivel)
        {
            case 1: if (skyboxNivel1 != null) skyboxNivel1.SetActive(true); break;
            case 2: if (skyboxNivel2 != null) skyboxNivel2.SetActive(true); break;
            case 3: if (skyboxNivel3 != null) skyboxNivel3.SetActive(true); break;
        }

        transicion.FadeIn();
        transicion.FadeOut();

        textoTiempo.text = "-";
        paresEncontrados = 0;
        puedeSeleccionar = false;
        tarjetasActivasEnJuego.Clear();

        if (nivel >= 1) tarjetasActivasEnJuego.AddRange(tarjetasNivel1);
        if (nivel >= 2) tarjetasActivasEnJuego.AddRange(tarjetasNivel2);
        if (nivel >= 3) tarjetasActivasEnJuego.AddRange(tarjetasNivel3);

        foreach (var tarjeta in tarjetasActivasEnJuego)
        {
            tarjeta.gameObject.SetActive(true);
        }

        totalPares = tarjetasActivasEnJuego.Count / 2;

        if (textoNivel != null) textoNivel.text = $"NIVEL {nivelActual}";
        if (textoPares != null) textoPares.text = $"{paresEncontrados}/{totalPares}";

        cronometroActivo = false;
        // MODIFICADO: Se establece el tiempo inicial del nivel desde el arreglo.
        if (nivel - 1 < tiempoPorNivel.Length)
        {
            tiempoRestante = tiempoPorNivel[nivel - 1];
        }
        else
        {
            // Si no hay un tiempo definido para el nivel, usa el último disponible.
            tiempoRestante = tiempoPorNivel[tiempoPorNivel.Length - 1];
        }

        if (posicionesFinales.Length < tarjetasActivasEnJuego.Count)
        {
            Debug.LogError($"No hay suficientes 'posicionesFinales' ({posicionesFinales.Length}) para las tarjetas del nivel actual ({tarjetasActivasEnJuego.Count}). Revisa las listas en el Inspector.");
            return;
        }

        AsignarPosicionesIniciales();
        StartCoroutine(IniciarSecuenciaDeMovimiento());
    }

    void DesactivarTodasLasTarjetas()
    {
        var todasLasTarjetas = tarjetasNivel1.Concat(tarjetasNivel2).Concat(tarjetasNivel3);
        foreach (var tarjeta in todasLasTarjetas)
        {
            if (tarjeta != null) tarjeta.gameObject.SetActive(false);
        }
    }

    void AsignarPosicionesIniciales()
    {
        System.Random rnd = new System.Random();
        tarjetasActivasEnJuego = tarjetasActivasEnJuego.OrderBy(item => rnd.Next()).ToList();

        for (int i = 0; i < tarjetasActivasEnJuego.Count; i++)
        {
            tarjetasActivasEnJuego[i].transform.position = posicionInicial.position;
            tarjetasActivasEnJuego[i].transform.rotation = Quaternion.Euler(rotacionInicialTarjetas);
        }
    }

    private IEnumerator IniciarSecuenciaDeMovimiento()
    {
        yield return new WaitForSeconds(delayInicial);

        for (int i = 0; i < tarjetasActivasEnJuego.Count; i++)
        {
            tarjetasActivasEnJuego[i].MoverHacia(posicionesFinales[i].position, duracionMovimiento, puntoDeReferenciaEspalda, null);
            yield return new WaitForSeconds(delayEntreMovimientos);
        }

        yield return new WaitForSeconds(duracionMovimiento);
        Debug.Log($"Movimiento Nivel {nivelActual} completado. ¡Puedes empezar a jugar!");
        puedeSeleccionar = true;
        cronometroActivo = true;
    }

    public void TarjetaSeleccionada(Target tarjeta)
    {
        if (!puedeSeleccionar || primeraSeleccion == tarjeta) return;
        if (primeraSeleccion == null) primeraSeleccion = tarjeta;
        else
        {
            segundaSeleccion = tarjeta;
            puedeSeleccionar = false;
            StartCoroutine(ComprobarPareja());
        }
    }

    private IEnumerator ComprobarPareja()
    {
        yield return new WaitForSeconds(delayParaComprobar);

        if (primeraSeleccion.Value == segundaSeleccion.Value)
        {
            primeraSeleccion.Ocultar();
            segundaSeleccion.Ocultar();

            if (audioMatchFound != null)
            {
                Instantiate(audioMatchFound, transform.position, Quaternion.identity);
            }

            paresEncontrados++;
            if (textoPares != null) textoPares.text = $"{paresEncontrados}/{totalPares}";

            if (paresEncontrados == totalPares)
            {
                cronometroActivo = false;
                Debug.Log($"¡NIVEL {nivelActual} COMPLETADO!");
                StartCoroutine(TransicionDeNivel());
            }
        }
        else
        {
            primeraSeleccion.Voltear();
            segundaSeleccion.Voltear();
            yield return new WaitUntil(() => !primeraSeleccion.IsRotating && !segundaSeleccion.IsRotating);
        }

        primeraSeleccion = null;
        segundaSeleccion = null;

        if (paresEncontrados != totalPares)
        {
            puedeSeleccionar = true;
        }
    }

    private IEnumerator TransicionDeNivel()
    {
        yield return new WaitForSeconds(delayNivelCompletado);

        if (audioNivelCompletado != null)
        {
            Instantiate(audioNivelCompletado, transform.position, Quaternion.identity);
        }

        foreach (var tarjeta in tarjetasActivasEnJuego)
        {
            tarjeta.ResetTarjeta();
            tarjeta.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(0.5f);

        Instantiate(audioRayo, transform.position, Quaternion.identity);

        Debug.Log("Retirando tarjetas al punto de inicio...");
        foreach (var tarjeta in tarjetasActivasEnJuego)
        {
            tarjeta.MoverHacia(posicionInicial.position, duracionMovimiento, null, Quaternion.Euler(rotacionInicialTarjetas));
        }

        yield return new WaitForSeconds(duracionMovimiento + 0.5f);

        nivelActual++;

        if (nivelActual > 3)
        {
            Debug.Log("¡HAS GANADO TODO EL JUEGO!");

            foreach (var tarjeta in tarjetasActivasEnJuego)
            {
                tarjeta.Ocultar();
            }
            if (objetoVictoria != null)
            {
                if (musicaJuego != null)
                {
                    musicaJuego.FadeOut();
                }
                menuActivable.SetActive(false);
                objetoVictoria.SetActive(true);
            }
        }
        else
        {
            Debug.Log($"Cargando Nivel {nivelActual}...");
            ConfigurarNivel(nivelActual);
        }
    }

    // AÑADIDO: Nueva función para manejar la derrota.
    void ManejarDerrota()
    {
        cronometroActivo = false;
        puedeSeleccionar = false;
        Debug.Log("¡SE ACABÓ EL TIEMPO! HAS PERDIDO.");

        if (audioDerrota != null)
        {
            Instantiate(audioDerrota, transform.position, Quaternion.identity);
        }
        if (objetoDerrota != null) objetoDerrota.SetActive(true);

        // Ocultamos todas las tarjetas activas.
        foreach (var tarjeta in tarjetasActivasEnJuego)
        {
            tarjeta.Ocultar();
        }

        musicaJuego.FadeOut();
    }
}
