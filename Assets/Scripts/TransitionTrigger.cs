using UnityEngine;

/// <summary>
/// Trigger invisible que detecta al jugador y activa/desactiva habitaciones.
/// Coloca este script en un GameObject con BoxCollider (isTrigger = true).
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class TransitionTrigger : MonoBehaviour
{
    [Header("Transition Configuration")]
    [Tooltip("Habitación que se ACTIVARÁ cuando el jugador cruce")]
    public Room roomToActivate;
    
    [Tooltip("Habitación que se DESACTIVARÁ cuando el jugador cruce")]
    public Room roomToDeactivate;
    
    [Header("Trigger Settings")]
    [Tooltip("Tag del jugador (normalmente 'Player')")]
    public string playerTag = "Player";
    
    [Tooltip("¿Permitir que el trigger se active múltiples veces?")]
    public bool allowMultipleActivations = false;
    
    [Header("VR Safety Settings")]
    [Tooltip("GameObject del quad de WallPenetrationTunneling (se desactiva temporalmente)")]
    public GameObject wallPenetrationQuad;
    
    [Tooltip("¿Buscar automáticamente el WallPenetrationTunneling en el jugador?")]
    public bool autoFindWallPenetration = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugMessages = true;
    
    private BoxCollider triggerCollider;
    private bool hasBeenTriggered = false;
    private bool isPlayerInside = false;
    
    private void Awake()
    {
        // Obtener el collider y asegurarse de que sea trigger
        triggerCollider = GetComponent<BoxCollider>();
        triggerCollider.isTrigger = true;
        
        // Configurar tamaño predeterminado: trigger delgado como una "lámina"
        if (triggerCollider.size.magnitude < 0.1f)
        {
            triggerCollider.size = new Vector3(2f, 2.5f, 0.1f); // MUY delgado en Z
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Verificar si es el jugador
        if (other.CompareTag(playerTag))
        {
            isPlayerInside = true;
            
            Debug.Log($"<color=yellow><b>[TRIGGER]</b> ?? Jugador entró al trigger '{gameObject.name}'</color>");
            
            // Buscar automáticamente el WallPenetrationTunneling si está habilitado
            if (autoFindWallPenetration && wallPenetrationQuad == null)
            {
                FindWallPenetrationQuad(other.transform);
            }
            
            // Desactivar temporalmente el quad de penetración
            if (wallPenetrationQuad != null)
            {
                wallPenetrationQuad.SetActive(false);
                
                if (showDebugMessages)
                {
                    Debug.Log("<color=yellow><b>[TRIGGER]</b> ?? WallPenetrationQuad desactivado temporalmente</color>");
                }
            }
            
            // Ejecutar transición si no ha sido activado
            if (!hasBeenTriggered || allowMultipleActivations)
            {
                ExecuteTransition();
            }
            else
            {
                Debug.Log("<color=grey><b>[TRIGGER]</b> ?? Trigger ya fue activado (ignorando)</color>");
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        // Verificar si es el jugador
        if (other.CompareTag(playerTag))
        {
            isPlayerInside = false;
            
            Debug.Log($"<color=yellow><b>[TRIGGER]</b> ?? Jugador salió del trigger '{gameObject.name}'</color>");
            
            // Reactivar el quad de penetración
            if (wallPenetrationQuad != null)
            {
                wallPenetrationQuad.SetActive(true);
                
                if (showDebugMessages)
                {
                    Debug.Log("<color=yellow><b>[TRIGGER]</b> ?? WallPenetrationQuad reactivado</color>");
                }
            }
        }
    }
    
    /// <summary>
    /// Busca automáticamente el GameObject de WallPenetrationTunneling en el jugador
    /// </summary>
    private void FindWallPenetrationQuad(Transform playerTransform)
    {
        // Buscar en todo el hierarchy del jugador
        Transform[] allChildren = playerTransform.GetComponentsInChildren<Transform>(true);
        
        foreach (Transform child in allChildren)
        {
            // Buscar por nombre común del objeto
            if (child.name.Contains("WallPenetration") || 
                child.name.Contains("Tunneling") ||
                child.name.Contains("Fade"))
            {
                wallPenetrationQuad = child.gameObject;
                
                if (showDebugMessages)
                {
                    Debug.Log($"[TransitionTrigger] WallPenetrationQuad encontrado automáticamente: {child.name}");
                }
                
                return;
            }
        }
        
        if (showDebugMessages)
        {
            Debug.LogWarning("[TransitionTrigger] No se pudo encontrar WallPenetrationQuad automáticamente");
        }
    }
    
    /// <summary>
    /// Ejecuta la transición entre habitaciones
    /// </summary>
    private void ExecuteTransition()
    {
        Debug.Log("<color=cyan>════════════════════════════════════</color>");
        Debug.Log($"<color=lime><b>[TRANSITION]</b> 🚪 Transición activada en '{gameObject.name}'</color>");
        
        // Activar habitación destino
        if (roomToActivate != null)
        {
            roomToActivate.Activate();
            
            if (showDebugMessages)
            {
                Debug.Log($"<color=lime><b>[TRANSITION]</b> ✅ Habitación activada: <b>{roomToActivate.roomName}</b></color>");
            }
        }
        else if (showDebugMessages)
        {
            Debug.LogWarning("<color=orange><b>[TRANSITION]</b> ⚠️ No hay habitación asignada para activar</color>");
        }
        
        // Desactivar habitación anterior
        if (roomToDeactivate != null)
        {
            roomToDeactivate.Deactivate();
            
            if (showDebugMessages)
            {
                Debug.Log($"<color=red><b>[TRANSITION]</b> ❌ Habitación desactivada: <b>{roomToDeactivate.roomName}</b></color>");
            }
        }
        else if (showDebugMessages)
        {
            Debug.LogWarning("<color=orange><b>[TRANSITION]</b> ⚠️ No hay habitación asignada para desactivar</color>");
        }
        
        Debug.Log("<color=cyan>════════════════════════════════════</color>");
        
        // Marcar como activado
        hasBeenTriggered = true;
    }
    
    /// <summary>
    /// Reinicia el trigger para permitir que vuelva a activarse
    /// </summary>
    public void ResetTrigger()
    {
        hasBeenTriggered = false;
        
        if (showDebugMessages)
        {
            Debug.Log($"[TransitionTrigger] Trigger '{gameObject.name}' reiniciado");
        }
    }
    
    /// <summary>
    /// Visualización en el editor (cubo cyan semi-transparente)
    /// </summary>
    private void OnDrawGizmos()
    {
        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.3f); // Cyan transparente
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(col.center, col.size);
            
            // Borde más visible
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(col.center, col.size);
        }
    }
}
