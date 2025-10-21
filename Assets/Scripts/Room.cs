using UnityEngine;

/// <summary>
/// Representa una habitación individual en el sistema de Impossible Spaces.
/// Permite activar/desactivar la habitación de forma visual.
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
    
    [Tooltip("Tags de colliders que NO deben desactivarse (ej: triggers especiales)")]
    public string[] excludedColliderTags = new string[0];
    
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
        IsActive = true;
    }
    
    /// <summary>
    /// Desactiva la habitación (la hace invisible)
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }
    
    /// <summary>
    /// Actualiza la visibilidad de todos los elementos visuales de la habitación
    /// </summary>
    private void UpdateRoomVisibility()
    {
        // Obtener todos los Renderers (meshes, sprites, etc.) en la habitación y sus hijos
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = isActive;
        }
        
        // También desactivar/activar luces si las hay
        Light[] lights = GetComponentsInChildren<Light>();
        foreach (Light light in lights)
        {
            light.enabled = isActive;
        }
        
        // Desactivar/activar colliders (excepto los excluidos por tag)
        if (disableCollidersWhenInactive)
        {
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders)
            {
                // No desactivar colliders excluidos por tag
                bool shouldExclude = false;
                foreach (string excludedTag in excludedColliderTags)
                {
                    if (col.CompareTag(excludedTag))
                    {
                        shouldExclude = true;
                        break;
                    }
                }
                
                if (!shouldExclude)
                {
                    col.enabled = isActive;
                }
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
}
