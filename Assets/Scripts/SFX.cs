using UnityEngine;
using UnityEngine.Audio;

public class SFX : MonoBehaviour
{
    [Header("Configuración")]
    public AudioMixerGroup canalSFX;

    [Header("Clips de Sonido")]
    public AudioClip sonidoAceptar;
    public AudioClip sonidoDesplazar;
    public AudioClip sonidoConfiguracion;

    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();

        if (source == null)
        {
            Debug.LogError("❌ El objeto SFX no tiene AudioSource");
            return;
        }

        if (canalSFX != null)
            source.outputAudioMixerGroup = canalSFX;
    }

    private bool PuedoSonar()
    {
        return PlayerPrefs.GetInt("EfectosActivos", 1) == 1;
    }

    public void PlayAceptar()
    {
        if (PuedoSonar() && sonidoAceptar != null)
            source.PlayOneShot(sonidoAceptar);
    }

    public void PlayDesplazar()
    {
        if (PuedoSonar() && sonidoDesplazar != null)
            source.PlayOneShot(sonidoDesplazar);
    }

    public void PlayConfig()
    {
        if (PuedoSonar() && sonidoConfiguracion != null)
            source.PlayOneShot(sonidoConfiguracion);
    }
}
