using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class LoginJugador : MonoBehaviour
{
    [Header("UI Canvas")]
    public GameObject PIN_Canvas;
    public GameObject SaludoJugador;
    public GameObject NoEncontroJugador;
    public GameObject ErrorConexion;

    [Header("UI Elements")]
    public TMP_InputField pinInput;
    public Button ingresarButton;
    public TMP_Text saludoTexto; // TMP dentro de SaludoJugador

    [Header("API")]
    public string apiUrl = "http://localhost/neuroquest/api/validar_pin.php";

    private void Start()
    {
        ingresarButton.onClick.AddListener(() =>
        {
            StartCoroutine(ValidarPinCoroutine());
        });
    }

    private IEnumerator ValidarPinCoroutine()
    {
        string pin = pinInput.text.Trim();

        if (string.IsNullOrEmpty(pin))
        {
            Debug.Log("Ingresa un PIN");
            yield break;
        }

        // JSON a enviar
        string jsonData = "{\"pin\":\"" + pin + "\"}";

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        // Ocultar canvas de PIN siempre
        PIN_Canvas.SetActive(false);

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error de conexión: " + request.error);
            ErrorConexion.SetActive(true);
            yield break;
        }

        string respuesta = request.downloadHandler.text;
        Debug.Log("Respuesta API: " + respuesta);

        // Verificar si success es true
        if (respuesta.Contains("\"success\":true"))
        {
            // Parsear toda la respuesta
            JugadorResponse jugadorResponse = JsonUtility.FromJson<JugadorResponse>(respuesta);

            // Guardar en PlayerManager
            PlayerManager.instance.jugadorActual = jugadorResponse.jugador;

            // Mostrar saludo
            SaludoJugador.SetActive(true);
            saludoTexto.text = "¡Hola, " + jugadorResponse.jugador.nombre + "!";

            Debug.Log($"✅ Jugador guardado en memoria: {jugadorResponse.jugador.nombre} (ID: {jugadorResponse.jugador.id})");
        }
        else if (respuesta.Contains("\"message\":\"PIN incorrecto") || respuesta.Contains("\"success\":false"))
        {
            NoEncontroJugador.SetActive(true);
        }
        else
        {
            ErrorConexion.SetActive(true);
        }
    }

    private string ParseNombre(string json)
    {
        string key = "\"nombre\":\"";
        int start = json.IndexOf(key);
        if (start == -1) return "Jugador";
        start += key.Length;
        int end = json.IndexOf("\"", start);
        if (end == -1) return "Jugador";
        return json.Substring(start, end - start);
    }

    private int ParseId(string json)
    {
        string key = "\"id\":";
        int start = json.IndexOf(key);
        if (start == -1) return 0;
        start += key.Length;
        int end = json.IndexOf(",", start);
        if (end == -1) end = json.IndexOf("}", start);
        string idStr = json.Substring(start, end - start).Trim();
        int id;
        if (int.TryParse(idStr, out id)) return id;
        return 0;
    }
}
