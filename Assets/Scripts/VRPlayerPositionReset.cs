using System.Collections;
using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// Resetea la posición del jugador VR a una posición específica al iniciar.
/// Corrige el problema de Guardian boundary y altura del jugador.
/// </summary>
public class VRPlayerPositionReset : MonoBehaviour
{
    [Header("Camera Rig Reference")]
    [Tooltip("Referencia al Camera Rig/XR Origin (el objeto raíz de VR)")]
    public Transform cameraRig;
    
    [Header("Position Settings")]
    [Tooltip("Posición objetivo del jugador (world space)")]
    public Vector3 targetPosition = new Vector3(0f, 0.1f, 0f);
    
    [Tooltip("Si está marcado, resetea la posición en cada Start()")]
    public bool resetOnStart = true;
    
    [Header("Timing")]
    [Tooltip("Segundos de espera antes de hacer el reset (para asegurar que el tracking esté listo)")]
    [Range(0f, 5f)]
    public float delayBeforeReset = 3f;
    
    [Header("Height Correction")]
    [Tooltip("Ajustar automáticamente la altura basándose en el centro del headset")]
    public bool autoCorrectHeight = true;
    
    [Tooltip("Altura del piso virtual (Y position)")]
    public float floorHeight = 0.1f;
    
    [Header("Continuous Height Monitor")]
    [Tooltip("Monitorear continuamente la altura del jugador")]
    public bool enableHeightMonitoring = true;
    
    [Tooltip("Intervalo en segundos para verificar la altura")]
    [Range(1f, 30f)]
    public float heightCheckInterval = 10f;
    
    [Tooltip("Altura máxima permitida del headset en world space")]
    [Range(1.0f, 3.0f)]
    public float maxAllowedHeight = 1.8f;
    
    [Tooltip("Altura objetivo para resetear (normalmente altura de jugador promedio)")]
    [Range(1.0f, 2.5f)]
    public float targetPlayerHeight = 1.6f;
    
    [Header("Debug")]
    [Tooltip("Mostrar logs de debug")]
    public bool showLogs = true;
    
    private Transform cameraTransform;
    private bool hasReset = false;
    private Coroutine heightMonitorCoroutine;
    
    private void Awake()
    {
        // Intentar encontrar el Camera Rig si no está asignado
        if (cameraRig == null)
        {
            AutoDetectCameraRig();
        }
        
        // Encontrar la cámara principal
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }
    
    private void Start()
    {
        if (resetOnStart)
        {
            StartCoroutine(ResetPositionWithDelay());
        }
        
        // Iniciar el monitoreo de altura continuo
        if (enableHeightMonitoring)
        {
            heightMonitorCoroutine = StartCoroutine(MonitorHeightContinuously());
        }
    }
    
    private void OnEnable()
    {
        // Reiniciar el monitoreo si se reactiva el componente
        if (enableHeightMonitoring && heightMonitorCoroutine == null)
        {
            heightMonitorCoroutine = StartCoroutine(MonitorHeightContinuously());
        }
    }
    
    private void OnDisable()
    {
        // Detener el monitoreo si se desactiva el componente
        if (heightMonitorCoroutine != null)
        {
            StopCoroutine(heightMonitorCoroutine);
            heightMonitorCoroutine = null;
        }
    }
    
    /// <summary>
    /// Monitorea continuamente la altura del jugador y corrige si excede el límite
    /// </summary>
    private IEnumerator MonitorHeightContinuously()
    {
        // Esperar un poco antes de comenzar el monitoreo
        yield return new WaitForSeconds(delayBeforeReset + 1f);
        
        if (showLogs)
        {
            Debug.Log($"<color=cyan>[VR Height Monitor] 👁️ Monitoreo de altura iniciado. Verificando cada {heightCheckInterval}s (límite: {maxAllowedHeight}m)</color>");
        }
        
        while (true)
        {
            yield return new WaitForSeconds(heightCheckInterval);
            
            if (cameraTransform != null)
            {
                float currentHeight = cameraTransform.position.y;
                
                if (showLogs)
                {
                    Debug.Log($"<color=cyan>[VR Height Monitor] 📏 Altura actual del headset: {currentHeight:F2}m (límite: {maxAllowedHeight}m)</color>");
                }
                
                // Si la altura excede el límite, corregir
                if (currentHeight > maxAllowedHeight)
                {
                    if (showLogs)
                    {
                        Debug.LogWarning($"<color=yellow>[VR Height Monitor] ⚠️ Altura excedida! {currentHeight:F2}m > {maxAllowedHeight}m. Corrigiendo...</color>");
                    }
                    
                    CorrectHeight();
                }
            }
        }
    }
    
