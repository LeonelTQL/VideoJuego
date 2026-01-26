using UnityEngine;
using TMPro;
using System.Collections;

public class PanelLector : MonoBehaviour
{
    [Header("Configuración UI")]
    public TextMeshProUGUI textoNotificacion;

    [Header("¿Qué tarjeta pide esta puerta?")]
    public string ID_Requerido = "Tarjeta"; // <--- AQUÍ DICES QUÉ LLAVE NECESITA

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
        // Usamos la variable ID_Requerido en lugar de escribir "Tarjeta" a mano

        // 1. ¿Tiene la tarjeta correcta?
        if (!Inventario.Instancia.TieneItem(ID_Requerido))
        {
            MostrarMensaje("[X] Necesitas: " + ID_Requerido, Color.red);
            return;
        }

        // 2. ¿Está hackeada?
        if (Inventario.Instancia.TieneItem(ID_Requerido) && !Inventario.Instancia.EsItemDesbloqueado(ID_Requerido))
        {
            MostrarMensaje("[BLOQUEADO] " + ID_Requerido + " requiere autorización.", Color.red);
            return;
        }

        // 3. Éxito
        if (Inventario.Instancia.EsItemDesbloqueado(ID_Requerido))
        {
            MostrarMensaje("[OK] ACCESO CONCEDIDO.", Color.green);
            if (objetoVerde != null) objetoVerde.SetActive(true);

            if (paredParaDestruir != null)
            {
                Destroy(paredParaDestruir);
                // Consumimos la tarjeta específica
                Inventario.Instancia.ConsumirItem(ID_Requerido);
            }
        }
    }

    // ... (El resto de funciones MostrarMensaje y OnTrigger siguen igual) ...
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