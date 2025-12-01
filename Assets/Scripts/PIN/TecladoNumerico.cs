using UnityEngine;
using TMPro;

public class TecladoNumerico : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;

    private void Start()
    {
        if (inputField != null)
            inputField.text = "";
    }

    public void AgregarNumero(string numero)
    {
        if (inputField == null) return;
        inputField.text += numero;
    }

    public void BorrarUltimo()
    {
        if (inputField == null || string.IsNullOrEmpty(inputField.text))
            return;

        inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
    }

    
}
