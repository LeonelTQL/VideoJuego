using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CambioEscenaFinal : MonoBehaviour
{
    [Header("Configuración")]
    public string nombreEscenaDestino;
    public float duracionTransicion = 1f;

    private bool yaActivado = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !yaActivado)
        {
            yaActivado = true;
            StartCoroutine(SecuenciaCambioEscena());
        }
    }

    IEnumerator SecuenciaCambioEscena()
    {

        yield return new WaitForSeconds(duracionTransicion);

        // 3. LIMPIEZA TOTAL: Destruir objetos que tengan DontDestroyOnLoad
        // Buscamos todos los objetos raíz y los borramos para que no pasen a la escena final
        LimpiarObjetosPersistentes();

        // 4. Cargar la escena final
        SceneManager.LoadScene(nombreEscenaDestino);
    }

    void LimpiarObjetosPersistentes()
    {
        // Esto busca todos los GameObjects en la jerarquía
        GameObject[] todosLosObjetos = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (GameObject obj in todosLosObjetos)
        {
            // Opcional: Puedes excluir este objeto (el que cambia la escena) 
            // para que la corrutina termine de ejecutarse sin errores.
            if (obj != gameObject)
            {
                Destroy(obj);
            }
        }
    }
}