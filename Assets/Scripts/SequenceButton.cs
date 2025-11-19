using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Botón individual para el puzzle de secuencia.
/// Cambia de color según su estado (normal, correcto, incorrecto).
/// Compatible con Unity Events y XR Interaction Toolkit.
/// </summary>
public class SequenceButton : MonoBehaviour
{
    [Header("Visual Settings")]
    [Tooltip("Objeto hijo con el Renderer que cambiará de color (opcional, si está vacío busca automáticamente)")]
    public GameObject visualObject;
    
    [Tooltip("Color inicial del botón (neutral)")]
    public Color normalColor = Color.red;
    
    [Tooltip("Color cuando el botón es presionado correctamente")]
    public Color correctColor = Color.green;
    
    [Tooltip("Color cuando el botón es presionado incorrectamente")]
    public Color incorrectColor = new Color(0.5f, 0f, 0f); // Rojo oscuro
    
    [Tooltip("Nombre de la propiedad del material para el color (_BaseColor para URP, _Color para Built-in)")]
    public string colorPropertyName = "_BaseColor";
    
    [Header("Animation")]
    [Tooltip("Escala al presionar")]
    [Range(0.8f, 1.2f)]
    public float pressScale = 0.9f;
    
    [Tooltip("Duración de la animación de presión")]
    [Range(0.05f, 0.5f)]
    public float animationDuration = 0.1f;
    
    [Header("Optional: Emission")]
    [Tooltip("Activar emisión cuando está en estado correcto")]
    public bool useEmission = true;
    
    [Tooltip("Color de emisión para estado correcto")]
    public Color emissionColor = Color.green;
    
    [Tooltip("Intensidad de emisión")]
    [Range(0f, 5f)]
    public float emissionIntensity = 2f;
    
    [Header("Unity Events")]
    [Tooltip("Evento que se ejecuta cuando se presiona el botón (para XR Interaction Toolkit)")]
    public UnityEvent onButtonPressed;
    
    // Referencias privadas
    private SequencePuzzle puzzleManager;
    private int buttonIndex;
    private Renderer buttonRenderer;
    private Material buttonMaterial;
    private Vector3 originalScale;
    private bool isAnimating = false;
    
    private void Awake()
    {
        // Buscar el renderer automáticamente
        FindRenderer();
        
        // Crear una instancia del material para no afectar otros objetos
        if (buttonRenderer != null)
        {
            buttonMaterial = buttonRenderer.material;
        }
        else
        {
            Debug.LogWarning($"<color=yellow>[SequenceButton] ⚠️ {gameObject.name} no tiene Renderer. Los cambios de color no funcionarán.</color>");
        }
        
        originalScale = transform.localScale;
    }
    
    /// <summary>
    /// Busca el Renderer automáticamente
    /// </summary>
    private void FindRenderer()
    {
        // Si se asignó un objeto visual manualmente, usarlo
        if (visualObject != null)
        {
            buttonRenderer = visualObject.GetComponent<Renderer>();
            if (buttonRenderer != null)
            {
                Debug.Log($"<color=cyan>[SequenceButton] ✅ {gameObject.name}: Renderer encontrado en objeto asignado '{visualObject.name}'</color>");
                return;
            }
        }
        
        // Intentar encontrar en este GameObject
        buttonRenderer = GetComponent<Renderer>();
        if (buttonRenderer != null)
        {
            Debug.Log($"<color=cyan>[SequenceButton] ✅ {gameObject.name}: Renderer encontrado en este GameObject</color>");
            return;
        }
        
        // Buscar en los hijos
        buttonRenderer = GetComponentInChildren<Renderer>();
        if (buttonRenderer != null)
        {
            Debug.Log($"<color=cyan>[SequenceButton] ✅ {gameObject.name}: Renderer encontrado en hijo '{buttonRenderer.gameObject.name}'</color>");
            return;
        }
        
        Debug.LogWarning($"<color=yellow>[SequenceButton] ⚠️ {gameObject.name}: No se encontró ningún Renderer. Asigna un objeto con Renderer en 'Visual Object' o agrega el script al objeto con el mesh.</color>");
    }
    
