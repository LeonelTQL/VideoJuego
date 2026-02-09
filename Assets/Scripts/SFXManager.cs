using UnityEngine;
using UnityEngine.Audio;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    [Header("Configuración")]
    public AudioMixerGroup canalSFX;

    [Header("Clips de Sonido")]
    public AudioClip sonidoAceptar;
    public AudioClip sonidoDesplazar;
    public AudioClip sonidoConfiguracion;

    private AudioSource source;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            source = gameObject.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = canalSFX;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- Esta es la clave: Verificar antes de reproducir ---
    private bool PuedoSonar()
    {
        // Retorna true solo si el valor guardado es 1 (Activo)
        return PlayerPrefs.GetInt("EfectosActivos", 1) == 1;
    }

    public void PlaySound(AudioClip clip, float volumen = 1f)
    {
        // Forzamos que si no existe el registro, por defecto sea 1 (Activo)
        if (clip != null && PlayerPrefs.GetInt("EfectosActivos", 1) == 1)
        {
            source.PlayOneShot(clip, volumen);
        }
    }
    // Dentro de SFXManager.cs
    public bool CanPlaySFX
    {
        get
        {
            // Retorna true si el ajuste es 1, false si es 0
            return PlayerPrefs.GetInt("EfectosActivos", 1) == 1;
        }
    }

    public void PlayAceptar() { if (PuedoSonar()) source.PlayOneShot(sonidoAceptar); }
    public void PlayDesplazar() { if (PuedoSonar()) source.PlayOneShot(sonidoDesplazar); }
    public void PlayConfig() { if (PuedoSonar()) source.PlayOneShot(sonidoConfiguracion); }
}