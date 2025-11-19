using UnityEngine;

/// <summary>
/// Representa una habitación individual en el sistema de Impossible Spaces.
/// Permite activar/desactivar/destruir la habitación de forma visual.
/// </summary>
public class Room : MonoBehaviour
{
    [Header("Room Identification")]
    [Tooltip("Nombre identificador de la habitación (ej: 'Sala1', 'Pasillo', etc.)")]
    public string roomName = "Habitacion";
    
    [Header("Room State")]
    [Tooltip("¿La habitación está activa visualmente?")]
    [SerializeField] private bool isActive = true;
    
    [Header("Collider Settings")]
    [Tooltip("¿Desactivar colliders cuando la habitación está inactiva?")]
    [SerializeField] private bool disableCollidersWhenInactive = true;
    
    [Tooltip("Tags de colliders/triggers que NO deben desactivarse (ej: 'Player', 'SpecialTrigger'). Deja vacío para desactivar TODOS los colliders cuando la habitación esté inactiva.")]
    public string[] excludedColliderTags = new string[0];
    
    [Header("Performance Settings")]
    [Tooltip("¿Destruir la habitación en lugar de solo desactivarla? (Ahorra memoria, pero no se puede volver)")]
    [SerializeField] private bool destroyWhenDeactivated = false;
    
    [Tooltip("Delay en segundos antes de destruir la habitación (para permitir transiciones suaves)")]
    [Range(0f, 5f)]
    public float destroyDelay = 0.5f;

    [Header("Debug Settings")]
    [Tooltip("Mostrar logs detallados en consola")]
    [SerializeField] private bool showDebugLogs = true;
    
    // Propiedad pública para controlar el estado
    public bool IsActive
    {
        get => isActive;
        set
        {
            isActive = value;
            UpdateRoomVisibility();
        }
    }
    
    private void Start()
    {
        // Aplicar el estado inicial al comenzar
        UpdateRoomVisibility();
    }
    
    /// <summary>
    /// Activa la habitación (la hace visible)
    /// </summary>
    public void Activate()
    {
        // IMPORTANTE: Asegurarse de que el GameObject raíz esté activo
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        
        IsActive = true;
        
        if (Application.isPlaying)
        {
            Debug.Log($"<color=lime>[Room] ✅ '{roomName}' activada</color>");
        }
    }
    
    /// <summary>
    /// Desactiva la habitación (la hace invisible o la destruye)
    /// </summary>
    public void Deactivate()
    {
        if (destroyWhenDeactivated)
        {
            // Destruir la habitación después del delay
            if (Application.isPlaying)
            {
                Debug.Log($"<color=red>[Room] 💥 '{roomName}' será destruida en {destroyDelay:F1}s</color>");
                Destroy(gameObject, destroyDelay);
            }
        }
        else
        {
            // Solo desactivar visualmente
            IsActive = false;
            
            if (Application.isPlaying)
            {
                Debug.Log($"<color=orange>[Room] ❌ '{roomName}' desactivada</color>");
            }
        }
    }
    
    /// <summary>
    /// Destruye la habitación inmediatamente (sin delay)
    /// </summary>
    public void DestroyImmediate()
    {
        if (Application.isPlaying)
        {
            Debug.Log($"<color=red>[Room] 💥 '{roomName}' destruida inmediatamente</color>");
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Actualiza la visibilidad de todos los elementos visuales de la habitación
    /// </summary>
    private void UpdateRoomVisibility()
    {
        if (showDebugLogs && Application.isPlaying)
        {
            Debug.Log($"<color=cyan>[Room] 🔧 Actualizando visibilidad de '{roomName}': {(isActive ? "ACTIVAR" : "DESACTIVAR")}</color>");
        }
        
        // Obtener todos los Renderers (meshes, sprites, etc.) - INCLUIR INACTIVOS
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true); // true = incluir inactivos
        
        int rendererCount = 0;
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = isActive;
            rendererCount++;
        }
        
        // También desactivar/activar luces si las hay - INCLUIR INACTIVOS
        Light[] lights = GetComponentsInChildren<Light>(true);
        
        int lightCount = 0;
        foreach (Light light in lights)
        {
            light.enabled = isActive;
            lightCount++;
        }
        
        // Desactivar/activar colliders (excepto los excluidos por tag) - INCLUIR INACTIVOS
        if (disableCollidersWhenInactive)
        {
            Collider[] colliders = GetComponentsInChildren<Collider>(true); // true = incluir inactivos
            
            int collidersModified = 0;
            int collidersSkipped = 0;
            int triggersModified = 0;
            
            foreach (Collider col in colliders)
            {
                // Verificar si está excluido por tag
                bool shouldExclude = false;
                foreach (string excludedTag in excludedColliderTags)
                {
                    if (col.CompareTag(excludedTag))
                    {
                        shouldExclude = true;
                        break;
                    }
                }
                
                if (shouldExclude)
                {
                    collidersSkipped++;
                    
                    if (showDebugLogs && Application.isPlaying)
                    {
                        Debug.Log($"<color=yellow>[Room]       └─ Collider excluido (tag): {col.gameObject.name}</color>");
                    }
                    continue;
                }
                
                // NUEVA LÓGICA SIMPLIFICADA:
                // TODOS los colliders (normales y triggers) siguen el estado de la habitación
                bool wasEnabled = col.enabled;
                col.enabled = isActive;
                
                // Contar separadamente triggers y colliders normales para debugging
                if (col.isTrigger)
                {
                    triggersModified++;
                    
                    if (showDebugLogs && Application.isPlaying && wasEnabled != isActive)
                    {
                        string action = isActive ? "Activando" : "Desactivando";
                        Debug.Log($"<color=lime>[Room]       └─ {action} trigger: {col.gameObject.name}</color>");
                    }
                }
                else
                {
                    collidersModified++;
                }
            }
            
            if (showDebugLogs && Application.isPlaying)
            {
                Debug.Log($"<color=cyan>[Room]    • Renderers: {rendererCount} {(isActive ? "activados" : "desactivados")}</color>");
                Debug.Log($"<color=cyan>[Room]    • Lights: {lightCount} {(isActive ? "activadas" : "desactivadas")}</color>");
                Debug.Log($"<color=cyan>[Room]    • Colliders normales: {collidersModified} {(isActive ? "activados" : "desactivados")}</color>");
                Debug.Log($"<color=cyan>[Room]    • Triggers: {triggersModified} {(isActive ? "activados" : "desactivados")}</color>");
                Debug.Log($"<color=yellow>[Room]    • Excluidos por tag: {collidersSkipped}</color>");
            }
        }
    }
    
    /// <summary>
    /// Para debugging en el editor
    /// </summary>
    private void OnValidate()
    {
        // Si cambias el valor en el Inspector, actualiza la visibilidad
        if (Application.isPlaying)
        {
            UpdateRoomVisibility();
        }
    }
    
    /// <summary>
    /// Se llama cuando el objeto es destruido
    /// </summary>
    private void OnDestroy()
    {
        if (Application.isPlaying)
        {
            Debug.Log($"<color=grey>[Room] 🗑️ '{roomName}' ha sido destruida y liberada de memoria</color>");
        }
    }
}
