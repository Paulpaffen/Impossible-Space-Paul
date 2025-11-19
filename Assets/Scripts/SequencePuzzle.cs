using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Puzzle de secuencia oculta: los botones deben presionarse en un orden específico.
/// Si el usuario se equivoca, el puzzle se resetea automáticamente.
/// TOTALMENTE PERSONALIZABLE: Define la cantidad de botones y el orden.
/// </summary>
public class SequencePuzzle : MonoBehaviour
{
    [Header("Puzzle Configuration")]
    [Tooltip("Referencias a TODOS los botones del puzzle (en el orden que quieras)")]
    public SequenceButton[] buttons;
    
    [Tooltip("Secuencia correcta usando los NOMBRES/ÍNDICES de los botones arriba.\nEjemplo: [0, 2, 1] = Primero el botón 0, luego el 2, luego el 1")]
    public int[] correctSequence = new int[] { 0, 2, 1 };
    
    [Header("Validation")]
    [Tooltip("Validar automáticamente la secuencia al iniciar (detecta errores de configuración)")]
    public bool validateOnStart = true;
    
    [Header("Completion Actions")]
    [Tooltip("Objetos a activar cuando se complete la secuencia")]
    public GameObject[] objectsToActivate;
    
    [Tooltip("Objetos a desactivar cuando se complete la secuencia")]
    public GameObject[] objectsToDeactivate;
    
    [Tooltip("Evento que se ejecuta al completar el puzzle")]
    public UnityEvent onPuzzleComplete;
    
    [Header("Timing")]
    [Tooltip("Delay antes de resetear cuando el usuario se equivoca (segundos)")]
    [Range(0f, 2f)]
    public float resetDelay = 0.5f;
    
    [Tooltip("Delay antes de ejecutar acciones al completar (para que vea el éxito)")]
    [Range(0f, 3f)]
    public float completionDelay = 1f;
    
    [Header("Audio (Optional)")]
    [Tooltip("Sonido cuando se presiona un botón correcto")]
    public AudioClip correctSound;
    
    [Tooltip("Sonido cuando se presiona un botón incorrecto")]
    public AudioClip incorrectSound;
    
    [Tooltip("Sonido cuando se completa el puzzle")]
    public AudioClip completionSound;
    
    [Tooltip("AudioSource para reproducir sonidos")]
    public AudioSource audioSource;
    
    [Header("Debug")]
    [Tooltip("Mostrar logs de debug")]
    public bool showLogs = true;
    
    [Tooltip("Mostrar la secuencia correcta en los logs (para testing)")]
    public bool showSequenceInLogs = true;
    
    // Estado privado
    private int currentStep = 0;
    private bool isComplete = false;
    private bool isResetting = false;
    private Dictionary<SequenceButton, int> buttonIndexMap;
    
