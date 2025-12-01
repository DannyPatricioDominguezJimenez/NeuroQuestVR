using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Text;

[System.Serializable]
public class SesionData
{
    public int jugador_id;
    public string minijuego;
    public int puntuacion;
    public int duracion_segundos;
    public int fallos;
}

public class MemoryGameVR : MonoBehaviour
{
    [Header("Cartas")]
    public List<Sprite> cardFronts;
    public Sprite cardBack;
    public List<Button> cardButtons;

    [Header("UI")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI mistakesText;
    public TextMeshProUGUI pointsText;

    [Header("Animacion")]
    public float flipDuration = 0.3f;

    [Header("Popup")]
    public PopupManager popupManager;

    [Header("Sonidos")]
    public AudioSource audioSource;
    public AudioClip clickSound;
    public AudioClip matchSound;
    public AudioClip errorSound;

    [Header("Música de Fondo")]
    public AudioSource musicSource;
    public AudioClip backgroundMusic;

    private int[] cardValues;
    private bool[] matched;
    private int firstCard = -1;
    private int secondCard = -1;
    private bool canClick = true;
    private bool gameOver = false;

    private float timer = 0f;
    private int mistakes = 0;
    private int points = 0;

    void OnEnable()
    {
        StartGame();
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    void OnDisable()
    {
        if (musicSource != null) musicSource.Stop();
    }

    void Update()
    {
        if (!gameOver)
        {
            timer += Time.deltaTime;
            timerText.text = "Tiempo: " + timer.ToString("F1") + "s";
            GameSession.tiempoSegundos = timer;
        }
    }

    public void StartGame()
    {
        timer = 0f;
        mistakes = 0;
        points = 0;
        gameOver = false;

        mistakesText.text = "Fallos: 0";
        pointsText.text = "Puntos: 0";

        firstCard = -1;
        secondCard = -1;
        canClick = true;

        GameSession.Reset();

        int pairCount = cardButtons.Count / 2;
        cardValues = new int[cardButtons.Count];
        matched = new bool[cardButtons.Count];

        for (int i = 0; i < pairCount; i++)
        {
            cardValues[i * 2] = i;
            cardValues[i * 2 + 1] = i;
        }

        for (int i = 0; i < cardValues.Length; i++)
        {
            int rnd = Random.Range(i, cardValues.Length);
            int temp = cardValues[i];
            cardValues[i] = cardValues[rnd];
            cardValues[rnd] = temp;
        }

        for (int i = 0; i < cardButtons.Count; i++)
        {
            int index = i;
            cardButtons[i].gameObject.SetActive(true);
            cardButtons[i].image.sprite = cardBack;
            cardButtons[i].transform.localRotation = Quaternion.identity;
            cardButtons[i].onClick.RemoveAllListeners();
            cardButtons[i].onClick.AddListener(() => OnCardClick(index));
            cardButtons[i].interactable = true;
            matched[i] = false;
        }
    }

    void OnCardClick(int index)
    {
        if (!canClick || matched[index] || index == firstCard) return;

        if (clickSound != null && audioSource != null) audioSource.PlayOneShot(clickSound);
        StartCoroutine(FlipCard(index, cardFronts[cardValues[index]]));

        if (firstCard == -1)
        {
            firstCard = index;
        }
        else
        {
            secondCard = index;
            canClick = false;
            StartCoroutine(CheckPair());
        }
    }

    IEnumerator FlipCard(int index, Sprite newSprite)
    {
        Button btn = cardButtons[index];
        float time = 0f;
        Quaternion startRotation = btn.transform.localRotation;
        Quaternion midRotation = Quaternion.Euler(0f, 90f, 0f);
        Quaternion endRotation = Quaternion.identity;

        while (time < flipDuration)
        {
            btn.transform.localRotation = Quaternion.Slerp(startRotation, midRotation, time / flipDuration);
            time += Time.deltaTime;
            yield return null;
        }
        btn.transform.localRotation = midRotation;
        btn.image.sprite = newSprite;

        time = 0f;
        while (time < flipDuration)
        {
            btn.transform.localRotation = Quaternion.Slerp(midRotation, endRotation, time / flipDuration);
            time += Time.deltaTime;
            yield return null;
        }
        btn.transform.localRotation = endRotation;
    }

    IEnumerator CheckPair()
    {
        yield return new WaitForSeconds(flipDuration * 2 + 0.2f);

        if (cardValues[firstCard] == cardValues[secondCard])
        {
            if (matchSound != null && audioSource != null) audioSource.PlayOneShot(matchSound);

            cardButtons[firstCard].gameObject.SetActive(false);
            cardButtons[secondCard].gameObject.SetActive(false);
            matched[firstCard] = true;
            matched[secondCard] = true;

            points++;
            pointsText.text = "Puntos: " + points;
            GameSession.puntaje = points;
        }
        else
        {
            if (errorSound != null && audioSource != null) audioSource.PlayOneShot(errorSound);

            StartCoroutine(FlipCard(firstCard, cardBack));
            StartCoroutine(FlipCard(secondCard, cardBack));

            mistakes++;
            mistakesText.text = "Fallos: " + mistakes;
            GameSession.errores = mistakes;
        }

        firstCard = -1;
        secondCard = -1;
        canClick = true;

        bool finished = true;
        for (int i = 0; i < matched.Length; i++)
        {
            if (!matched[i])
            {
                finished = false;
                break;
            }
        }

        if (finished && !gameOver)
        {
            gameOver = true;
            canClick = false;

            // Mostrar popup inmediatamente
            if (popupManager != null)
            {
                popupManager.MostrarPopup(points, mistakes, timer);
            }

            // Guardar sesión en la API usando SesionData
            StartCoroutine(GuardarSesion());
        }
    }

    IEnumerator GuardarSesion()
    {
        if (PlayerManager.instance == null || PlayerManager.instance.jugadorActual == null)
        {
            Debug.LogWarning("No hay jugador en memoria para guardar sesión");
            yield break;
        }

        string url = "http://localhost/neuroquest/api/guardar_sesion.php";

        SesionData data = new SesionData()
        {
            jugador_id = PlayerManager.instance.jugadorActual.id,
            minijuego = "memoria",
            puntuacion = GameSession.puntaje,
            duracion_segundos = Mathf.RoundToInt(GameSession.tiempoSegundos),
            fallos = GameSession.errores
        };

        string json = JsonUtility.ToJson(data);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error guardando sesión: " + request.error);
        }
        else
        {
            Debug.Log("Sesión guardada correctamente: " + request.downloadHandler.text);
        }
    }
}
