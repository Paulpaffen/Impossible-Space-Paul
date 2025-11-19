using UnityEngine;

/// <summary>
/// Script versátil para activar/desactivar GameObjects.
/// Puede usarse con Unity Events (botones) o con Trigger Colliders (automático).
/// </summary>
public class ObjectToggler : MonoBehaviour
{
    [Header("Target Objects")]
    [Tooltip("GameObject(s) a activar/desactivar (ej: puerta, botones, etc.)")]
    public GameObject[] targetObjects;
    
    [Header("Toggle Settings")]
    [Tooltip("Acción a realizar cuando se activa")]
    public ToggleAction action = ToggleAction.Disable;
    
    [Tooltip("¿Destruir los objetos en lugar de solo desactivarlos?")]
    public bool destroyInsteadOfDisable = false;
    
    [Header("Trigger Activation (Optional)")]
    [Tooltip("¿Activar automáticamente cuando el jugador entre al trigger?")]
    public bool useColliderTrigger = false;
    
    [Tooltip("Tag del jugador para detectar en el trigger")]
    public string playerTag = "Player";
    
    [Tooltip("¿Solo activarse una vez al cruzar el trigger?")]
    public bool triggerOnlyOnce = true;
    
    [Header("Audio Feedback")]
    [Tooltip("Sonido a reproducir al activar/desactivar (opcional)")]
    public AudioClip toggleSound;
    
    [Tooltip("Volumen del sonido")]
    [Range(0f, 1f)]
    public float volume = 1.0f;
    
    [Header("Visual Feedback")]
    [Tooltip("Mostrar logs en consola")]
    public bool showLogs = true;
    
    // Estado interno
    private AudioSource audioSource;
    private bool hasBeenTriggered = false;
    private Collider triggerCollider;
    
    public enum ToggleAction
    {
        Toggle,    // Alternar estado
        Enable,    // Siempre activar
        Disable    // Siempre desactivar
    }
    
