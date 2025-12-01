[System.Serializable]
public class Jugador
{
    public int id;
    public int cuidador_id;
    public string nombre;
    public int edad;
    public string genero;
    public string discapacidad;
    public string pin_codigo;
}

public class JugadorResponse
{
    public bool success;
    public Jugador jugador;
    public string message;
}
