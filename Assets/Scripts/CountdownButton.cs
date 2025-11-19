using UnityEngine;
using TMPro;

/// <summary>
/// Botón que cuenta hacia abajo y activa/desactiva objetos al llegar a cero.
/// Útil para puzzles que requieren presionar un botón X veces.
/// </summary>
public class CountdownButton : MonoBehaviour
{
    [Header("Counter Settings")]
    [Tooltip("Número inicial del contador (cuántas veces hay que presionar)")]
    [Min(1)]
    public int initialCount = 3;
    
    [Tooltip("Referencia al TextMeshPro que muestra el número")]
    public TextMeshPro counterText;
    
    [Header("Target Actions")]
    [Tooltip("Qué hacer cuando llegue a cero")]
    public ActionType actionOnZero = ActionType.Activate;
    
    [Tooltip("Objeto(s) a activar/desactivar cuando llegue a cero")]
    public GameObject[] targetObjects;
    
    [Header("Optional: Multiple Actions")]
    [Tooltip("Activar objetos adicionales (además de la acción principal)")]
    public GameObject[] objectsToActivate;
    
    [Tooltip("Desactivar objetos adicionales (además de la acción principal)")]
    public GameObject[] objectsToDeactivate;
    
    [Header("Button Behavior")]
    [Tooltip("Desactivar el botón después de llegar a cero")]
    public bool disableButtonAtZero = true;
    
    [Tooltip("Permitir que el contador baje de cero (números negativos)")]
    public bool allowNegative = false;
    
    [Header("Visual Feedback")]
    [Tooltip("Color del texto cuando el contador es mayor a cero")]
    public Color activeColor = Color.white;
    
    [Tooltip("Color del texto cuando el contador llega a cero")]
    public Color zeroColor = Color.green;
    
    [Tooltip("Escala del texto al presionar (animación)")]
    [Range(0.5f, 2f)]
    public float pressScaleMultiplier = 1.2f;
    
    [Tooltip("Duración de la animación de presión")]
    [Range(0.05f, 0.5f)]
    public float pressAnimationDuration = 0.1f;
    
    [Header("Audio (Optional)")]
    [Tooltip("Sonido al presionar el botón")]
    public AudioClip pressSound;
    
    [Tooltip("Sonido al llegar a cero")]
    public AudioClip zeroSound;
    
    [Tooltip("AudioSource para reproducir sonidos (si es null, usa GetComponent)")]
    public AudioSource audioSource;
    
    [Header("Debug")]
    [Tooltip("Mostrar logs de debug")]
    public bool showLogs = true;
    
    // Estado privado
    private int currentCount;
    private bool hasReachedZero = false;
    private Vector3 originalTextScale;
    private bool isAnimating = false;
    
    public enum ActionType
    {
        Activate,
        Deactivate,
        Toggle
    }
    
    private void Awake()
    {
        // Intentar encontrar componentes si no están asignados
        if (counterText == null)
        {
            counterText = GetComponentInChildren<TextMeshPro>();
        }
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        
        if (counterText != null)
        {
            originalTextScale = counterText.transform.localScale;
        }
    }
    
    private void Start()
    {
        // Inicializar el contador
        ResetCounter();
    }
    
    /// <summary>
    /// Resetea el contador a su valor inicial
    /// </summary>
    public void ResetCounter()
    {
        currentCount = initialCount;
        hasReachedZero = false;
        UpdateDisplay();
        
        if (showLogs)
        {
            Debug.Log($"<color=cyan>[CountdownButton] ?? Contador reseteado a {initialCount}</color>");
        }
    }
    
    /// <summary>
    /// Método principal: presionar el botón (llamar desde eventos de Unity, VR trigger, etc.)
    /// </summary>
    public void PressButton()
    {
        // Si ya llegó a cero y está deshabilitado, no hacer nada
        if (hasReachedZero && disableButtonAtZero)
        {
            if (showLogs)
            {
                Debug.Log("<color=yellow>[CountdownButton] ?? Botón deshabilitado (ya llegó a cero)</color>");
            }
            return;
        }
        
        // Restar 1 al contador
        currentCount--;
        
        if (showLogs)
        {
            Debug.Log($"<color=lime>[CountdownButton] ?? Botón presionado! Contador: {currentCount}</color>");
        }
        
        // No permitir valores negativos si está deshabilitado
        if (!allowNegative && currentCount < 0)
        {
            currentCount = 0;
        }
        
        // Actualizar visualización
        UpdateDisplay();
        
        // Reproducir sonido de presión
        PlaySound(pressSound);
        
        // Animación visual
        if (counterText != null && !isAnimating)
        {
            StartCoroutine(PressAnimation());
        }
        
        // Verificar si llegó a cero
        if (currentCount == 0 && !hasReachedZero)
        {
            OnReachZero();
        }
    }
    