    private void Awake()
    {
        // Buscar AudioSource si no está asignado
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Crear mapa de botones a índices para búsqueda rápida
        buttonIndexMap = new Dictionary<SequenceButton, int>();
        
        // Registrar este puzzle en cada botón
        if (buttons != null)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null)
                {
                    buttons[i].SetPuzzleManager(this, i);
                    buttonIndexMap[buttons[i]] = i;
                }
            }
        }
    }
    
    private void Start()
    {
        // Validar configuración
        if (validateOnStart)
        {
            ValidateConfiguration();
        }
        
        ResetPuzzle();
        
        if (showLogs && showSequenceInLogs)
        {
            ShowSequenceInConsole();
        }
    }
    
    /// <summary>
    /// Valida que la configuración sea correcta
    /// </summary>
    private void ValidateConfiguration()
    {
        bool hasErrors = false;
        
        // Verificar que hay botones
        if (buttons == null || buttons.Length == 0)
        {
            Debug.LogError("<color=red>[SequencePuzzle] ❌ ERROR: No hay botones asignados!</color>");
            hasErrors = true;
        }
        
        // Verificar que hay secuencia
        if (correctSequence == null || correctSequence.Length == 0)
        {
            Debug.LogError("<color=red>[SequencePuzzle] ❌ ERROR: La secuencia está vacía!</color>");
            hasErrors = true;
        }
        
        // Verificar que los índices de la secuencia son válidos
        if (buttons != null && correctSequence != null)
        {
            foreach (int index in correctSequence)
            {
                if (index < 0 || index >= buttons.Length)
                {
                    Debug.LogError($"<color=red>[SequencePuzzle] ❌ ERROR: Índice {index} en la secuencia está fuera de rango! (Tienes {buttons.Length} botones: índices 0-{buttons.Length - 1})</color>");
                    hasErrors = true;
                }
            }
        }
        
        // Verificar que no hay botones null
        if (buttons != null)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] == null)
                {
                    Debug.LogWarning($"<color=yellow>[SequencePuzzle] ⚠️ ADVERTENCIA: El botón en el índice {i} es NULL!</color>");
                }
            }
        }
        
        if (!hasErrors && showLogs)
        {
            Debug.Log($"<color=lime>[SequencePuzzle] ✅ Configuración válida: {buttons.Length} botones, secuencia de {correctSequence.Length} pasos</color>");
        }
    }
    
    /// <summary>
    /// Método llamado cuando un botón es presionado
    /// </summary>
    public void OnButtonPressed(int buttonIndex)
    {
        // No procesar si ya está completo o reseteando
        if (isComplete || isResetting)
        {
            return;
        }
        
        // Verificar si es el botón correcto en la secuencia
        int expectedButton = correctSequence[currentStep];
        
        if (buttonIndex == expectedButton)
        {
            // ¡CORRECTO!
            HandleCorrectPress(buttonIndex);
        }
        else
        {
            // ¡INCORRECTO!
            HandleIncorrectPress(buttonIndex, expectedButton);
        }
    }
    
    /// <summary>
    /// Maneja cuando se presiona el botón correcto
    /// </summary>
    private void HandleCorrectPress(int buttonIndex)
    {
        if (showLogs)
        {
            Debug.Log($"<color=lime>[SequencePuzzle] ✅ ¡Correcto! Botón {buttonIndex} (paso {currentStep + 1}/{correctSequence.Length})</color>");
        }
        
        // Marcar el botón como correcto
        if (buttons[buttonIndex] != null)
        {
            buttons[buttonIndex].SetCorrectState();
        }
        
        // Reproducir sonido de éxito
        PlaySound(correctSound);
        
        // Avanzar al siguiente paso
        currentStep++;
        
        // Verificar si completó toda la secuencia
        if (currentStep >= correctSequence.Length)
        {
            StartCoroutine(CompletePuzzle());
        }
    }
    
    /// <summary>
    /// Maneja cuando se presiona un botón incorrecto
    /// </summary>
    private void HandleIncorrectPress(int pressedButton, int expectedButton)
    {
        if (showLogs)
        {
            Debug.LogWarning($"<color=red>[SequencePuzzle] ❌ Incorrecto! Presionaste el botón {pressedButton}, esperaba el {expectedButton} (paso {currentStep + 1}). Reseteando...</color>");
        }
        
        // Marcar el botón como incorrecto temporalmente
        if (buttons[pressedButton] != null)
        {
            buttons[pressedButton].SetIncorrectState();
        }
        
        // Reproducir sonido de error
        PlaySound(incorrectSound);
        
        // Resetear después de un delay
        StartCoroutine(ResetAfterDelay());
    }
    
    /// <summary>
    /// Resetea el puzzle al estado inicial
    /// </summary>
    public void ResetPuzzle()
    {
        if (showLogs)
        {
            Debug.Log("<color=yellow>[SequencePuzzle] 🔄 Reseteando puzzle...</color>");
        }
        
        currentStep = 0;
        isComplete = false;
        isResetting = false;
        
        // Resetear todos los botones
        if (buttons != null)
        {
            foreach (var button in buttons)
            {
                if (button != null)
                {
                    button.ResetButton();
                }
            }
        }
    }
    
    /// <summary>
    /// Coroutine para resetear después de un delay
    /// </summary>
    private IEnumerator ResetAfterDelay()
    {
        isResetting = true;
        yield return new WaitForSeconds(resetDelay);
        ResetPuzzle();
    }
    
    /// <summary>
    /// Coroutine para completar el puzzle
    /// </summary>
    private IEnumerator CompletePuzzle()
    {
        isComplete = true;
        
        if (showLogs)
        {
            Debug.Log("<color=green>[SequencePuzzle] 🎉 ¡PUZZLE COMPLETADO!</color>");
        }
        
        // Reproducir sonido de completado
        PlaySound(completionSound);
        
        // Esperar un momento para que el jugador vea el éxito
        yield return new WaitForSeconds(completionDelay);
        
        // Activar objetos
        if (objectsToActivate != null)
        {
            foreach (var obj in objectsToActivate)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                    if (showLogs)
                    {
                        Debug.Log($"<color=lime>[SequencePuzzle] ✅ Activado: {obj.name}</color>");
                    }
                }
            }
        }
        
        // Desactivar objetos
        if (objectsToDeactivate != null)
        {
            foreach (var obj in objectsToDeactivate)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                    if (showLogs)
                    {
                        Debug.Log($"<color=yellow>[SequencePuzzle] ❌ Desactivado: {obj.name}</color>");
                    }
                }
            }
        }
        
        // Invocar evento
        onPuzzleComplete?.Invoke();
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
    /// Muestra la secuencia correcta en la consola
    /// </summary>
    private void ShowSequenceInConsole()
    {
        if (correctSequence == null || correctSequence.Length == 0)
        {
            Debug.LogWarning("<color=yellow>[SequencePuzzle] ⚠️ No hay secuencia configurada</color>");
            return;
        }
        
        string sequenceStr = "Secuencia correcta: ";
        for (int i = 0; i < correctSequence.Length; i++)
        {
            int buttonIndex = correctSequence[i];
            string buttonName = (buttons != null && buttonIndex < buttons.Length && buttons[buttonIndex] != null) 
                ? buttons[buttonIndex].gameObject.name 
                : $"Botón {buttonIndex}";
            
            sequenceStr += buttonName;
            if (i < correctSequence.Length - 1)
            {
                sequenceStr += " → ";
            }
        }
        
        Debug.Log($"<color=cyan>[SequencePuzzle] 🎯 {sequenceStr}</color>");
    }
    
    /// <summary>
    /// Métodos de testing en el inspector
    /// </summary>
    [ContextMenu("Reset Puzzle")]
    public void TestReset()
    {
        ResetPuzzle();
    }
    
    [ContextMenu("Show Correct Sequence")]
    public void ShowSequence()
    {
        ShowSequenceInConsole();
    }
    
    [ContextMenu("Complete Puzzle (Force)")]
    public void ForceComplete()
    {
        StartCoroutine(CompletePuzzle());
    }
    
    [ContextMenu("Validate Configuration")]
    public void TestValidation()
    {
        ValidateConfiguration();
    }
    
    /// <summary>
    /// Propiedades públicas para leer el estado
    /// </summary>
    public int CurrentStep => currentStep;
    public bool IsComplete => isComplete;
    public int TotalSteps => correctSequence != null ? correctSequence.Length : 0;
    public int TotalButtons => buttons != null ? buttons.Length : 0;
}
