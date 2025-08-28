using System.Collections;
using UnityEngine;

public class Target : MonoBehaviour
{
  [SerializeField] private int value;
  [SerializeField] private float rotationDuration = 0.5f;

  [SerializeField] private GameObject audioSource;

  private bool isRotating = false;
  private bool isMoving = false;

  private Gestor_Juego_Memoria gestorJuego;

  public int Value { get => value; set => this.value = value; }
  public bool Selected { get; private set; } = false;
  // AÑADIDO: Propiedad pública para que el gestor sepa si la tarjeta está girando.
  public bool IsRotating { get => isRotating; }

  void Start()
  {
    gestorJuego = FindFirstObjectByType<Gestor_Juego_Memoria>();

    if (gestorJuego == null)
    {
      Debug.LogError("No se encontró el GestorJuego en la escena.");
    }
  }

  public void Seleccionar()
  {
    if (Selected || isRotating || isMoving)
    {
      return;
    }
    Debug.Log($"Tarjeta seleccionada con valor: {value}");

    if (gestorJuego.puedeSeleccionar)
    {
      if (audioSource != null)
      {
        Instantiate(audioSource, transform.position, Quaternion.identity);
      }

      Voltear();
      gestorJuego.TarjetaSeleccionada(this);
    }
  }

  public void Voltear()
  {
    Selected = !Selected;
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
    Selected = false;
    gameObject.SetActive(false);
  }

  // AÑADIDO: Método para que el Gestor reinicie la tarjeta de forma segura.
  public void ResetTarjeta()
  {
    Selected = false;
  }

  public void MoverHacia(Vector3 destino, float duracion, Transform puntoDeReferencia, Quaternion? rotacionFinal = null)
  {
    if (isMoving) return;
    StartCoroutine(MoveCoroutine(destino, duracion, puntoDeReferencia, rotacionFinal));
  }

  private IEnumerator MoveCoroutine(Vector3 destino, float duracion, Transform puntoDeReferencia, Quaternion? rotacionFinal)
  {
    isMoving = true;
    Vector3 startPosition = transform.position;
    Quaternion startRotation = transform.rotation;
    float elapsedTime = 0.0f;

    while (elapsedTime < duracion)
    {
      transform.position = Vector3.Lerp(startPosition, destino, elapsedTime / duracion);

      if (rotacionFinal.HasValue)
      {
        transform.rotation = Quaternion.Slerp(startRotation, rotacionFinal.Value, elapsedTime / duracion);
      }
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
    if (rotacionFinal.HasValue)
    {
      transform.rotation = rotacionFinal.Value;
    }
    isMoving = false;
  }
}