    private void Start()
    {
        ResetButton();
        
        // Suscribirse automáticamente al evento para que funcione con XR Interaction
        if (onButtonPressed != null)
        {
            onButtonPressed.AddListener(PressButton);
        }
    }
    
    /// <summary>
    /// Asigna el puzzle manager y el índice de este botón
    /// </summary>
    public void SetPuzzleManager(SequencePuzzle manager, int index)
    {
        puzzleManager = manager;
        buttonIndex = index;
    }
    
    /// <summary>
    /// Método para ser llamado cuando el botón es presionado (desde trigger VR, eventos, etc.)
    /// ESTE ES EL MÉTODO QUE DEBES LLAMAR DESDE TUS UNITY EVENTS O XR INTERACTION TOOLKIT
    /// </summary>
    public void PressButton()
    {
        if (puzzleManager != null)
        {
            puzzleManager.OnButtonPressed(buttonIndex);
            
            // Animación de presión
            if (!isAnimating)
            {
                StartCoroutine(PressAnimation());
            }
        }
        else
        {
            Debug.LogError($"<color=red>[SequenceButton] ❌ {gameObject.name} no tiene PuzzleManager asignado!</color>");
        }
    }
    
    /// <summary>
    /// Resetea el botón al estado inicial
    /// </summary>
    public void ResetButton()
    {
        SetColor(normalColor);
        
        if (useEmission && buttonMaterial != null)
        {
            buttonMaterial.DisableKeyword("_EMISSION");
            buttonMaterial.SetColor("_EmissionColor", Color.black);
        }
    }
    
    /// <summary>
    /// Establece el estado visual de "correcto"
    /// </summary>
    public void SetCorrectState()
    {
        SetColor(correctColor);
        
        // Activar emisión si está habilitado
        if (useEmission && buttonMaterial != null)
        {
            buttonMaterial.EnableKeyword("_EMISSION");
            buttonMaterial.SetColor("_EmissionColor", emissionColor * emissionIntensity);
        }
    }
    
    /// <summary>
    /// Establece el estado visual de "incorrecto" temporalmente
    /// </summary>
    public void SetIncorrectState()
    {
        SetColor(incorrectColor);
    }
    
    /// <summary>
    /// Cambia el color del material
    /// </summary>
    private void SetColor(Color color)
    {
        if (buttonMaterial != null)
        {
            buttonMaterial.SetColor(colorPropertyName, color);
        }
    }
    
    /// <summary>
    /// Animación de presión del botón
    /// </summary>
    private System.Collections.IEnumerator PressAnimation()
    {
        isAnimating = true;
        
        // Comprimir
        float elapsed = 0f;
        Vector3 targetScale = originalScale * pressScale;
        
        while (elapsed < animationDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (animationDuration / 2f);
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }
        
        // Volver a normal
        elapsed = 0f;
        while (elapsed < animationDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (animationDuration / 2f);
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }
        
        transform.localScale = originalScale;
        isAnimating = false;
    }
    
    /// <summary>
    /// Test desde el inspector
    /// </summary>
    [ContextMenu("Press Button (Test)")]
    public void TestPress()
    {
        PressButton();
    }
    
    [ContextMenu("Set Correct State (Visual Test)")]
    public void TestCorrectState()
    {
        SetCorrectState();
    }
    
    [ContextMenu("Set Incorrect State (Visual Test)")]
    public void TestIncorrectState()
    {
        SetIncorrectState();
    }
    
    [ContextMenu("Reset (Visual Test)")]
    public void TestReset()
    {
        ResetButton();
    }
    
    [ContextMenu("Find Renderer Now")]
    public void TestFindRenderer()
    {
        FindRenderer();
    }
}
