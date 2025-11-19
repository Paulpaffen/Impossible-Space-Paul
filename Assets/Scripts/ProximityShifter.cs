using UnityEngine;

/// <summary>
/// Script para hacer que un objeto (ej: botón) se mueva a posiciones específicas cuando el jugador se acerca.
/// Útil para puzzles donde el botón "huye" a ubicaciones predefinidas.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ProximityShifter : MonoBehaviour
{
    [Header("Target Object")]
    [Tooltip("Objeto que se moverá/rotará (ej: el botón)")]
    public Transform targetObject;
    
    [Header("Shift Positions")]
    [Tooltip("Lista de posiciones a las que se moverá el botón (en coordenadas del mundo)")]
    public ShiftData[] shiftPositions;
    
    [System.Serializable]
    public class ShiftData
    {
        [Header("Position")]
        public Vector3 position;
        
        [Header("Rotation (Euler Angles)")]
        public Vector3 rotation;
        
        [Header("Settings")]
        [Tooltip("Delay antes de moverse a esta posición")]
        [Range(0f, 2f)]
        public float delay = 0.2f;
        
        [Tooltip("Velocidad de animación para este movimiento (0 = instantáneo)")]
        [Range(0f, 10f)]
        public float animationSpeed = 5f;
        
        [Header("Audio")]
        [Tooltip("Sonido específico para este movimiento (opcional)")]
        public AudioClip sound;
    }
    
    [Header("Trigger Settings")]
    [Tooltip("Tag del jugador o mano")]
    public string triggerTag = "Player";
    
    [Tooltip("Usar manos específicas (dejar vacío para cualquier tag)")]
    public string[] handTags = new string[] { "HandLeft", "HandRight" };
    
    [Header("Audio Settings")]
    [Tooltip("Sonido por defecto si no hay uno específico en ShiftData")]
    public AudioClip defaultShiftSound;
    
    [Tooltip("Volumen del sonido")]
    [Range(0f, 1f)]
    public float volume = 0.7f;
    
    [Header("Visual Feedback")]
    [Tooltip("Mostrar logs en consola")]
    public bool showLogs = true;
    
    [Tooltip("Color del gizmo en Scene view")]
    public Color gizmoColor = new Color(1f, 0.5f, 0f, 0.3f); // Naranja transparente
    
    // Estado interno
    private int currentShiftIndex = 0;
    private bool isShifting = false;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private float currentAnimationSpeed;
    private AudioSource audioSource;
    private Collider triggerCollider;
    
    public int MaxShifts => shiftPositions != null ? shiftPositions.Length : 0;
    
    private void Start()
    {
        // Verificar que hay un objeto asignado
        if (targetObject == null)
        {
            Debug.LogError("<color=red>[ProximityShifter] ❌ No hay Target Object asignado</color>");
            enabled = false;
            return;
        }
        
        // Verificar que hay posiciones definidas
        if (shiftPositions == null || shiftPositions.Length == 0)
        {
            Debug.LogError("<color=red>[ProximityShifter] ❌ No hay posiciones definidas en Shift Positions</color>");
            enabled = false;
            return;
        }
        
        // Configurar collider como trigger
        triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;
        
        // Crear AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D sound
        
        // Inicializar posición y rotación objetivo
        targetPosition = targetObject.position;
        targetRotation = targetObject.rotation;
        currentAnimationSpeed = shiftPositions[0].animationSpeed;
        
        if (showLogs)
        {
            Debug.Log($"<color=cyan>[ProximityShifter] 🎯 Iniciado. Movimientos disponibles: {MaxShifts}</color>");
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Si ya se alcanzaron todas las posiciones, ignorar
        if (currentShiftIndex >= MaxShifts)
        {
            return;
        }
        
        // Si ya está en proceso de moverse, ignorar
        if (isShifting)
        {
            return;
        }
        
        // Verificar si es el jugador o una mano
        bool isValidTrigger = false;
        
        // Verificar tag principal
        if (other.CompareTag(triggerTag))
        {
            isValidTrigger = true;
        }
        
        // Verificar tags de manos
        foreach (string handTag in handTags)
        {
            if (!string.IsNullOrEmpty(handTag) && other.CompareTag(handTag))
            {
                isValidTrigger = true;
                break;
            }
        }
        
        if (!isValidTrigger)
        {
            return;
        }
        
        // Ejecutar el movimiento
        if (showLogs)
        {
            Debug.Log($"<color=yellow>[ProximityShifter] 👋 Mano detectada! Moviendo botón...</color>");
        }
        
        // Programar el movimiento con el delay específico de esta posición
        float delay = shiftPositions[currentShiftIndex].delay;
        Invoke(nameof(ExecuteShift), delay);
        isShifting = true;
    }
    
    /// <summary>
    /// Ejecuta el movimiento/rotación del objeto a la siguiente posición
    /// </summary>
    private void ExecuteShift()
    {
        if (targetObject == null || currentShiftIndex >= MaxShifts) return;
        
        // Obtener los datos de este movimiento
        ShiftData shiftData = shiftPositions[currentShiftIndex];
        
        // Establecer nueva posición y rotación objetivo
        targetPosition = shiftData.position;
        targetRotation = Quaternion.Euler(shiftData.rotation);
        currentAnimationSpeed = shiftData.animationSpeed;
        
        // Reproducir sonido (específico o por defecto)
        AudioClip soundToPlay = shiftData.sound != null ? shiftData.sound : defaultShiftSound;
        if (audioSource != null && soundToPlay != null)
        {
            audioSource.PlayOneShot(soundToPlay, volume);
        }
        
        if (showLogs)
        {
            Debug.Log($"<color=lime>[ProximityShifter] 🔄 Movimiento {currentShiftIndex + 1}/{MaxShifts}</color>");
            Debug.Log($"<color=cyan>   Nueva posición: {targetPosition}</color>");
            Debug.Log($"<color=cyan>   Nueva rotación: {shiftData.rotation}</color>");
            
            if (currentShiftIndex >= MaxShifts - 1)
            {
                Debug.Log($"<color=green>[ProximityShifter] ✅ Última posición alcanzada. Botón fijo ahora.</color>");
            }
        }
        
        // Incrementar índice
        currentShiftIndex++;
        isShifting = false;
    }
    
    private void Update()
    {
        if (targetObject == null) return;
        
        // Animar el movimiento si la velocidad > 0
        if (currentAnimationSpeed > 0f)
        {
            // Interpolar posición
            targetObject.position = Vector3.Lerp(
                targetObject.position, 
                targetPosition, 
                Time.deltaTime * currentAnimationSpeed
            );
            
            // Interpolar rotación
            targetObject.rotation = Quaternion.Slerp(
                targetObject.rotation, 
                targetRotation, 
                Time.deltaTime * currentAnimationSpeed
            );
        }
        else
        {
            // Movimiento instantáneo
            targetObject.position = targetPosition;
            targetObject.rotation = targetRotation;
        }
    }
    
    /// <summary>
    /// Reinicia el contador de movimientos
    /// </summary>
    [ContextMenu("Reset Shift Count")]
    public void ResetShiftCount()
    {
        currentShiftIndex = 0;
        isShifting = false;
        
        if (targetObject != null && shiftPositions != null && shiftPositions.Length > 0)
        {
            // Volver a la posición inicial (antes del primer shift)
            // Puedes comentar estas líneas si quieres mantener la posición actual
            // targetPosition = targetObject.position;
            // targetRotation = targetObject.rotation;
        }
        
        if (showLogs)
        {
            Debug.Log("<color=cyan>[ProximityShifter] 🔄 Contador reiniciado</color>");
        }
    }
    
    /// <summary>
    /// Fuerza un movimiento manual (para testing)
    /// </summary>
    [ContextMenu("Force Shift")]
    public void ForceShift()
    {
        if (currentShiftIndex < MaxShifts)
        {
            ExecuteShift();
        }
        else
        {
            Debug.LogWarning("<color=orange>[ProximityShifter] ⚠️ Ya se alcanzaron todas las posiciones</color>");
        }
    }
    
    /// <summary>
    /// Establece la posición actual del target object como posición inicial
    /// </summary>
    [ContextMenu("Set Current Position as Start")]
    public void SetCurrentPositionAsStart()
    {
        if (targetObject != null)
        {
            targetPosition = targetObject.position;
            targetRotation = targetObject.rotation;
            Debug.Log($"<color=cyan>[ProximityShifter] 📍 Posición inicial establecida: {targetPosition}</color>");
        }
    }
    
    /// <summary>
    /// Captura la posición actual del target object y la añade al array
    /// (solo funciona en el editor)
    /// </summary>
    [ContextMenu("Capture Current Position")]
    public void CaptureCurrentPosition()
    {
        #if UNITY_EDITOR
        if (targetObject != null)
        {
            Debug.Log($"<color=yellow>[ProximityShifter] 📸 Posición capturada: {targetObject.position} | Rotación: {targetObject.rotation.eulerAngles}</color>");
            Debug.Log("<color=yellow>   Añade manualmente esta posición al array Shift Positions en el Inspector</color>");
        }
        #endif
    }
    
    /// <summary>
    /// Visualización en Scene view
    /// </summary>
    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;
        
        // Dibujar el área del trigger
        Gizmos.color = gizmoColor;
        
        if (col is SphereCollider sphereCol)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawSphere(sphereCol.center, sphereCol.radius);
            
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
            Gizmos.DrawWireSphere(sphereCol.center, sphereCol.radius);
        }
        else if (col is BoxCollider boxCol)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(boxCol.center, boxCol.size);
            
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
            Gizmos.DrawWireCube(boxCol.center, boxCol.size);
        }
        
        #if UNITY_EDITOR
        // Texto informativo
        string info = $"ProximityShifter\n{currentShiftIndex}/{MaxShifts} movimientos";
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, info);
        #endif
    }
    
    /// <summary>
    /// Dibuja líneas en Scene view mostrando la trayectoria del botón
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (shiftPositions == null || shiftPositions.Length == 0) return;
        
        // Dibujar posición inicial (si hay target object)
        if (targetObject != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(targetObject.position, 0.08f);
            
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(targetObject.position + Vector3.up * 0.3f, "START");
            #endif
        }
        
        // Dibujar todas las posiciones objetivo
        for (int i = 0; i < shiftPositions.Length; i++)
        {
            Vector3 pos = shiftPositions[i].position;
            
            // Color diferente según el índice
            if (Application.isPlaying)
            {
                // En play mode, colorear según si ya pasó
                Gizmos.color = i < currentShiftIndex ? Color.gray : (i == currentShiftIndex ? Color.yellow : Color.green);
            }
            else
            {
                // En editor, gradiente de amarillo a verde
                float t = (float)i / Mathf.Max(shiftPositions.Length - 1, 1);
                Gizmos.color = Color.Lerp(Color.yellow, Color.green, t);
            }
            
            // Dibujar esfera en la posición
            Gizmos.DrawWireSphere(pos, 0.1f);
            
            // Línea desde la posición anterior
            if (i > 0)
            {
                Gizmos.DrawLine(shiftPositions[i - 1].position, pos);
            }
            else if (targetObject != null)
            {
                // Línea desde la posición inicial del objeto
                Gizmos.DrawLine(targetObject.position, pos);
            }
            
            #if UNITY_EDITOR
            // Etiqueta con el número
            UnityEditor.Handles.Label(pos + Vector3.up * 0.2f, $"#{i + 1}");
            
            // Mostrar rotación como pequeñas flechas
            Quaternion rot = Quaternion.Euler(shiftPositions[i].rotation);
            Vector3 forward = rot * Vector3.forward * 0.15f;
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(pos, forward);
            #endif
        }
    }
}
