using UnityEngine;

public class DiscoBallHazard : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Rotation speed in degrees per second")]
    [SerializeField] private float rotationSpeed = 60f;
    
    [Tooltip("Rotation direction: 1 = clockwise, -1 = counter-clockwise")]
    [SerializeField] private float rotationDirection = 1f;
    
    [Header("Beam Configuration")]
    [Tooltip("Number of laser beams emanating from the disco ball")]
    [SerializeField] private int numberOfBeams = 4;
    
    [Tooltip("How far beams extend from center")]
    [SerializeField] private float beamLength = 10f;
    
    [Header("Beam Prefabs - Multi-Color Support")]
    [Tooltip("Array of different colored laser beam prefabs (Cyan, Orange, Purple, etc.)")]
    [SerializeField] private GameObject[] laserBeamPrefabs;
    
    [Tooltip("How to assign colors to beams")]
    [SerializeField] private ColorMode colorMode = ColorMode.Random;
    
    [Header("Color Switching")]
    [Tooltip("Should beams change colors over time?")]
    [SerializeField] private bool switchColors = false;
    
    [Tooltip("How often to switch colors (seconds)")]
    [SerializeField] private float colorSwitchInterval = 2f;
    
    [Header("Beam Container")]
    [Tooltip("Parent object that holds all beams (auto-created if null)")]
    [SerializeField] private Transform beamContainer;
    
    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip rotationSound;
    [SerializeField] private AudioSource audioSource;

    [Header("Sprite Settings")]
    [Tooltip("Native width of the laser beam sprite in units")]
    [SerializeField] private float beamSpriteWidth = 4f; // Your sprite is 4 units wide

    // Color assignment modes
    public enum ColorMode
    {
        Random,          // Each beam gets random color
        Sequential,      // Cycle through colors in order
        AllSameRandom,   // All beams same color, picked randomly
        Alternating      // Alternate between colors (RGBRGB...)
    }
    
    // Runtime tracking
    private GameObject[] spawnedBeams;
    private float colorSwitchTimer = 0f;
    
    void Start()
    {
        // Validate beam prefabs
        if (laserBeamPrefabs == null || laserBeamPrefabs.Length == 0)
        {
            Debug.LogError("DiscoBallHazard: No laser beam prefabs assigned! Add at least one prefab.");
            return;
        }
        
        // Create beam container if not assigned
        if (beamContainer == null)
        {
            GameObject container = new GameObject("BeamContainer");
            container.transform.SetParent(transform);
            container.transform.localPosition = Vector3.zero;
            beamContainer = container.transform;
        }
        
        // Spawn beams
        SpawnBeams();
        
        // Play rotation sound if available
        if (audioSource != null && rotationSound != null)
        {
            audioSource.clip = rotationSound;
            audioSource.loop = true;
            audioSource.Play();
        }
        
        Debug.Log($"DiscoBall initialized with {numberOfBeams} beams, rotating at {rotationSpeed}°/s, using {laserBeamPrefabs.Length} color variants");
    }
    
    void Update()
    {
        // Rotate the beam container continuously
        if (beamContainer != null)
        {
            float rotationThisFrame = rotationSpeed * rotationDirection * Time.deltaTime;
            beamContainer.Rotate(0f, 0f, rotationThisFrame);
        }
        
        // Handle color switching
        if (switchColors && spawnedBeams != null && spawnedBeams.Length > 0)
        {
            colorSwitchTimer += Time.deltaTime;
            if (colorSwitchTimer >= colorSwitchInterval)
            {
                colorSwitchTimer = 0f;
                SwitchBeamColors();
            }
        }
    }
    
    void SpawnBeams()
    {
        if (laserBeamPrefabs == null || laserBeamPrefabs.Length == 0)
        {
            Debug.LogError("DiscoBallHazard: No laser beam prefabs assigned!");
            return;
        }
        
        if (beamContainer == null)
        {
            Debug.LogError("DiscoBallHazard: No beam container found!");
            return;
        }
        
        // Clear any existing beams
        foreach (Transform child in beamContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Initialize beam tracking array
        spawnedBeams = new GameObject[numberOfBeams];
        
        // Calculate angle between beams
        float angleStep = 360f / numberOfBeams;
        
        // Pick a random starting color for "AllSameRandom" mode
        int randomColorIndex = Random.Range(0, laserBeamPrefabs.Length);
        
        // Spawn beams in a circle
        for (int i = 0; i < numberOfBeams; i++)
        {
            float angle = i * angleStep;
            
            // Determine which prefab to use based on color mode
            GameObject prefabToUse = GetBeamPrefabForIndex(i, randomColorIndex);
            
            if (prefabToUse == null)
            {
                Debug.LogWarning($"DiscoBallHazard: Beam prefab at index {i} is null, skipping");
                continue;
            }
            
            // Create beam
            GameObject beam = Instantiate(prefabToUse, beamContainer);
            beam.name = $"LaserBeam_{i}";
            spawnedBeams[i] = beam;
            
            // Position at center of disco ball
            beam.transform.localPosition = Vector3.zero;
            
            // Rotate to correct angle
            beam.transform.localRotation = Quaternion.Euler(0f, 0f, angle);

            // Scale beam length
            Vector3 scale = beam.transform.localScale;
            scale.x = beamLength / beamSpriteWidth;
            beam.transform.localScale = scale;

            // Configure beam collider size
            BoxCollider2D beamCollider = beam.GetComponent<BoxCollider2D>();
            if (beamCollider != null)
            {
                Vector2 colliderSize = beamCollider.size;
                colliderSize.x = beamSpriteWidth; // Match sprite's native width
                beamCollider.size = colliderSize;
                beamCollider.offset = new Vector2(beamSpriteWidth / 2f, 0f); // Center it
            }

            Debug.Log($"Spawned beam {i} at angle {angle}° using prefab {prefabToUse.name}");
        }
    }
    
    GameObject GetBeamPrefabForIndex(int beamIndex, int randomColorIndex)
    {
        switch (colorMode)
        {
            case ColorMode.Random:
                // Each beam gets random color
                return laserBeamPrefabs[Random.Range(0, laserBeamPrefabs.Length)];
                
            case ColorMode.Sequential:
                // Cycle through colors in order (0,1,2,0,1,2...)
                return laserBeamPrefabs[beamIndex % laserBeamPrefabs.Length];
                
            case ColorMode.AllSameRandom:
                // All beams same color (picked randomly once)
                return laserBeamPrefabs[randomColorIndex];
                
            case ColorMode.Alternating:
                // Alternate between colors
                return laserBeamPrefabs[beamIndex % laserBeamPrefabs.Length];
                
            default:
                return laserBeamPrefabs[0];
        }
    }
    
    void SwitchBeamColors()
    {
        if (spawnedBeams == null || laserBeamPrefabs == null || laserBeamPrefabs.Length <= 1)
            return;
        
        Debug.Log("Switching beam colors!");
        
        // Pick random color for all-same mode
        int randomColorIndex = Random.Range(0, laserBeamPrefabs.Length);
        
        for (int i = 0; i < spawnedBeams.Length; i++)
        {
            if (spawnedBeams[i] == null) continue;

            // Get current beam properties
            Transform beamTransform = spawnedBeams[i].transform;
            Vector3 position = beamTransform.localPosition;
            Quaternion rotation = beamTransform.localRotation;

            // Pick new prefab
            GameObject newPrefab = GetBeamPrefabForIndex(i, randomColorIndex);

            // Destroy old beam
            Destroy(spawnedBeams[i]);

            // Create new beam with different color
            GameObject newBeam = Instantiate(newPrefab, beamContainer);
            newBeam.name = $"LaserBeam_{i}";
            newBeam.transform.localPosition = position;
            newBeam.transform.localRotation = rotation;

            // Recalculate scale using beamLength
            Vector3 scale = newBeam.transform.localScale;
            scale.x = beamLength / beamSpriteWidth;
            newBeam.transform.localScale = scale;

            // Update collider
            BoxCollider2D beamCollider = newBeam.GetComponent<BoxCollider2D>();
            if (beamCollider != null)
            {
                Vector2 colliderSize = beamCollider.size;
                colliderSize.x = beamSpriteWidth; // Match sprite's native width
                beamCollider.size = colliderSize;
                beamCollider.offset = new Vector2(beamSpriteWidth / 2f, 0f); // Center it
            }

            // Update tracking array
            spawnedBeams[i] = newBeam;
        }
    }
    
    // Call this to change beam count at runtime
    public void ReconfigureBeams(int newBeamCount)
    {
        numberOfBeams = newBeamCount;
        SpawnBeams();
    }
    
    // Call this to change rotation speed at runtime
    public void SetRotationSpeed(float newSpeed)
    {
        rotationSpeed = newSpeed;
    }
    
    // Call this to manually trigger color switch
    public void TriggerColorSwitch()
    {
        SwitchBeamColors();
    }
    
    // Visualize rotation range in editor
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 1f, 0.3f);
        Vector3 pos = transform.position;
        float radius = beamLength;
        
        int segments = 32;
        float angleStep = 360f / segments;
        
        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleStep * Mathf.Deg2Rad;
            float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;
            
            Vector3 point1 = pos + new Vector3(Mathf.Cos(angle1), Mathf.Sin(angle1), 0f) * radius;
            Vector3 point2 = pos + new Vector3(Mathf.Cos(angle2), Mathf.Sin(angle2), 0f) * radius;
            
            Gizmos.DrawLine(point1, point2);
        }
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(pos, 0.3f);
    }
    
    void OnDrawGizmosSelected()
    {
        if (numberOfBeams <= 0) return;
        
        Gizmos.color = Color.red;
        Vector3 pos = transform.position;
        float angleStep = 360f / numberOfBeams;
        
        for (int i = 0; i < numberOfBeams; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
            Vector3 endPoint = pos + direction * beamLength;
            
            Gizmos.DrawLine(pos, endPoint);
        }
        
        #if UNITY_EDITOR
        string colorInfo = switchColors ? $"\nSwitching every {colorSwitchInterval}s" : "";
        UnityEditor.Handles.Label(pos + Vector3.up * (beamLength + 1f),
            $"Disco Ball\nBeams: {numberOfBeams}\nSpeed: {rotationSpeed}°/s\nColors: {colorMode}{colorInfo}");
        #endif
    }
}
