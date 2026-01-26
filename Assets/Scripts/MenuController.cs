using UnityEngine;
using UnityEngine.SceneManagement; // Importante para manejar escenas

public class MenuController : MonoBehaviour
{
    public void IrAClase()
    {
        // Carga la escena por su nombre exacto
        SceneManager.LoadScene("Clase");
    }
}