using System.Collections;
using UnityEngine;

public class Target : MonoBehaviour
{
  [SerializeField] private int value;
  [SerializeField] private float rotationDuration = 0.5f;

  private bool selected = false;
  private bool isRotating = false;
  private bool isMoving = false;

  private Gestor_Juego_Memoria gestorJuego;

  public int Value { get => value; set => this.value = value; }
  public bool Selected { get => selected; set => this.selected = value; }

  void Start()
  {
    selected = false;
    gestorJuego = FindFirstObjectByType<Gestor_Juego_Memoria>();

    if (gestorJuego == null)
    {
      Debug.LogError("No se encontró el GestorJuego en la escena.");
    }
  }

  public void Seleccionar()
  {
    if (selected || isRotating || isMoving)
    {
      return;
    }
    Debug.Log($"Tarjeta seleccionada con valor: {value}");

    Voltear();
    gestorJuego.TarjetaSeleccionada(this);
  }

  public void Voltear()
  {
    selected = !selected;
    StartCoroutine(RotateCoroutine(new Vector3(0, 180, 0), rotationDuration));
  }

  private IEnumerator RotateCoroutine(Vector3 byAngles, float duration)
  {
    isRotating = true;
    Quaternion startRotation = transform.rotation;
    Quaternion endRotation = transform.rotation * Quaternion.Euler(byAngles);
    float elapsedTime = 0.0f;

    while (elapsedTime < duration)
    {
      transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsedTime / duration);
      elapsedTime += Time.deltaTime;
      yield return null;
    }

    transform.rotation = endRotation;
    isRotating = false;
  }

  public void Ocultar()
  {
    gameObject.SetActive(false);
  }

  // AÑADIDO: Un parámetro opcional para una rotación final específica.
  public void MoverHacia(Vector3 destino, float duracion, Transform puntoDeReferencia, Quaternion? rotacionFinal = null)
  {
    if (isMoving) return;
    // Le pasamos la nueva rotación final a la coroutine.
    StartCoroutine(MoveCoroutine(destino, duracion, puntoDeReferencia, rotacionFinal));
  }

  // MODIFICADO: La coroutine ahora maneja la rotación de dos maneras.
  private IEnumerator MoveCoroutine(Vector3 destino, float duracion, Transform puntoDeReferencia, Quaternion? rotacionFinal)
  {
    isMoving = true;
    Vector3 startPosition = transform.position;
    // Guardamos la rotación inicial del movimiento.
    Quaternion startRotation = transform.rotation;
    float elapsedTime = 0.0f;

    while (elapsedTime < duracion)
    {
      transform.position = Vector3.Lerp(startPosition, destino, elapsedTime / duracion);

      // Si se especificó una rotación final, rotamos suavemente hacia ella.
      if (rotacionFinal.HasValue)
      {
        transform.rotation = Quaternion.Slerp(startRotation, rotacionFinal.Value, elapsedTime / duracion);
      }
      // Si no, usamos la lógica anterior de "dar la espalda".
      else if (puntoDeReferencia != null)
      {
        Vector3 direccion = transform.position - puntoDeReferencia.position;
        direccion.y = 0;
        transform.rotation = Quaternion.LookRotation(direccion);
      }

      elapsedTime += Time.deltaTime;
      yield return null;
    }

    transform.position = destino;
    // Si hay una rotación final, nos aseguramos de que termine exactamente en esa rotación.
    if (rotacionFinal.HasValue)
    {
      transform.rotation = rotacionFinal.Value;
    }
    isMoving = false;
  }
}
