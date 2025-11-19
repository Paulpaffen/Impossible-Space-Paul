using UnityEngine;

/// <summary>
/// Trigger invisible que reproduce un sonido cuando el jugador lo cruza.
/// Útil para marcar posiciones específicas en el espacio.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(AudioSource))]
public class UnitMarkerTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("Tag del jugador")]
    public string playerTag = "Player";
    
    [Header("Sound Settings")]
    [Tooltip("Sonido a reproducir")]
    public AudioClip markerSound;
    
    [Tooltip("Volumen del sonido")]
    [Range(0f, 1f)]
    public float volume = 0.5f;
    
    [Tooltip("Pitch del sonido")]
    [Range(0.5f, 2f)]
    public float pitch = 1.0f;
    
    [Header("Visual Settings")]
    [Tooltip("Color del gizmo en Scene view")]
    public Color gizmoColor = Color.green;
    
    [Tooltip("Mostrar logs al cruzar")]
    public bool showLogs = true;
    
    private AudioSource audioSource;
    private BoxCollider triggerCollider;
    private bool hasBeenTriggered = false;
    
    private void Awake()
    {
        // Configurar collider
        triggerCollider = GetComponent<BoxCollider>();
        triggerCollider.isTrigger = true;
        
        // Tamaño pequeño para el trigger
        if (triggerCollider.size.magnitude < 0.1f)
        {
            triggerCollider.size = new Vector3(0.2f, 2f, 0.2f); // Delgado
        }
        
        // Configurar audio
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D sound
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.clip = markerSound;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            PlaySound();
            
            if (showLogs)
            {
                Debug.Log($"<color=yellow>[Marker] ?? Cruzado marcador en {transform.position}</color>");
            }
        }
    }
    
    private void PlaySound()
    {
        if (audioSource != null && markerSound != null)
        {
            audioSource.PlayOneShot(markerSound, volume);
        }
    }
    
    /// <summary>
    /// Crea una cuadrícula de marcadores alrededor del origen
    /// </summary>
    [ContextMenu("Create Grid of Markers")]
    public void CreateMarkerGrid()
    {
        GameObject parent = new GameObject("MarkerGrid");
        
        // Crear marcadores en los 4 ejes principales
        for (int i = 1; i <= 5; i++)
        {
            // Eje X positivo
            CreateMarker(parent, new Vector3(i, 0, 0), $"Marker_X+{i}");
            
            // Eje X negativo
            CreateMarker(parent, new Vector3(-i, 0, 0), $"Marker_X-{i}");
            
            // Eje Z positivo
            CreateMarker(parent, new Vector3(0, 0, i), $"Marker_Z+{i}");
            
            // Eje Z negativo
            CreateMarker(parent, new Vector3(0, 0, -i), $"Marker_Z-{i}");
        }
        
        Debug.Log("<color=lime>[Marker] ? Cuadrícula creada con 20 marcadores</color>");
    }
    
    private void CreateMarker(GameObject parent, Vector3 position, string name)
    {
        GameObject marker = new GameObject(name);
        marker.transform.parent = parent.transform;
        marker.transform.position = position;
        
        UnitMarkerTrigger trigger = marker.AddComponent<UnitMarkerTrigger>();
        trigger.markerSound = this.markerSound;
        trigger.volume = this.volume;
        trigger.pitch = this.pitch;
        trigger.showLogs = this.showLogs;
        
        // Color diferente según eje
        if (position.x != 0)
        {
            trigger.gizmoColor = position.x > 0 ? Color.red : new Color(1f, 0.5f, 0.5f);
        }
        else
        {
            trigger.gizmoColor = position.z > 0 ? Color.blue : new Color(0.5f, 0.5f, 1f);
        }
    }
    
    private void OnDrawGizmos()
    {
        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null)
        {
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(col.center, col.size);
            
            // Borde
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireCube(col.center, col.size);
        }
        
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, gameObject.name);
        #endif
    }
}
