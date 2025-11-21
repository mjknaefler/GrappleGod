using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Player SFX")]
    public AudioClip shootSound;
    public AudioClip hurtSound;   // 🔊 NEW

    private AudioSource audioSource;

    void Awake()
    {
        // Auto-find an AudioSource on this object or its children
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = GetComponentInChildren<AudioSource>();

        if (audioSource == null)
            Debug.LogError("PlayerAudio: No AudioSource found on " + gameObject.name);
    }

    public void PlayShoot()
    {
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
    }

    public void PlayHurt()       // 🔊 NEW
    {
        if (audioSource != null && hurtSound != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }
    }
}
