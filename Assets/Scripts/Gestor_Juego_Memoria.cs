using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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


    private Target primeraSeleccion;
    private Target segundaSeleccion;

    private int paresEncontrados = 0;
    private bool puedeSeleccionar = false;
    private int totalPares;
    private int nivelActual = 1;

    void Start()
    {
        if (objetoVictoria != null) objetoVictoria.SetActive(false);
        DesactivarTodasLasTarjetas();
        ConfigurarNivel(nivelActual);
    }

    void ConfigurarNivel(int nivel)
    {
        if (skyboxNivel1 != null) skyboxNivel1.SetActive(false);
        if (skyboxNivel2 != null) skyboxNivel2.SetActive(false);
        if (skyboxNivel3 != null) skyboxNivel3.SetActive(false);

        switch (nivel)
        {
            case 1:
                if (skyboxNivel1 != null) skyboxNivel1.SetActive(true);
                break;
            case 2:
                if (skyboxNivel2 != null) skyboxNivel2.SetActive(true);
                break;
            case 3:
                if (skyboxNivel3 != null) skyboxNivel3.SetActive(true);
                break;
        }

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
            paresEncontrados++;

            if (paresEncontrados == totalPares)
            {
                Debug.Log($"¡NIVEL {nivelActual} COMPLETADO!");
                StartCoroutine(TransicionDeNivel());
            }
        }
        else
        {
            primeraSeleccion.Voltear();
            segundaSeleccion.Voltear();

            // MODIFICACIÓN CLAVE: Espera hasta que ambas tarjetas hayan terminado de girar.
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
        yield return new WaitForSeconds(1.5f);

        foreach (var tarjeta in tarjetasActivasEnJuego)
        {
            tarjeta.ResetTarjeta();
            tarjeta.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(0.5f);

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
            if (objetoVictoria != null) objetoVictoria.SetActive(true);
        }
        else
        {
            Debug.Log($"Cargando Nivel {nivelActual}...");
            ConfigurarNivel(nivelActual);
        }
    }
}
