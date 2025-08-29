using UnityEngine;
// Es muy importante incluir este 'using' para poder trabajar con TextMeshPro.
using TMPro;

public class ActualizadorTextoUI : MonoBehaviour
{
    // Crea un campo en el Inspector de Unity para que arrastres
    // el objeto de texto que quieres controlar.
    [SerializeField]
    private TextMeshProUGUI textoParaActualizar;

    // AÑADIDO: Campo para escribir el texto directamente en el Inspector.
    // El atributo [TextArea] hace el campo de texto más grande para mayor comodidad.
    [SerializeField]
    private string textoAMostrar;

    public void ActualizarTexto()
    {
        // Primero, comprobamos si el campo de texto ha sido asignado
        // en el Inspector para evitar un error si está vacío.
        if (textoParaActualizar != null)
        {
            // Actualizamos la propiedad .text del componente TextMeshProUGUI.
            textoParaActualizar.text = textoAMostrar;
        }
        else
        {
            // Si no se asignó, mostramos un error en la consola para facilitar la depuración.
            Debug.LogError("El componente TextMeshProUGUI no ha sido asignado en el Inspector.");
        }
    }
}

