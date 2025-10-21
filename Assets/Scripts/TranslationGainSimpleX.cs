using UnityEngine;

/// <summary>
/// Controlador de Translation Gain SIMPLIFICADO solo para eje X.
/// Versión de prueba para comprimir 7m virtuales en 4m físicos.
/// </summary>
public class TranslationGainSimpleX : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Transform del OVRCameraRig (se busca automáticamente si está vacío)")]
    public Transform ovrCameraRig;
    
    [Tooltip("Transform del VirtualWorld (contenedor de habitaciones)")]
    public Transform virtualWorld;
    
    [Header("Gain Settings - SOLO EJE X")]
    [Tooltip("Factor de amplificación SOLO en X (1.0 = sin gain, 1.75 = comprime 7m en 4m)")]
    [Range(1.0f, 2.0f)]
    public float translationGainX = 1.75f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private bool showGizmos = true;
    [Tooltip("Solo mostrar logs cuando el movimiento sea mayor a este umbral")]
    [Range(0.001f, 0.1f)]
    public float logThreshold = 0.01f;
    
    // Variables privadas
    private Transform centerEyeAnchor;
    private Vector3 lastRealPosition;
    private float totalRealDistanceX = 0f;
    private float totalVirtualDistanceX = 0f;
    private Vector3 initialPhysicalCenter;
    
    private void Start()
    {
        // Buscar OVRCameraRig automáticamente si no está asignado
        if (ovrCameraRig == null)
        {
            // Intentar encontrar por el nombre común del nuevo sistema
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("CameraRig") || obj.name.Contains("XR Origin") || obj.name.Contains("XROrigin"))
                {
                    ovrCameraRig = obj.transform;
                    LogSuccess($"CameraRig encontrado automáticamente: {obj.name}");
                    break;
                }
            }
            
            if (ovrCameraRig == null)
            {
                LogError("No se encontró CameraRig. Asigna manualmente el rig en el Inspector.");
                enabled = false;
                return;
            }
        }
        
        // IMPORTANTE: Buscar el CenterEyeAnchor (la cámara que SÍ se mueve)
        centerEyeAnchor = FindCenterEyeAnchor(ovrCameraRig);
        
        if (centerEyeAnchor == null)
        {
            LogError("No se encontró CenterEyeAnchor. El tracking no funcionará correctamente.");
            LogError("Busca en el CameraRig un objeto hijo llamado 'CenterEyeAnchor' o 'Main Camera'");
            enabled = false;
            return;
        }
        
        LogSuccess($"CenterEyeAnchor encontrado: {centerEyeAnchor.name}");
        
        if (virtualWorld == null)
        {
            LogError("No se asignó VirtualWorld. Deshabilitando script.");
            enabled = false;
            return;
        }
        
        // Inicializar posición de referencia usando el CenterEyeAnchor
        lastRealPosition = centerEyeAnchor.position;
        initialPhysicalCenter = centerEyeAnchor.position;
        
        LogSuccess($"Translation Gain iniciado con factor {translationGainX:F2}x (solo eje X)");
    }
    
    /// <summary>
    /// Busca el CenterEyeAnchor dentro del CameraRig
    /// </summary>
    private Transform FindCenterEyeAnchor(Transform rig)
    {
        // Buscar por nombre común
        string[] possibleNames = { "CenterEyeAnchor", "Main Camera", "Camera", "Center" };
        
        foreach (string name in possibleNames)
        {
            Transform found = rig.Find(name);
            if (found != null)
            {
                return found;
            }
            
            // Buscar recursivamente en todos los hijos
            found = FindInChildren(rig, name);
            if (found != null)
            {
                return found;
            }
        }
        
        // Si no encontró por nombre, buscar la primera cámara
        Camera cam = rig.GetComponentInChildren<Camera>();
        if (cam != null)
        {
            return cam.transform;
        }
        
        return null;
    }
    
    /// <summary>
    /// Busca recursivamente en los hijos
    /// </summary>
    private Transform FindInChildren(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name.Contains(name))
            {
                return child;
            }
            
            Transform found = FindInChildren(child, name);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }
    
    private void LateUpdate()
    {
        // LateUpdate para ejecutar DESPUÉS del tracking del Meta SDK
        ApplyGainX();
    }
    
    /// <summary>
    /// Aplica el translation gain SOLO en el eje X de forma CONTINUA
    /// </summary>
    private void ApplyGainX()
    {
        if (centerEyeAnchor == null) return;
        
        // Calcular movimiento REAL del jugador en X usando el CenterEyeAnchor
        Vector3 currentRealPos = centerEyeAnchor.position;
        float realMovementX = currentRealPos.x - lastRealPosition.x;
        
        // Si no hubo movimiento significativo, salir
        if (Mathf.Abs(realMovementX) < 0.0001f)
        {
            lastRealPosition = currentRealPos;
            return;
        }
        
        // FÓRMULA CORRECTA: Desplazamiento del mundo = -(gain - 1) * movimiento_real
        float gainFactor = translationGainX - 1f;
        float worldMovementX = -gainFactor * realMovementX;
        
        // Aplicar SOLO en X, mantener Y y Z intactos
        Vector3 worldOffset = new Vector3(worldMovementX, 0f, 0f);
        
        // Mover SOLO el mundo, el rig ya se mueve por sí mismo con el tracking
        virtualWorld.position += worldOffset;
        
        // Estadísticas
        totalRealDistanceX += Mathf.Abs(realMovementX);
        totalVirtualDistanceX += Mathf.Abs(realMovementX * translationGainX);
        
        // Log solo si el movimiento es significativo
        if (showDebugLogs && Mathf.Abs(realMovementX) > logThreshold)
        {
            string direction = realMovementX > 0 ? "→" : "←";
            LogMovement($"{direction} Real: {realMovementX:F3}m | Mundo: {worldMovementX:F3}m | Percibido: {(realMovementX * translationGainX):F3}m");
        }
        
        // Actualizar posición anterior
        lastRealPosition = currentRealPos;
    }
    
    /// <summary>
    /// Recentra al jugador físicamente compensando con el mundo virtual
    /// SOLO en eje X - SE LLAMA MANUALMENTE desde TransitionTrigger
    /// </summary>
    public void RecenterPlayerX()
    {
        if (centerEyeAnchor == null) return;
        
        // Obtener posición actual del jugador en X
        float playerCurrentX = centerEyeAnchor.position.x;
        
        // Calcular offset respecto al centro físico inicial
        float offsetX = playerCurrentX - initialPhysicalCenter.x;
        
        // Mover SOLO el mundo para compensar (solo X)
        virtualWorld.position += new Vector3(offsetX, 0f, 0f);
        
        // Actualizar referencia
        lastRealPosition = centerEyeAnchor.position;
        
        LogRecenter($"Jugador recentrado. Offset compensado: {offsetX:F2}m");
        LogInfo($"Nueva posición mundo: X={virtualWorld.position.x:F2}m");
    }
    
    /// <summary>
    /// Muestra estadísticas acumuladas
    /// </summary>
    public void ShowStats()
    {
        Debug.Log("<color=cyan>====== TRANSLATION GAIN STATS ======</color>");
        Debug.Log($"<color=white>📏 Distancia REAL caminada (X): {totalRealDistanceX:F2}m</color>");
        Debug.Log($"<color=white>🌍 Distancia VIRTUAL percibida (X): {totalVirtualDistanceX:F2}m</color>");
        Debug.Log($"<color=white>📊 Ratio efectivo: {(totalVirtualDistanceX / Mathf.Max(totalRealDistanceX, 0.001f)):F2}x</color>");
        Debug.Log($"<color=lime>💾 Ahorro de espacio físico: {(totalVirtualDistanceX - totalRealDistanceX):F2}m</color>");
        Debug.Log("<color=cyan>====================================</color>");
    }
    
    // Métodos de logging con colores
    private void LogSuccess(string message)
    {
        if (showDebugLogs)
            Debug.Log($"<color=lime><b>[GainX]</b> ✅ {message}</color>");
    }
    
    private void LogInfo(string message)
    {
        if (showDebugLogs)
            Debug.Log($"<color=cyan><b>[GainX]</b> ℹ️ {message}</color>");
    }
    
    private void LogMovement(string message)
    {
        if (showDebugLogs)
            Debug.Log($"<color=yellow><b>[GainX]</b> 🚶 {message}</color>");
    }
    
    private void LogRecenter(string message)
    {
        if (showDebugLogs)
            Debug.Log($"<color=magenta><b>[GainX]</b> 🔄 {message}</color>");
    }
    
    private void LogError(string message)
    {
        Debug.LogError($"<color=red><b>[GainX]</b> ❌ {message}</color>");
    }
    
    /// <summary>
    /// Visualización en Scene view
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showGizmos || centerEyeAnchor == null) return;
        
        // Dibujar línea vertical en la posición X del jugador
        Gizmos.color = Color.blue;
        Vector3 playerPos = centerEyeAnchor.position;
        Gizmos.DrawLine(playerPos + Vector3.up * 3f, playerPos - Vector3.up * 0.5f);
        
        // Dibujar esfera en la cabeza del jugador
        Gizmos.DrawWireSphere(playerPos + Vector3.up * 0.2f, 0.2f);
        
        #if UNITY_EDITOR
        // Texto de posición (solo visible en Scene)
        UnityEditor.Handles.Label(playerPos + Vector3.up * 0.5f, $"Camera X: {playerPos.x:F2}m");
        #endif
    }
}
