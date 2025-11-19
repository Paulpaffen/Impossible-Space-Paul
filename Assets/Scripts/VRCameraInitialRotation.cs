using UnityEngine;

/// <summary>
/// Fuerza la rotación inicial del Camera Rig de VR al iniciar el juego.
/// Solo afecta la rotación Y (horizontal), manteniendo X y Z sin cambios.
/// Se ejecuta una sola vez al inicio y luego se desactiva.
/// </summary>
public class VRCameraInitialRotation : MonoBehaviour
{
    [Header("Camera Rig Reference")]
    [Tooltip("Referencia al Camera Rig/XR Origin (arrastra aquí el objeto raíz de VR)")]
    public Transform cameraRig;
    
    [Header("Initial Rotation")]
    [Tooltip("Rotación Y inicial deseada (en grados)")]
    [Range(-180f, 180f)]
    public float initialYRotation = -90f;
    
    [Header("Debug")]
    [Tooltip("Mostrar logs de debug")]
    public bool showLogs = true;
    
    private bool hasBeenInitialized = false;
    
    private void Start()
    {
        // Si no hay referencia, intentar encontrarla automáticamente
        if (cameraRig == null)
        {
            AutoDetectCameraRig();
        }
        
        // Aplicar la rotación inicial
        if (cameraRig != null && !hasBeenInitialized)
        {
            SetInitialRotation();
            hasBeenInitialized = true;
        }
        else if (cameraRig == null)
        {
            Debug.LogError("<color=red>[VR Rotation] ? No se encontró el Camera Rig. Por favor asigna la referencia manualmente.</color>");
        }
    }
    
    /// <summary>
    /// Intenta detectar automáticamente el Camera Rig
    /// </summary>
    private void AutoDetectCameraRig()
    {
        // Buscar por nombres comunes de XR Rigs
        string[] possibleNames = { "XR Origin", "XRRig", "Camera Rig", "CameraRig", "VR Rig", "VRRig" };
        
        foreach (string name in possibleNames)
        {
            GameObject found = GameObject.Find(name);
            if (found != null)
            {
                cameraRig = found.transform;
                if (showLogs)
                {
                    Debug.Log($"<color=lime>[VR Rotation] ? Camera Rig detectado automáticamente: {name}</color>");
                }
                return;
            }
        }
        
        if (showLogs)
        {
            Debug.LogWarning("<color=orange>[VR Rotation] ?? No se pudo detectar automáticamente el Camera Rig.</color>");
        }
    }
    
    /// <summary>
    /// Establece la rotación inicial del Camera Rig
    /// </summary>
    private void SetInitialRotation()
    {
        // Guardar la rotación actual
        Vector3 currentRotation = cameraRig.eulerAngles;
        
        if (showLogs)
        {
            Debug.Log($"<color=cyan>[VR Rotation] ?? Rotación original: {currentRotation}</color>");
        }
        
        // Aplicar solo la rotación Y, mantener X y Z
        Vector3 newRotation = new Vector3(
            currentRotation.x,
            initialYRotation,
            currentRotation.z
        );
        
        cameraRig.eulerAngles = newRotation;
        
        if (showLogs)
        {
            Debug.Log($"<color=lime>[VR Rotation] ?? Nueva rotación aplicada: {newRotation}</color>");
            Debug.Log($"<color=yellow>[VR Rotation] ?? Camera Rig ahora mira hacia: {initialYRotation}°</color>");
        }
    }
    
    /// <summary>
    /// Fuerza la rotación manualmente (útil para testing)
    /// </summary>
    [ContextMenu("Force Reset Rotation")]
    public void ForceResetRotation()
    {
        if (cameraRig != null)
        {
            SetInitialRotation();
            Debug.Log("<color=lime>[VR Rotation] ?? Rotación forzada desde el menú contextual</color>");
        }
        else
        {
            Debug.LogWarning("<color=orange>[VR Rotation] ?? No hay Camera Rig asignado</color>");
        }
    }
    
    /// <summary>
    /// Establece una nueva rotación Y y la aplica inmediatamente
    /// </summary>
    public void SetRotationY(float newYRotation)
    {
        initialYRotation = newYRotation;
        if (cameraRig != null)
        {
            SetInitialRotation();
        }
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (cameraRig != null)
        {
            // Dibujar flecha indicando la dirección de visión
            Vector3 forward = Quaternion.Euler(0, initialYRotation, 0) * Vector3.forward;
            Vector3 position = cameraRig.position;
            
            // Flecha principal
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(position, forward * 2f);
            
            // Círculo en el suelo
            UnityEditor.Handles.color = new Color(0, 1, 1, 0.3f);
            UnityEditor.Handles.DrawWireDisc(position, Vector3.up, 1f);
            
            // Etiqueta
            UnityEditor.Handles.Label(position + Vector3.up * 2f, $"VR Inicial\n? {initialYRotation}°");
        }
    }
#endif
}
