using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Text;

[System.Serializable]
public class SesionDataGlobos
{
    public int jugador_id;
    public string minijuego;
    public int puntuacion;
    public int duracion_segundos;
    public int fallos;
}

public class BalloonGameManager : MonoBehaviour
{
    public static BalloonGameManager Instance;

    [Header("Prefabs & UI")]
    public GameObject balloonPrefab;
    public RectTransform panelJuego;
    public TMP_Text scoreText;
    public TMP_Text mistakesText;
    public TMP_Text timerText;

    [Header("Popup Final")]
    public PopupManagerGlobos popupManager;

    [Header("Audio")]
    public AudioSource bgMusicSource;
    public AudioSource popAudioSource;

    [Header("Configuración del juego")]
    public int totalBalloons = 30;
    public float spawnInterval = 1f;
    public float initialSpeed = 100f;
    public float speedIncrease = 20f;
    public int maxSpawnAttempts = 20;
    public float graceTime = 2f;

    private int popped = 0;
    private int missed = 0;
    private float timer = 0f;
    private bool gameActive = false;

    void Awake() => Instance = this;

    void Start()
    {
        if (bgMusicSource != null)
            bgMusicSource.Play();

        scoreText.text = "Puntos: 0";
        mistakesText.text = "Fallos: 0";
        timerText.text = "Tiempo: 0.0s";

        if (popupManager != null && popupManager.popupCanvas != null)
            popupManager.popupCanvas.SetActive(false);

        StartCoroutine(GameFlow());
    }

    IEnumerator GameFlow()
    {
        yield return new WaitForSeconds(graceTime);

        gameActive = true;
        StartCoroutine(SpawnBalloons());

        while (gameActive)
        {
            timer += Time.deltaTime;
            timerText.text = "Tiempo: " + timer.ToString("F1") + "s";
            yield return null;
        }

        if (bgMusicSource != null)
            bgMusicSource.Stop();

        if (popupManager != null)
            popupManager.MostrarPopup(popped, missed, timer);

        StartCoroutine(GuardarSesion());
    }

    IEnumerator SpawnBalloons()
    {
        for (int i = 0; i < totalBalloons; i++)
        {
            SpawnBalloon();
            yield return new WaitForSeconds(spawnInterval);
            spawnInterval = Mathf.Max(0.25f, spawnInterval - 0.02f);
        }

        while (popped + missed < totalBalloons)
            yield return null;

        gameActive = false;
    }

    void SpawnBalloon()
    {
        Vector2 pos = GetNonOverlappingPosition();
        GameObject b = Instantiate(balloonPrefab, panelJuego);
        RectTransform rt = b.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;

        BalloonButton balloon = b.GetComponent<BalloonButton>();
        balloon.speed = initialSpeed + (popped * speedIncrease / totalBalloons);
        balloon.popClip = popAudioSource != null ? popAudioSource.clip : null;
        balloon.manager = this;
    }

    Vector2 GetNonOverlappingPosition()
    {
        Vector2 pos = Vector2.zero;
        int attempts = 0;

        float width = panelJuego.rect.width;
        float height = panelJuego.rect.height;
        float startY = -height / 2f - 50f;

        while (attempts < maxSpawnAttempts)
        {
            pos = new Vector2(
                Random.Range(-width / 2f + 50f, width / 2f - 50f),
                startY
            );

            bool overlap = false;
            foreach (Transform child in panelJuego)
            {
                RectTransform rt = child.GetComponent<RectTransform>();
                if (Vector2.Distance(rt.anchoredPosition, pos) < 100f)
                {
                    overlap = true;
                    break;
                }
            }

            if (!overlap) break;
            attempts++;
        }

        return pos;
    }

    public void BalloonPopped()
    {
        popped++;
        scoreText.text = "Puntos: " + popped;
    }

    public void BalloonMissed()
    {
        missed++;
        mistakesText.text = "Fallos: " + missed;
    }

    IEnumerator GuardarSesion()
    {
        if (PlayerManager.instance == null || PlayerManager.instance.jugadorActual == null)
        {
            Debug.LogWarning("⚠️ No hay jugador activo para guardar sesión.");
            yield break;
        }

        string url = "http://localhost/neuroquest/api/guardar_sesion.php";

        SesionDataGlobos data = new SesionDataGlobos()
        {
            jugador_id = PlayerManager.instance.jugadorActual.id,
            minijuego = "globos",
            puntuacion = popped,
            duracion_segundos = Mathf.RoundToInt(timer),
            fallos = missed
        };

        string json = JsonUtility.ToJson(data);
        Debug.Log("📤 Enviando datos a la API: " + json);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Error guardando sesión: " + request.error);
        }
        else
        {
            Debug.Log("✅ Sesión guardada correctamente: " + request.downloadHandler.text);
        }
    }
}
