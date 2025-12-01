using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Text;

[System.Serializable]
public class Producto
{
    public string nombre;
    public Sprite imagen;
}

[System.Serializable]
public class SesionDataShopping
{
    public int jugador_id;
    public string minijuego;
    public int puntuacion;
    public int duracion_segundos;
    public int fallos;
}

public class ShoppingGameVR : MonoBehaviour
{
    [Header("Productos disponibles (los que tú tienes en el panel)")]
    public List<Button> botonesProductos = new List<Button>();
    public List<Producto> productos = new List<Producto>();

    [Header("UI Elementos del Juego")]
    public TextMeshProUGUI puntosText;
    public TextMeshProUGUI erroresText;
    public TextMeshProUGUI tiempoText;

    // ********** NUEVOS CAMPOS ESTATICOS **********
    [Header("Listado Estático de Compras")]
    public TextMeshProUGUI tituloListadoText; // Para el título "Listado de Productos"
    // Máximo 6 ítems, como solicitaste. Si necesitas más, agrégalos aquí.
    public List<TextMeshProUGUI> itemsListadoTMP = new List<TextMeshProUGUI>(6);
    // *********************************************

    [Header("Paneles")]
    public PopupShoppingManager popupManager;

    // Se elimina la Transform panelListado y GameObject baseItemListado.

    private List<Producto> listaDeCompras = new List<Producto>();
    // Usamos el nombre del producto como clave para un chequeo rápido
    private HashSet<string> productosMarcados = new HashSet<string>();

    private float tiempo = 0f;
    private int puntos = 0;
    private int errores = 0;
    private bool juegoTerminado = false;

    void OnEnable()
    {
        IniciarJuego();
    }

    void Update()
    {
        if (!juegoTerminado)
        {
            tiempo += Time.deltaTime;
            tiempoText.text = "Tiempo: " + tiempo.ToString("F1") + "s";
            GameSession.tiempoSegundos = tiempo;
        }
    }

    public void IniciarJuego()
    {
        // ... (Reset de variables) ...
        puntos = 0;
        errores = 0;
        tiempo = 0;
        juegoTerminado = false;
        productosMarcados.Clear();

        puntosText.text = "Puntos: 0";
        erroresText.text = "Fallos: 0";
        GameSession.Reset();

        // ********** CONFIGURACIÓN ESTÁTICA **********
        // 1. Título
        if (tituloListadoText != null)
        {
            tituloListadoText.text = "🛒 **Listado de Productos**"; // Título visible y destacado
        }

        // 2. Selección de productos aleatorios (Máximo 6)
        listaDeCompras = new List<Producto>(productos);
        listaDeCompras.Shuffle();
        // Usamos el mínimo entre 6 (el número de textos disponibles) y los productos reales
        int numItemsToShow = Mathf.Min(itemsListadoTMP.Count, listaDeCompras.Count);
        listaDeCompras = listaDeCompras.GetRange(0, numItemsToShow);

        // 3. Crear listado visual estático
        for (int i = 0; i < itemsListadoTMP.Count; i++)
        {
            TextMeshProUGUI itemTMP = itemsListadoTMP[i];

            if (i < listaDeCompras.Count)
            {
                // Producto a listar
                Producto producto = listaDeCompras[i];
                itemTMP.gameObject.SetActive(true);
                itemTMP.text = producto.nombre;
                itemTMP.color = Color.white;
                // **IMPORTANTE**: Quita el tag Strikethrough al inicio
                itemTMP.fontStyle = FontStyles.Normal;
            }
            else
            {
                // Ocultar textos estáticos no usados
                itemTMP.gameObject.SetActive(false);
            }
        }
        // *********************************************

        // Configurar botones (La lógica sigue siendo la misma)
        for (int i = 0; i < botonesProductos.Count && i < productos.Count; i++)
        {
            var boton = botonesProductos[i];
            var producto = productos[i];

            Image img = boton.GetComponentInChildren<Image>();
            if (img != null && producto.imagen != null)
                img.sprite = producto.imagen;

            TMP_Text txt = boton.GetComponentInChildren<TMP_Text>();
            if (txt != null)
                txt.text = producto.nombre;

            boton.onClick.RemoveAllListeners();
            boton.onClick.AddListener(() => OnProductoClick(producto));
        }
    }

