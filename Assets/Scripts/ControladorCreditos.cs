using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para cambiar de escena

public class ControladorCreditos : MonoBehaviour
{
    [SerializeField] private string nombreMenuPrincipal = "MenuPrincipal";
    [SerializeField] private float tiempoDeEspera = 30f; // Duración de tus créditos en segundos

    void Start()
    {
        // Opción A: Por tiempo. Si sabes cuánto duran los créditos, esto los manda al menú al terminar.
        Invoke("IrAlMenu", tiempoDeEspera);
    }

    void Update()
    {
        // Opción B: El "Skip". Si presionas Esc o cualquier tecla configurada como "Cancel".
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            IrAlMenu();
        }
    }

    public void IrAlMenu()
    {
        SceneManager.LoadScene(nombreMenuPrincipal);
    }
}