using UnityEngine;

public class CardItem : MonoBehaviour
{
    [Header("Configuración UI Mensaje")]
    public GameObject uiPanelMessage;

    [Header("Configuración del Item")]
    public string ID_del_Item = "Tarjeta"; // <--- AQUÍ CAMBIAS EL NOMBRE EN UNITY
    public Sprite iconoParaInventario;

    private bool isPlayerNearby = false;
    private bool isMenuOpen = false;
    private bool cardCollected = false;

    void Start()
    {
        // Al iniciar, verificamos si ESTA tarjeta específica ya la tenemos
        if (Inventario.Instancia != null && Inventario.Instancia.TieneItem(ID_del_Item))
        {
            Destroy(gameObject); // Si ya tengo la "Tarjeta A", borro la "Tarjeta A" del suelo
        }
    }

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E) && !cardCollected)
        {
            RecogerObjeto();
        }

        if (isMenuOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CerrarMensaje();
        }
    }

    void RecogerObjeto()
    {
        if (iconoParaInventario == null) Debug.LogError("⚠️ ¡Falta el Sprite!");

        isMenuOpen = true;
        cardCollected = true;
        if (uiPanelMessage != null) uiPanelMessage.SetActive(true);

        if (Inventario.Instancia != null)
        {
            // Guardamos el item con SU NOMBRE ÚNICO
            Inventario.Instancia.AgregarItem(ID_del_Item, iconoParaInventario, false);
        }

        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        Time.timeScale = 0f;
    }

    void CerrarMensaje()
    {
        isMenuOpen = false;
        if (uiPanelMessage != null) uiPanelMessage.SetActive(false);
        Time.timeScale = 1f;
        Destroy(gameObject);
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