    /// <summary>
    /// Corrige la altura del jugador sin afectar la posición horizontal
    /// </summary>
    private void CorrectHeight()
    {
        if (cameraRig == null || cameraTransform == null)
        {
            Debug.LogError("<color=red>[VR Height Monitor] ❌ No se puede corregir altura: referencias faltantes.</color>");
            return;
        }
        
        // Guardar la posición horizontal actual del rig
        Vector3 currentRigPosition = cameraRig.position;
        
        // Calcular el offset del headset respecto al camera rig
        float headsetHeightOffset = cameraTransform.localPosition.y;
        
        // Calcular la nueva posición Y del rig para que el headset esté a la altura objetivo
        float newRigY = targetPlayerHeight - headsetHeightOffset;
        
        // Aplicar la nueva altura manteniendo X y Z
        cameraRig.position = new Vector3(currentRigPosition.x, newRigY, currentRigPosition.z);
        
        if (showLogs)
        {
            Debug.Log($"<color=lime>[VR Height Monitor] ✅ Altura corregida: Headset ahora en {cameraTransform.position.y:F2}m (objetivo: {targetPlayerHeight}m)</color>");
        }
    }
    
    /// <summary>
    /// Coroutine que espera antes de resetear la posición
    /// </summary>
    private IEnumerator ResetPositionWithDelay()
    {
        if (showLogs)
        {
            Debug.Log($"<color=yellow>[VR Position] ?? Esperando {delayBeforeReset} segundos antes de resetear posición...</color>");
        }
        
        yield return new WaitForSeconds(delayBeforeReset);
        
        ResetPosition();
    }
    
    /// <summary>
    /// Resetea la posición del jugador VR
    /// </summary>
    public void ResetPosition()
    {
        if (cameraRig == null)
        {
            Debug.LogError("<color=red>[VR Position] ? No se encontró el Camera Rig. Asigna la referencia manualmente.</color>");
            return;
        }
        
        if (showLogs)
        {
            Debug.Log($"<color=cyan>[VR Position] ?? Posición original del Camera Rig: {cameraRig.position}</color>");
            if (cameraTransform != null)
            {
                Debug.Log($"<color=cyan>[VR Position] ?? Posición original del Headset: {cameraTransform.position}</color>");
                Debug.Log($"<color=cyan>[VR Position] ?? Altura del jugador sobre el rig: {cameraTransform.localPosition.y}m</color>");
            }
        }
        
        Vector3 newPosition = targetPosition;
        
        // Corrección automática de altura
        if (autoCorrectHeight && cameraTransform != null)
        {
            // Calcular el offset del headset respecto al camera rig
            float headsetHeightOffset = cameraTransform.localPosition.y;
            
            // Ajustar la posición del rig para que los pies del jugador estén en floorHeight
            // La altura real del jugador es la del headset, así que restamos ese offset
            newPosition.y = floorHeight - headsetHeightOffset;
            
            if (showLogs)
            {
                Debug.Log($"<color=lime>[VR Position] ?? Corrección de altura aplicada: Rig Y = {newPosition.y} (floor={floorHeight}, offset={headsetHeightOffset})</color>");
            }
        }
        
        // También podemos tomar en cuenta el offset horizontal del headset
        // para que el punto (0,0,0) sea donde el jugador está parado, no donde está su cabeza
        Vector3 headsetHorizontalOffset = new Vector3(
            cameraTransform.localPosition.x,
            0f,
            cameraTransform.localPosition.z
        );
        
        // Ajustar la posición del rig para compensar donde está la cabeza
        newPosition -= headsetHorizontalOffset;
        
        // Aplicar la nueva posición
        cameraRig.position = newPosition;
        
        if (showLogs)
        {
            Debug.Log($"<color=lime>[VR Position] ? Nueva posición del Camera Rig: {cameraRig.position}</color>");
            if (cameraTransform != null)
            {
                Debug.Log($"<color=lime>[VR Position] ?? Nueva posición del Headset: {cameraTransform.position}</color>");
            }
            Debug.Log("<color=green>[VR Position] ?? Recentrado completado!</color>");
        }
        
        hasReset = true;
    }
    
