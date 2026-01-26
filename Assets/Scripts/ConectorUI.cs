using UnityEngine;
using UnityEngine.UI;

public class ConectorUI : MonoBehaviour
{
    [Header("Arrastra aquí el Panel y Slots de ESTA escena")]
    public GameObject panelInventarioLocal;
    public Image[] slotsLocales;

    void Start()
    {
        // Si existe el inventario inmortal, le damos la nueva UI
        if (Inventario.Instancia != null)
        {
            Inventario.Instancia.ReconectarUI(panelInventarioLocal, slotsLocales);
        }
    }
}