    private void Start()
    {
        // Crear AudioSource si hay un sonido asignado
        if (toggleSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound
        }
        
        // Si se usa como trigger, verificar que haya un collider
        if (useColliderTrigger)
        {
            triggerCollider = GetComponent<Collider>();
            
            if (triggerCollider == null)
            {
                Debug.LogWarning("<color=orange>[ObjectToggler] ⚠️ useColliderTrigger está activado pero no hay Collider. Agregando BoxCollider...</color>");
                triggerCollider = gameObject.AddComponent<BoxCollider>();
            }
            
            triggerCollider.isTrigger = true;
            
            if (showLogs)
            {
                Debug.Log($"<color=cyan>[ObjectToggler] 🎯 Configurado como trigger. Esperando al jugador con tag '{playerTag}'</color>");
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Solo procesar si está configurado como trigger
        if (!useColliderTrigger) return;
        
        // Verificar si es el jugador
        if (!other.CompareTag(playerTag)) return;
        
        // Si solo se activa una vez, verificar
        if (triggerOnlyOnce && hasBeenTriggered) return;
        
        // Ejecutar la acción
        ExecuteAction();
        
        hasBeenTriggered = true;
        
        if (showLogs)
        {
            Debug.Log($"<color=yellow>[ObjectToggler] 👤 Jugador cruzó el trigger en '{gameObject.name}'</color>");
        }
    }
    
    /// <summary>
    /// Ejecuta la acción configurada sobre los objetos objetivo
    /// </summary>
    private void ExecuteAction()
    {
        if (targetObjects == null || targetObjects.Length == 0)
        {
            Debug.LogError("<color=red>[ObjectToggler] ❌ No hay objetos asignados en Target Objects</color>");
            return;
        }
        
        foreach (GameObject obj in targetObjects)
        {
            if (obj == null) continue;
            
            switch (action)
            {
                case ToggleAction.Toggle:
                    ToggleObject(obj);
                    break;
                case ToggleAction.Enable:
                    EnableObject(obj);
                    break;
                case ToggleAction.Disable:
                    DisableObject(obj);
                    break;
            }
        }
        
        PlaySound();
    }
    
    /// <summary>
    /// Alterna el estado del objeto
    /// </summary>
    private void ToggleObject(GameObject obj)
    {
        if (destroyInsteadOfDisable && obj.activeSelf)
        {
            if (showLogs)
            {
                Debug.Log($"<color=red>[ObjectToggler] 💥 Destruyendo: {obj.name}</color>");
            }
            Destroy(obj);
        }
        else
        {
            bool newState = !obj.activeSelf;
            obj.SetActive(newState);
            
            if (showLogs)
            {
                string status = newState ? "✅ Activado" : "❌ Desactivado";
                Debug.Log($"<color=yellow>[ObjectToggler] {status}: {obj.name}</color>");
            }
        }
    }
    
    /// <summary>
    /// Activa el objeto
    /// </summary>
    private void EnableObject(GameObject obj)
    {
        obj.SetActive(true);
        
        if (showLogs)
        {
            Debug.Log($"<color=lime>[ObjectToggler] ✅ Activado: {obj.name}</color>");
        }
    }
    
    /// <summary>
    /// Desactiva o destruye el objeto
    /// </summary>
    private void DisableObject(GameObject obj)
    {
        if (destroyInsteadOfDisable)
        {
            if (showLogs)
            {
                Debug.Log($"<color=red>[ObjectToggler] 💥 Destruyendo: {obj.name}</color>");
            }
            Destroy(obj);
        }
        else
        {
            obj.SetActive(false);
            
            if (showLogs)
            {
                Debug.Log($"<color=red>[ObjectToggler] ❌ Desactivado: {obj.name}</color>");
            }
        }
    }
    
    // ==========================================
    // MÉTODOS PÚBLICOS PARA UNITY EVENTS
    // ==========================================
    
    /// <summary>
    /// Activa o desactiva los objetos objetivo (alterna).
    /// Llama este método desde Unity Events.
    /// </summary>
    public void Toggle()
    {
        action = ToggleAction.Toggle;
        ExecuteAction();
    }
    
    /// <summary>
    /// Desactiva los objetos objetivo (siempre).
    /// Llama este método desde Unity Events.
    /// </summary>
    public void Disable()
    {
        action = ToggleAction.Disable;
        ExecuteAction();
    }
    
    /// <summary>
    /// Activa los objetos objetivo (siempre).
    /// Llama este método desde Unity Events.
    /// </summary>
    public void Enable()
    {
        action = ToggleAction.Enable;
        ExecuteAction();
    }
    
    /// <summary>
    /// Reproduce el sonido de feedback si está asignado
    /// </summary>
    private void PlaySound()
    {
        if (audioSource != null && toggleSound != null)
        {
            audioSource.PlayOneShot(toggleSound, volume);
        }
    }
    
    /// <summary>
    /// Reinicia el estado del trigger (permite activarse nuevamente)
    /// </summary>
    [ContextMenu("Reset Trigger")]
    public void ResetTrigger()
    {
        hasBeenTriggered = false;
        
        if (showLogs)
        {
            Debug.Log("<color=cyan>[ObjectToggler] 🔄 Trigger reiniciado</color>");
        }
    }
    
    /// <summary>
    /// Visualización en el editor (si se usa como trigger)
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!useColliderTrigger) return;
        
        Collider col = GetComponent<Collider>();
        if (col == null) return;
        
        // Color según la acción
        Color gizmoColor = Color.green;
        switch (action)
        {
            case ToggleAction.Enable:
                gizmoColor = Color.green;
                break;
            case ToggleAction.Disable:
                gizmoColor = Color.red;
                break;
            case ToggleAction.Toggle:
                gizmoColor = Color.yellow;
                break;
        }
        
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
        
        if (col is BoxCollider boxCol)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(boxCol.center, boxCol.size);
            
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireCube(boxCol.center, boxCol.size);
        }
        
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1f, $"ObjectToggler\n{action}");
        #endif
    }
}