    void OnProductoClick(Producto producto)
    {
        if (juegoTerminado) return;

        // Caso 1: producto está en la lista
        if (listaDeCompras.Exists(p => p.nombre == producto.nombre))
        {
            // Ya marcado → error
            if (productosMarcados.Contains(producto.nombre))
            {
                errores++;
                erroresText.text = "Fallos: " + errores;
                GameSession.errores = errores;
                return;
            }

            // Producto correcto: Marcar con Strikethrough

            // 1. Encuentra el TMP_Text estático que corresponde a este producto
            TextMeshProUGUI itemTMP = null;
            for (int i = 0; i < listaDeCompras.Count; i++)
            {
                if (listaDeCompras[i].nombre == producto.nombre)
                {
                    itemTMP = itemsListadoTMP[i];
                    break;
                }
            }

            if (itemTMP != null)
            {
                // **NUEVA LÓGICA DE MARCADO: STRIKETHROUGH**
                itemTMP.color = new Color(0.5f, 1f, 0.5f, 0.7f); // Verde suave y semitransparente
                itemTMP.fontStyle = FontStyles.Strikethrough; // Aplica el tachado
            }

            productosMarcados.Add(producto.nombre);
            puntos++;
            puntosText.text = "Puntos: " + puntos;
            GameSession.puntaje = puntos;
        }
        else
        {
            // No está en la lista
            errores++;
            erroresText.text = "Fallos: " + errores;
            GameSession.errores = errores;
        }

        if (productosMarcados.Count == listaDeCompras.Count)
        {
            juegoTerminado = true;
            MostrarPopupFinal();
        }
    }

    // Se elimina la corrutina AnimarCheck ya que ahora usamos Strikethrough

    void MostrarPopupFinal()
    {
        if (popupManager != null)
            popupManager.MostrarPopup(puntos, errores, tiempo);

        StartCoroutine(GuardarSesion());
    }

    IEnumerator GuardarSesion()
    {
        // ... (Lógica de guardar sesión sin cambios) ...
        if (PlayerManager.instance == null || PlayerManager.instance.jugadorActual == null)
        {
            Debug.LogWarning("No hay jugador para guardar la sesión");
            yield break;
        }

        string url = "http://localhost/neuroquest/api/guardar_sesion.php";

        SesionDataShopping data = new SesionDataShopping()
        {
            jugador_id = PlayerManager.instance.jugadorActual.id,
            minijuego = "compras",
            puntuacion = GameSession.puntaje,
            duracion_segundos = Mathf.RoundToInt(GameSession.tiempoSegundos),
            fallos = GameSession.errores
        };

        string json = JsonUtility.ToJson(data);


        Debug.Log("--- 📊 Datos de Sesión de Compras a Enviar ---");
        Debug.Log($"Jugador ID: {data.jugador_id}");
        Debug.Log($"Minijuego: **{data.minijuego}**");
        Debug.Log($"Puntuación: {data.puntuacion}");
        Debug.Log($"Duración (s): {data.duracion_segundos}");
        Debug.Log($"Fallos: {data.fallos}");
        Debug.Log($"JSON Final: {json}");
        Debug.Log("-------------------------------------------");

        UnityWebRequest req = new UnityWebRequest(url, "POST");
        byte[] body = Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error guardando sesión: " + req.error);
        }
        else
        {
            Debug.Log("Sesión guardada correctamente: " + req.downloadHandler.text);
        }
    }
}

public static class ListExtensions
{
    private static System.Random rng = new System.Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
}