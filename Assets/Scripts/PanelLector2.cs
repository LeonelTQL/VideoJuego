using UnityEngine;
using TMPro;
using System.Collections;

public class PanelLector2 : MonoBehaviour
{
    [Header("Configuración UI")]
    public TextMeshProUGUI textoNotificacion;

    // Ya no necesitamos un solo ID variable, porque pediste A y B fijos.
    // public string ID_Requerido = "Tarjeta"; 

    [Header("Objetos a Activar/Desactivar")]
    public GameObject objetoVerde;
    public GameObject paredParaDestruir;

    private bool isPlayerNearby = false;

    void Start()
    {
        if (textoNotificacion != null) textoNotificacion.text = "";
        if (objetoVerde != null) objetoVerde.SetActive(false);
    }

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            IntentarAbrir();
        }
    }

    void IntentarAbrir()
    {
        // 1. Verificamos si tiene la Tarjeta A y la Tarjeta B
        bool tieneTarjetaA = Inventario.Instancia.TieneItem("Tarjeta_A");
        bool tieneTarjetaB = Inventario.Instancia.TieneItem("Tarjeta_B");

        // CASO DE FALLO: Si falta alguna de las dos
        if (!tieneTarjetaA || !tieneTarjetaB)
        {
            MostrarMensaje("[X] Faltan credenciales: Necesitas Tarjeta_A y Tarjeta_B", Color.red);
            return;
        }

        // 2. ÉXITO (Solo si tiene ambas)
        // Ya no verificamos si están "desbloqueadas", solo que las tengas.
        if (tieneTarjetaA && tieneTarjetaB)
        {
            MostrarMensaje("[OK] ACCESO DOBLE CONCEDIDO.", Color.green);

            if (objetoVerde != null)
                objetoVerde.SetActive(true);

            if (paredParaDestruir != null)
            {
                Destroy(paredParaDestruir); // Desaparece la puerta

                // OPCIONAL: Si quieres que se gasten las tarjetas al usarlas, descomenta esto:
                Inventario.Instancia.ConsumirItem("Tarjeta_A");
                Inventario.Instancia.ConsumirItem("Tarjeta_B");
            }
        }
    }

    // ------------------------------------------------------------------
    // El resto de funciones auxiliares se mantienen igual
    // ------------------------------------------------------------------

    void MostrarMensaje(string mensaje, Color colorTexto)
    {
        if (textoNotificacion != null)
        {
            textoNotificacion.text = mensaje;
            textoNotificacion.color = colorTexto;
            StopAllCoroutines();
            StartCoroutine(BorrarMensajeDespuesDe(3f));
        }
    }

    IEnumerator BorrarMensajeDespuesDe(float segundos)
    {
        yield return new WaitForSeconds(segundos);
        if (textoNotificacion != null) textoNotificacion.text = "";
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