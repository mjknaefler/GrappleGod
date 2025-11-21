using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    private AudioSource audioSource;

    private void Awake()
    {
        // If a MusicManager already exists, destroy this new one
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // This becomes the one and only MusicManager
        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();

        // Start playing if not already playing
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}
