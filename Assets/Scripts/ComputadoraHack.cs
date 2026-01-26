using UnityEngine;
using TMPro;
using System.Collections;

public class ComputadoraHack : MonoBehaviour
{
    [Header("UI del Hackeo")]
    public TextMeshProUGUI textoNotificacion;

    private bool isPlayerNearby = false;
    private bool yaHackeada = false;

    // No necesitamos variable "inventarioJugador" porque usamos el Singleton estático

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E) && !yaHackeada)
        {
            // --- CORRECCIÓN AQUÍ ---
            // Verificamos si tiene el item "Tarjeta" en la lista
            if (Inventario.Instancia.TieneItem("Tarjeta"))
            {
                StartCoroutine(ProcesoHackeo());
            }
            else
            {
                MostrarMensaje("No hay tarjeta conectada para hackear.", Color.yellow);
            }
        }
    }

    IEnumerator ProcesoHackeo()
    {
        MostrarMensaje("INICIANDO HACKEO... ESPERE...", Color.cyan);
        yield return new WaitForSeconds(2f);

        MostrarMensaje("... BYPASSING SECURITY ...", Color.cyan);
        yield return new WaitForSeconds(2f);

        // --- CORRECCIÓN AQUÍ ---
        // Llamamos a la función global para desbloquear
        Inventario.Instancia.DesbloquearTarjeta();

        yaHackeada = true;
        MostrarMensaje("¡LISTO! TARJETA DESBLOQUEADA.", Color.green);
    }

    void MostrarMensaje(string mensaje, Color colorTexto)
    {
        if (textoNotificacion != null)
        {
            textoNotificacion.text = mensaje;
            textoNotificacion.color = colorTexto;
            StartCoroutine(BorrarTexto(4f));
        }
    }

    IEnumerator BorrarTexto(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        textoNotificacion.text = "";
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerNearby = true;
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerNearby = false;
    }
}