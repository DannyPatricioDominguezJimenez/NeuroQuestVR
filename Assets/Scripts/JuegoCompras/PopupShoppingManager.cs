using UnityEngine;
using TMPro;

// Asegúrate de que el nombre del archivo y la clase coincidan
public class PopupShoppingManager : MonoBehaviour
{
    [Header("Popup UI")]
    public GameObject popupCanvas; // Panel que se activa/desactiva
    public TMP_Text nombreText;
    public TMP_Text puntajeText;
    public TMP_Text erroresText;
    public TMP_Text tiempoText;

    // Este método es llamado por ShoppingGameVR.cs
    public void MostrarPopup(int puntaje, int errores, float tiempo)
    {
        // 1. Mostrar el nombre del jugador (lógica de tu ejemplo anterior)
        if (PlayerManager.instance != null && PlayerManager.instance.jugadorActual != null)
        {
            // Asume que PlayerManager.instance.jugadorActual.nombre existe
            nombreText.text = "Jugador: " + PlayerManager.instance.jugadorActual.nombre;
        }
        else
        {
            nombreText.text = "Jugador: Desconocido";
        }

        // 2. Asignar los valores del juego
        puntajeText.text = "Puntaje: " + puntaje;
        erroresText.text = "Errores: " + errores;
        // Formato a un decimal, como en tu ejemplo anterior
        tiempoText.text = "Tiempo: " + tiempo.ToString("F1") + "s";

        // 3. ¡El paso clave! Activar el Canvas/Panel para que se vea
        if (popupCanvas != null)
        {
            popupCanvas.SetActive(true);
        }
        else
        {
            Debug.LogError("PopupCanvas NO ASIGNADO. Asigna el GameObject del panel en el Inspector.");
        }
    }
}