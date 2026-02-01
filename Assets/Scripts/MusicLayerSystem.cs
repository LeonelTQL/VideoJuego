using UnityEngine;
using System.Collections;

public class MusicLayerSystem : MonoBehaviour
{
    public static MusicLayerSystem Instance;

    [Header("Configuración de Capas")]
    public AudioSource[] capas;
    public float velocidadFade = 0.8f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("<color=green>MusicLayerSystem: Instancia creada y protegida.</color>");
        }
        else
        {
            Debug.Log("<color=yellow>MusicLayerSystem: Duplicado destruido.</color>");
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (capas == null || capas.Length == 0)
        {
            Debug.LogError("MusicLayerSystem: ¡No has arrastrado los AudioSources al array 'capas' en el Inspector!");
            return;
        }

        double tiempoInicio = AudioSettings.dspTime + 0.5f;

        for (int i = 0; i < capas.Length; i++)
        {
            capas[i].loop = true;
            capas[i].playOnAwake = false;
            capas[i].volume = 0f;
            capas[i].PlayScheduled(tiempoInicio);
            Debug.Log($"MusicLayerSystem: Capa {i} preparada y programada.");
        }

        // Forzamos el inicio de la capa 0
        Debug.Log("MusicLayerSystem: Iniciando capa base (0)...");
        ActivarSiguienteNivel(0);
    }

    public void ActivarSiguienteNivel(int indice)
    {
        if (indice >= 0 && indice < capas.Length)
        {
            Debug.Log($"MusicLayerSystem: Intentando activar capa {indice}...");
            StartCoroutine(FadeIn(indice));
        }
        else
        {
            Debug.LogWarning($"MusicLayerSystem: El índice {indice} está fuera de rango.");
        }
    }

    IEnumerator FadeIn(int indice)
    {
        Debug.Log($"MusicLayerSystem: Empezando Fade In de la capa {indice}. Volumen actual: {capas[indice].volume}");

        float targetVolume = 1f;
        while (capas[indice].volume < targetVolume)
        {
            capas[indice].volume += velocidadFade * Time.deltaTime;
            yield return null;
        }

        capas[indice].volume = targetVolume;
        Debug.Log($"<color=cyan>MusicLayerSystem: Capa {indice} ha llegado al volumen máximo.</color>");
    }
}