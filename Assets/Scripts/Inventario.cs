using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventario : MonoBehaviour
{
    public static Inventario Instancia;

    [Header("UI Slots")]
    public GameObject panelInventario;
    public Image[] slotsUI;

    // Clase interna para el item
    [System.Serializable]
    public class ItemDatos
    {
        public string id;
        public Sprite icono;
        public bool desbloqueado;
    }

    private List<ItemDatos> items = new List<ItemDatos>();
    private bool inventarioAbierto = false;

    void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // SOLO intentamos abrir si el panel existe (Evita el error rojo)
        if (panelInventario != null && Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventario();
        }
    }

    // --- ESTA ES LA FUNCION MAGICA NUEVA ---
    // Un script auxiliar llamará a esto cada vez que cargue una escena nueva
    public void ReconectarUI(GameObject nuevoPanel, Image[] nuevosSlots)
    {
        panelInventario = nuevoPanel;
        slotsUI = nuevosSlots;

        // Cerramos el panel por defecto en la nueva escena
        inventarioAbierto = false;
        if (panelInventario != null) panelInventario.SetActive(false);

        // Volvemos a pintar los items que teniamos guardados
        ActualizarUI();
        Debug.Log("✅ Inventario reconectado a la nueva escena.");
    }
    // ---------------------------------------

    public void ToggleInventario()
    {
        inventarioAbierto = !inventarioAbierto;
        if (panelInventario != null) panelInventario.SetActive(inventarioAbierto);
        if (inventarioAbierto) ActualizarUI();
    }

    public void AgregarItem(string id, Sprite sprite, bool estadoInicial = false)
    {
        // Si la UI no está lista, guardamos el dato pero no actualizamos visualmente para evitar error
        ItemDatos nuevo = new ItemDatos { id = id, icono = sprite, desbloqueado = estadoInicial };
        items.Add(nuevo);

        if (slotsUI != null && slotsUI.Length > 0) ActualizarUI();
    }

    public bool TieneItem(string id)
    {
        return items.Exists(x => x.id == id);
    }

    public bool EsItemDesbloqueado(string id)
    {
        ItemDatos item = items.Find(x => x.id == id);
        return item != null && item.desbloqueado;
    }

    public void DesbloquearTarjeta()
    {
        ItemDatos tarjeta = items.Find(x => x.id == "Tarjeta");
        if (tarjeta != null) tarjeta.desbloqueado = true;
    }

    void ActualizarUI()
    {
        if (panelInventario == null || slotsUI == null) return;

        for (int i = 0; i < slotsUI.Length; i++)
        {
            if (i < items.Count)
            {
                slotsUI[i].enabled = true;
                slotsUI[i].sprite = items[i].icono;
                slotsUI[i].preserveAspect = true;
                slotsUI[i].color = Color.white;
            }
            else
            {
                slotsUI[i].enabled = false;
            }
        }
    }

    // --- NUEVA FUNCIÓN: ELIMINAR ITEM ---
    public void ConsumirItem(string id)
    {
        // Buscamos el item en la lista
        ItemDatos itemAborrar = items.Find(x => x.id == id);

        if (itemAborrar != null)
        {
            items.Remove(itemAborrar); // Lo borramos de la memoria
            ActualizarUI();            // Refrescamos los dibujos para que desaparezca
            Debug.Log("Item consumido: " + id);
        }
    }
}