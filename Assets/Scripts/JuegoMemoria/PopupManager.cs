using UnityEngine;
using TMPro;

public class PopupManager : MonoBehaviour
{
    [Header("Popup UI")]
    public GameObject popupCanvas; // panel que creaste
    public TMP_Text nombreText;
    public TMP_Text puntajeText;
    public TMP_Text erroresText;
    public TMP_Text tiempoText;

    public void MostrarPopup(int puntaje, int errores, float tiempo)
    {
        if (PlayerManager.instance != null && PlayerManager.instance.jugadorActual != null)
        {
            nombreText.text = "Jugador: " + PlayerManager.instance.jugadorActual.nombre;
        }
        else
        {
            nombreText.text = "Jugador: Desconocido";
        }

        puntajeText.text = "Puntaje: " + puntaje;
        erroresText.text = "Errores: " + errores;
        tiempoText.text = "Tiempo: " + tiempo.ToString("F1") + "s";

        popupCanvas.SetActive(true);
    }
}
