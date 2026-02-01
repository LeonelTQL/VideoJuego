using UnityEngine;
using UnityEngine.Audio;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    [Header("Configuración")]
    public AudioMixerGroup canalSFX; // Arrastra el grupo SFX aquí

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

    public void PlaySound(AudioClip clip, float volumen = 1f)
    {
        if (clip != null)
        {
            source.PlayOneShot(clip, volumen);
        }
    }

    public void PlayAceptar() => source.PlayOneShot(sonidoAceptar);
    public void PlayDesplazar() => source.PlayOneShot(sonidoDesplazar);
    public void PlayConfig() => source.PlayOneShot(sonidoConfiguracion);
}