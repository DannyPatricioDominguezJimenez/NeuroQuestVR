using UnityEngine;
using UnityEngine.UI;

public class BalloonButton : MonoBehaviour
{
    public float speed = 100f;
    public AudioClip popClip;
    public BalloonGameManager manager;

    private RectTransform rt;
    private Button button;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        button = GetComponent<Button>();
        button.onClick.AddListener(PopBalloon);
    }

    void Update()
    {
        if (manager != null && manager.panelJuego != null)
        {
            rt.anchoredPosition += Vector2.up * speed * Time.deltaTime;

            // Si sale del panel, cuenta como error
            if (rt.anchoredPosition.y > manager.panelJuego.rect.height / 2f)
            {
                manager.BalloonMissed();
                Destroy(gameObject);
            }
        }
    }

    void PopBalloon()
    {
        if (popClip != null && manager.popAudioSource != null)
            manager.popAudioSource.PlayOneShot(popClip);

        if (manager != null)
            manager.BalloonPopped();

        Destroy(gameObject);
    }
}