    /// <summary>
    /// Se llama cuando el contador llega a cero
    /// </summary>
    private void OnReachZero()
    {
        hasReachedZero = true;
        
        if (showLogs)
        {
            Debug.Log($"<color=green>[CountdownButton] ? ¡Contador llegó a CERO! Ejecutando acción: {actionOnZero}</color>");
        }
        
        // Reproducir sonido especial de cero
        PlaySound(zeroSound);
        
        // Ejecutar acción principal en los objetos target
        ExecuteMainAction();
        
        // Ejecutar acciones adicionales
        ExecuteAdditionalActions();
    }
    
    /// <summary>
    /// Ejecuta la acción principal en los objetos target
    /// </summary>
    private void ExecuteMainAction()
    {
        if (targetObjects == null || targetObjects.Length == 0)
        {
            if (showLogs)
            {
                Debug.LogWarning("<color=orange>[CountdownButton] ?? No hay objetos target asignados</color>");
            }
            return;
        }
        
        foreach (GameObject obj in targetObjects)
        {
            if (obj == null) continue;
            
            switch (actionOnZero)
            {
                case ActionType.Activate:
                    obj.SetActive(true);
                    if (showLogs)
                    {
                        Debug.Log($"<color=lime>[CountdownButton] ? Activado: {obj.name}</color>");
                    }
                    break;
                    
                case ActionType.Deactivate:
                    obj.SetActive(false);
                    if (showLogs)
                    {
                        Debug.Log($"<color=yellow>[CountdownButton] ? Desactivado: {obj.name}</color>");
                    }
                    break;
                    
                case ActionType.Toggle:
                    obj.SetActive(!obj.activeSelf);
                    if (showLogs)
                    {
                        Debug.Log($"<color=cyan>[CountdownButton] ?? Toggle: {obj.name} -> {obj.activeSelf}</color>");
                    }
                    break;
            }
        }
    }
    
    /// <summary>
    /// Ejecuta las acciones adicionales opcionales
    /// </summary>
    private void ExecuteAdditionalActions()
    {
        // Activar objetos adicionales
        if (objectsToActivate != null)
        {
            foreach (GameObject obj in objectsToActivate)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                    if (showLogs)
                    {
                        Debug.Log($"<color=lime>[CountdownButton] ? Activado adicional: {obj.name}</color>");
                    }
                }
            }
        }
        
        // Desactivar objetos adicionales
        if (objectsToDeactivate != null)
        {
            foreach (GameObject obj in objectsToDeactivate)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                    if (showLogs)
                    {
                        Debug.Log($"<color=yellow>[CountdownButton] ? Desactivado adicional: {obj.name}</color>");
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Actualiza el texto del contador
    /// </summary>
    private void UpdateDisplay()
    {
        if (counterText == null) return;
        
        // Actualizar el texto
        counterText.text = currentCount.ToString();
        
        // Cambiar color según el estado
        counterText.color = currentCount <= 0 ? zeroColor : activeColor;
    }
    
    /// <summary>
    /// Animación de presión del botón
    /// </summary>
    private System.Collections.IEnumerator PressAnimation()
    {
        if (counterText == null) yield break;
        
        isAnimating = true;
        
        // Agrandar
        float elapsed = 0f;
        Vector3 targetScale = originalTextScale * pressScaleMultiplier;
        
        while (elapsed < pressAnimationDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (pressAnimationDuration / 2f);
            counterText.transform.localScale = Vector3.Lerp(originalTextScale, targetScale, t);
            yield return null;
        }
        
        // Volver a normal
        elapsed = 0f;
        while (elapsed < pressAnimationDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (pressAnimationDuration / 2f);
            counterText.transform.localScale = Vector3.Lerp(targetScale, originalTextScale, t);
            yield return null;
        }
        
        counterText.transform.localScale = originalTextScale;
        isAnimating = false;
    }
    
    /// <summary>
    /// Reproduce un sonido si está disponible
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    /// <summary>
    /// Métodos públicos para control externo
    /// </summary>
    [ContextMenu("Press Button (Test)")]
    public void TestPress()
    {
        PressButton();
    }
    
    [ContextMenu("Reset Counter")]
    public void TestReset()
    {
        ResetCounter();
    }
    
    [ContextMenu("Force Reach Zero")]
    public void ForceReachZero()
    {
        currentCount = 0;
        UpdateDisplay();
        OnReachZero();
    }
    
    /// <summary>
    /// Propiedades públicas para leer el estado
    /// </summary>
    public int CurrentCount => currentCount;
    public bool HasReachedZero => hasReachedZero;
    public int InitialCount => initialCount;
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        // Actualizar el display en el editor cuando cambian valores
        if (Application.isPlaying && counterText != null)
        {
            UpdateDisplay();
        }
    }
#endif
}
