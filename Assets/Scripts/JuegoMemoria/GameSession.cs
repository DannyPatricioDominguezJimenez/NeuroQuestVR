using UnityEngine;

public static class GameSession
{
    public static int jugadorId;
    public static string jugadorNombre;

    public static int puntaje = 0;
    public static int errores = 0;
    public static float tiempoSegundos = 0f;

    // Resetear la sesión e inicializar con el jugador actual
    public static void Reset()
    {
        puntaje = 0;
        errores = 0;
        tiempoSegundos = 0f;

        if (PlayerManager.instance != null && PlayerManager.instance.jugadorActual != null)
        {
            jugadorId = PlayerManager.instance.jugadorActual.id;
            jugadorNombre = PlayerManager.instance.jugadorActual.nombre;
        }
    }
}
