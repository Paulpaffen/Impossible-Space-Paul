using UnityEngine;

/// <summary>
/// Componente helper que activa el recentrado cuando TransitionTrigger lo necesita.
/// Agrega este componente al mismo GameObject que TransitionTrigger.
/// NOTA: Actualmente DESACTIVADO - el recentrado no se ejecuta.
/// </summary>
public class RecenterHelper : MonoBehaviour
{
    [Tooltip("Habilitar/deshabilitar el recentrado")]
    public bool enableRecenter = false;
    
    public void TriggerRecenter()
    {
        if (!enableRecenter)
        {
            Debug.Log("<color=grey><b>[Recenter]</b> ⏭️ Recentrado desactivado (enableRecenter = false)</color>");
            return;
        }
        
        // Buscar el gain controller
        TranslationGainSimpleX gainController = FindFirstObjectByType<TranslationGainSimpleX>();
        
        if (gainController != null)
        {
            gainController.RecenterPlayerX();
            Debug.Log("<color=magenta><b>[Recenter]</b> 🔄 Recentrado ejecutado exitosamente</color>");
        }
        else
        {
            Debug.LogWarning("<color=orange><b>[Recenter]</b> ⚠️ No se encontró TranslationGainSimpleX</color>");
        }
    }
}