    /// <summary>
    /// Intenta detectar automáticamente el Camera Rig
    /// </summary>
    private void AutoDetectCameraRig()
    {
        // Primero intentar encontrar por objeto padre
        if (Camera.main != null)
        {
            // El Camera Rig suele ser el padre o abuelo de la cámara
            Transform current = Camera.main.transform;
            
            // Subir hasta 3 niveles buscando el rig
            for (int i = 0; i < 3; i++)
            {
                if (current.parent != null)
                {
                    current = current.parent;
                    
                    // Verificar si tiene nombres típicos de XR Rig
                    if (current.name.Contains("XR") || 
                        current.name.Contains("Rig") || 
                        current.name.Contains("Camera") ||
                        current.name.Contains("VR"))
                    {
                        cameraRig = current;
                        if (showLogs)
                        {
                            Debug.Log($"<color=lime>[VR Position] ?? Camera Rig detectado por jerarquía: {current.name}</color>");
                        }
                        return;
                    }
                }
            }
        }
        
        // Si no se encontró, buscar por nombres comunes
        string[] possibleNames = { 
            "XR Origin", 
            "XRRig", 
            "Camera Rig", 
            "CameraRig", 
            "VR Rig", 
            "VRRig",
            "OVRCameraRig",
            "Player"
        };
        
        foreach (string name in possibleNames)
        {
            GameObject found = GameObject.Find(name);
            if (found != null)
            {
                cameraRig = found.transform;
                if (showLogs)
                {
                    Debug.Log($"<color=lime>[VR Position] ?? Camera Rig detectado por nombre: {name}</color>");
                }
                return;
            }
        }
        
        if (showLogs)
        {
            Debug.LogWarning("<color=orange>[VR Position] ?? No se pudo detectar automáticamente el Camera Rig.</color>");
        }
    }
    
    /// <summary>
    /// Método para llamar desde el inspector o código
    /// </summary>
    [ContextMenu("Reset Position Now")]
    public void ResetPositionNow()
    {
        ResetPosition();
    }
    
    /// <summary>
    /// Método para corregir altura manualmente desde el inspector
    /// </summary>
    [ContextMenu("Correct Height Now")]
    public void CorrectHeightNow()
    {
        CorrectHeight();
    }
    
    /// <summary>
    /// Reinicia la coroutine del reset
    /// </summary>
    [ContextMenu("Restart Reset Coroutine")]
    public void RestartResetCoroutine()
    {
        StopAllCoroutines();
        hasReset = false;
        StartCoroutine(ResetPositionWithDelay());
        
        if (enableHeightMonitoring)
        {
            heightMonitorCoroutine = StartCoroutine(MonitorHeightContinuously());
        }
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Dibujar la posición objetivo
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(targetPosition, 0.2f);
        
        // Dibujar línea desde el origen
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(Vector3.zero, targetPosition);
        
        // Dibujar un plano en el floor height
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Vector3 floorCenter = new Vector3(targetPosition.x, floorHeight, targetPosition.z);
        
        // Dibujar una grid pequeña para visualizar el piso
        for (int x = -2; x <= 2; x++)
        {
            Gizmos.DrawLine(
                floorCenter + new Vector3(x, 0, -2),
                floorCenter + new Vector3(x, 0, 2)
            );
        }
        for (int z = -2; z <= 2; z++)
        {
            Gizmos.DrawLine(
                floorCenter + new Vector3(-2, 0, z),
                floorCenter + new Vector3(2, 0, z)
            );
        }
        
        // Etiqueta
        UnityEditor.Handles.Label(
            targetPosition + Vector3.up * 0.5f, 
            $"Target Position\n?? {targetPosition}\n?? Floor: {floorHeight}m"
        );
        
        // Si hay camera rig asignado, mostrar su posición actual
        if (cameraRig != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(cameraRig.position, Vector3.one * 0.3f);
            
            UnityEditor.Handles.color = Color.cyan;
            UnityEditor.Handles.Label(
                cameraRig.position + Vector3.up, 
                $"Current Rig\n?? {cameraRig.position}"
            );
            
            // Línea desde la posición actual a la objetivo
            Gizmos.color = Color.red;
            Gizmos.DrawLine(cameraRig.position, targetPosition);
        }
    }
#endif
}
