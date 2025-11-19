using UnityEngine;

/// <summary>
/// Reproduce un sonido cada vez que el jugador avanza 1 unidad en Unity.
/// Útil para calibrar escalas y medir distancias en VR.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class UnitStepSoundTracker : MonoBehaviour
{
    [Header("Tracking Settings")]
    [Tooltip("Transform a rastrear (CenterEyeAnchor del jugador)")]
    public Transform targetTransform;
    
    [Tooltip("Distancia en unidades de Unity para activar el sonido")]
    [Range(0.1f, 5f)]
    public float unitDistance = 1.0f;
    
    [Tooltip("Rastrear movimiento en el eje X")]
    public bool trackX = true;
    
    [Tooltip("Rastrear movimiento en el eje Z")]
    public bool trackZ = true;
    
    [Header("Sound Settings")]
    [Tooltip("Sonido a reproducir cada unidad (deja vacío para usar beep por defecto)")]
    public AudioClip stepSound;
    
    [Tooltip("Volumen del sonido (0-1)")]
    [Range(0f, 1f)]
    public float volume = 0.5f;
    
    [Tooltip("Pitch del sonido (0.5-2)")]
    [Range(0.5f, 2f)]
    public float pitch = 1.0f;
    
    [Tooltip("Usar pitch diferente para X y Z")]
    public bool useDifferentPitchForAxes = true;
    
    [Tooltip("Pitch para movimiento en X")]
    [Range(0.5f, 2f)]
    public float pitchX = 1.0f;
    
    [Tooltip("Pitch para movimiento en Z")]
    [Range(0.5f, 2f)]
    public float pitchZ = 1.2f;
    
    [Header("Visual Feedback")]
    [Tooltip("Mostrar logs en consola")]
    public bool showLogs = true;
    
    [Tooltip("Mostrar marcadores visuales en Scene")]
    public bool showGizmos = true;
    
    // Variables privadas
    private AudioSource audioSource;
    private Vector3 lastPosition;
    private float nextTriggerX;
    private float nextTriggerZ;
    private int stepsX = 0;
    private int stepsZ = 0;
    private Vector3 startPosition;
    
    private void Start()
    {
        // Obtener AudioSource
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        
        // Si no hay clip asignado, generar un beep sintético
        if (stepSound != null)
        {
            audioSource.clip = stepSound;
        }
        
        // Buscar el CenterEyeAnchor si no está asignado
        if (targetTransform == null)
        {
            targetTransform = FindCenterEyeAnchor();
        }
        
        if (targetTransform == null)
        {
            Debug.LogError("<color=red>[UnitStepSound] ? No se encontró transform a rastrear</color>");
            enabled = false;
            return;
        }
        
        // Inicializar posiciones
        startPosition = targetTransform.position;
        lastPosition = startPosition;
        nextTriggerX = Mathf.Floor(startPosition.x / unitDistance) * unitDistance + unitDistance;
        nextTriggerZ = Mathf.Floor(startPosition.z / unitDistance) * unitDistance + unitDistance;
        
        if (showLogs)
        {
            Debug.Log($"<color=lime>[UnitStepSound] ? Rastreador iniciado</color>");
            Debug.Log($"<color=cyan>   • Posición inicial: X={startPosition.x:F2}, Z={startPosition.z:F2}</color>");
            Debug.Log($"<color=cyan>   • Sonido cada {unitDistance:F1} unidades</color>");
            Debug.Log($"<color=cyan>   • Rastreo: X={trackX}, Z={trackZ}</color>");
        }
    }
    
    private Transform FindCenterEyeAnchor()
    {
        // Buscar en OVRCameraRig
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("CameraRig") || obj.name.Contains("XR Origin"))
            {
                Transform rig = obj.transform;
                
                // Buscar CenterEyeAnchor
                string[] possibleNames = { "CenterEyeAnchor", "Main Camera", "Camera" };
                foreach (string name in possibleNames)
                {
                    Transform found = rig.Find(name);
                    if (found != null) return found;
                    
                    found = FindInChildren(rig, name);
                    if (found != null) return found;
                }
                
                // Buscar cámara
                Camera cam = rig.GetComponentInChildren<Camera>();
                if (cam != null) return cam.transform;
            }
        }
        return null;
    }
    
    private Transform FindInChildren(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name.Contains(name)) return child;
            
            Transform found = FindInChildren(child, name);
            if (found != null) return found;
        }
        return null;
    }
    
    private void Update()
    {
        if (targetTransform == null) return;
        
        Vector3 currentPos = targetTransform.position;
        
        // Verificar cruce de unidades en X
        if (trackX)
        {
            CheckAxisCrossing(currentPos.x, lastPosition.x, ref nextTriggerX, "X", pitchX, ref stepsX);
        }
        
        // Verificar cruce de unidades en Z
        if (trackZ)
        {
            CheckAxisCrossing(currentPos.z, lastPosition.z, ref nextTriggerZ, "Z", pitchZ, ref stepsZ);
        }
        
        lastPosition = currentPos;
    }
    
    private void CheckAxisCrossing(float currentValue, float lastValue, ref float nextTrigger, string axisName, float axisPitch, ref int stepCount)
    {
        // Movimiento positivo
        if (currentValue >= nextTrigger && lastValue < nextTrigger)
        {
            PlaySound(axisPitch);
            stepCount++;
            nextTrigger += unitDistance;
            
            if (showLogs)
            {
                string direction = axisName == "X" ? "?" : "?";
                Debug.Log($"<color=yellow>[UnitStep] {direction} Cruzado {axisName} = {nextTrigger - unitDistance:F1} | Total pasos: {stepCount}</color>");
            }
        }
        // Movimiento negativo
        else if (currentValue <= nextTrigger - unitDistance && lastValue > nextTrigger - unitDistance)
        {
            PlaySound(axisPitch);
            stepCount++;
            nextTrigger -= unitDistance;
            
            if (showLogs)
            {
                string direction = axisName == "X" ? "?" : "?";
                Debug.Log($"<color=yellow>[UnitStep] {direction} Cruzado {axisName} = {nextTrigger:F1} | Total pasos: {stepCount}</color>");
            }
        }
    }
    
    private void PlaySound(float customPitch)
    {
        if (audioSource == null) return;
        
        // Aplicar pitch personalizado si está habilitado
        if (useDifferentPitchForAxes)
        {
            audioSource.pitch = customPitch;
        }
        else
        {
            audioSource.pitch = pitch;
        }
        
        // Reproducir sonido
        if (stepSound != null)
        {
            audioSource.PlayOneShot(stepSound, volume);
        }
        else
        {
            // Si no hay clip, reproducir un beep simple
            audioSource.PlayOneShot(AudioClip.Create("Beep", 4410, 1, 44100, false), volume);
        }
    }
    
    /// <summary>
    /// Reinicia el conteo de pasos
    /// </summary>
    [ContextMenu("Reset Step Count")]
    public void ResetStepCount()
    {
        stepsX = 0;
        stepsZ = 0;
        
        if (targetTransform != null)
        {
            startPosition = targetTransform.position;
            lastPosition = startPosition;
            nextTriggerX = Mathf.Floor(startPosition.x / unitDistance) * unitDistance + unitDistance;
            nextTriggerZ = Mathf.Floor(startPosition.z / unitDistance) * unitDistance + unitDistance;
        }
        
        Debug.Log("<color=lime>[UnitStepSound] ?? Conteo reiniciado</color>");
    }
    
    /// <summary>
    /// Muestra estadísticas
    /// </summary>
    [ContextMenu("Show Statistics")]
    public void ShowStats()
    {
        Debug.Log("<color=cyan>??? UNIT STEP TRACKER STATS ???</color>");
        Debug.Log($"<color=white>?? Distancia total:</color>");
        Debug.Log($"<color=white>   • Pasos en X: {stepsX} × {unitDistance:F1} = {stepsX * unitDistance:F2} unidades</color>");
        Debug.Log($"<color=white>   • Pasos en Z: {stepsZ} × {unitDistance:F1} = {stepsZ * unitDistance:F2} unidades</color>");
        
        if (targetTransform != null)
        {
            Vector3 currentPos = targetTransform.position;
            Vector3 delta = currentPos - startPosition;
            Debug.Log($"<color=white>?? Posición actual:</color>");
            Debug.Log($"<color=white>   • X: {currentPos.x:F2} (? {delta.x:F2})</color>");
            Debug.Log($"<color=white>   • Z: {currentPos.z:F2} (? {delta.z:F2})</color>");
        }
        
        Debug.Log("<color=cyan>???????????????????????????????</color>");
    }
    
    private void OnDrawGizmos()
    {
        if (!showGizmos || targetTransform == null) return;
        
        Vector3 pos = targetTransform.position;
        
        // Dibujar cuadrícula de 1 unidad alrededor del jugador
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        
        // Líneas verticales (paralelas a Z)
        for (int x = -5; x <= 5; x++)
        {
            float xPos = Mathf.Floor(pos.x) + x;
            Gizmos.DrawLine(new Vector3(xPos, 0, pos.z - 5), new Vector3(xPos, 0, pos.z + 5));
        }
        
        // Líneas horizontales (paralelas a X)
        for (int z = -5; z <= 5; z++)
        {
            float zPos = Mathf.Floor(pos.z) + z;
            Gizmos.DrawLine(new Vector3(pos.x - 5, 0, zPos), new Vector3(pos.x + 5, 0, zPos));
        }
        
        // Marcar el próximo trigger en X
        Gizmos.color = Color.red;
        Vector3 nextX = new Vector3(nextTriggerX, pos.y, pos.z);
        Gizmos.DrawWireSphere(nextX, 0.1f);
        
        // Marcar el próximo trigger en Z
        Gizmos.color = Color.blue;
        Vector3 nextZ = new Vector3(pos.x, pos.y, nextTriggerZ);
        Gizmos.DrawWireSphere(nextZ, 0.1f);
        
        #if UNITY_EDITOR
        // Etiquetas
        UnityEditor.Handles.Label(nextX + Vector3.up * 0.5f, $"Next X: {nextTriggerX:F1}");
        UnityEditor.Handles.Label(nextZ + Vector3.up * 0.5f, $"Next Z: {nextTriggerZ:F1}");
        #endif
    }
}
