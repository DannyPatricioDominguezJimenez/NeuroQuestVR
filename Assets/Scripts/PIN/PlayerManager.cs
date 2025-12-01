using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    public Jugador jugadorActual;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // persiste entre escenas
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
