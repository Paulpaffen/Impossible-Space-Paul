using UnityEngine;

/// <summary>
/// Script de diagnóstico para verificar el setup del Translation Gain
/// </summary>
public class GainDiagnostic : MonoBehaviour
{
    private TranslationGainSimpleX gainController;
    private Transform centerEyeAnchor;
    private Transform virtualWorld;
    private Vector3 lastPlayerPos;
    private float checkInterval = 0.5f;
    private float nextCheckTime = 0f;
    
    private void Start()
    {
        Debug.Log("<color=yellow>════════════════════════════════════════════</color>");
        Debug.Log("<color=yellow><b>[DIAGNOSTIC]</b> 🔍 Iniciando diagnóstico del sistema...</color>");
        Debug.Log("<color=yellow>════════════════════════════════════════════</color>");
        
        // Buscar componentes
        gainController = FindFirstObjectByType<TranslationGainSimpleX>();
        
        // Verificar GainController
        if (gainController == null)
        {
            Debug.LogError("<color=red><b>[DIAGNOSTIC]</b> ❌ NO SE ENCONTRÓ TranslationGainSimpleX en la escena</color>");
            Debug.LogError("<color=red>   → Solución: Verifica que el GameObject 'GainController' tenga el componente</color>");
        }
        else
        {
            Debug.Log("<color=lime><b>[DIAGNOSTIC]</b> ✅ TranslationGainSimpleX encontrado</color>");
            
            // Verificar si está habilitado
            if (!gainController.enabled)
            {
                Debug.LogError("<color=red><b>[DIAGNOSTIC]</b> ❌ TranslationGainSimpleX está DESHABILITADO</color>");
            }
            else
            {
                Debug.Log("<color=lime><b>[DIAGNOSTIC]</b> ✅ TranslationGainSimpleX está habilitado</color>");
            }
            
            // Verificar referencias
            if (gainController.ovrCameraRig == null)
            {
                Debug.LogError("<color=red><b>[DIAGNOSTIC]</b> ❌ ovrCameraRig NO está asignado</color>");
                Debug.LogError("<color=red>   → Solución: Asigna el CameraRig en el Inspector</color>");
            }
            else
            {
                Debug.Log($"<color=lime><b>[DIAGNOSTIC]</b> ✅ ovrCameraRig asignado: {gainController.ovrCameraRig.name}</color>");
                
                // Buscar el CenterEyeAnchor
                centerEyeAnchor = FindCenterEyeAnchor(gainController.ovrCameraRig);
                if (centerEyeAnchor != null)
                {
                    Debug.Log($"<color=lime><b>[DIAGNOSTIC]</b> ✅ CenterEyeAnchor encontrado: {centerEyeAnchor.name}</color>");
                    Debug.Log($"<color=cyan><b>[DIAGNOSTIC]</b> ℹ️ Posición inicial CenterEyeAnchor: X={centerEyeAnchor.position.x:F2}m, Y={centerEyeAnchor.position.y:F2}m, Z={centerEyeAnchor.position.z:F2}m</color>");
                }
                else
                {
                    Debug.LogError("<color=red><b>[DIAGNOSTIC]</b> ❌ NO se encontró CenterEyeAnchor en el CameraRig</color>");
                    Debug.LogError("<color=red>   → Verifica que tu CameraRig tenga un hijo llamado 'CenterEyeAnchor' o una cámara</color>");
                }
            }
            
            if (gainController.virtualWorld == null)
            {
                Debug.LogError("<color=red><b>[DIAGNOSTIC]</b> ❌ virtualWorld NO está asignado</color>");
                Debug.LogError("<color=red>   → Solución: Asigna el GameObject 'VirtualWorld' en el Inspector</color>");
            }
            else
            {
                Debug.Log($"<color=lime><b>[DIAGNOSTIC]</b> ✅ virtualWorld asignado: {gainController.virtualWorld.name}</color>");
                virtualWorld = gainController.virtualWorld;
            }
            
            // Verificar gain
            Debug.Log($"<color=cyan><b>[DIAGNOSTIC]</b> ℹ️ Translation Gain configurado: {gainController.translationGainX:F2}x</color>");
        }
        
        // Buscar VirtualWorld
        GameObject vw = GameObject.Find("VirtualWorld");
        if (vw == null)
        {
            Debug.LogError("<color=red><b>[DIAGNOSTIC]</b> ❌ NO se encontró GameObject 'VirtualWorld' en la escena</color>");
            Debug.LogError("<color=red>   → Solución: Crea un GameObject vacío llamado 'VirtualWorld' y pon tus habitaciones como hijos</color>");
        }
        else
        {
            Debug.Log($"<color=lime><b>[DIAGNOSTIC]</b> ✅ VirtualWorld encontrado: {vw.name}</color>");
            Debug.Log($"<color=cyan><b>[DIAGNOSTIC]</b> ℹ️ VirtualWorld tiene {vw.transform.childCount} hijos</color>");
            
            // Listar hijos
            for (int i = 0; i < vw.transform.childCount; i++)
            {
                Debug.Log($"<color=cyan>   └─ Hijo {i + 1}: {vw.transform.GetChild(i).name}</color>");
            }
        }
        
        Debug.Log("<color=yellow>════════════════════════════════════════════</color>");
        Debug.Log("<color=lime><b>[DIAGNOSTIC]</b> ✅ Diagnóstico completado. Observa los mensajes arriba.</color>");
        Debug.Log("<color=cyan><b>[DIAGNOSTIC]</b> 💡 Ahora camina en VR y observa si aparecen logs de movimiento</color>");
        Debug.Log("<color=yellow>════════════════════════════════════════════</color>");
        
        if (centerEyeAnchor != null)
        {
            lastPlayerPos = centerEyeAnchor.position;
        }
    }
    
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
            
            // Buscar recursivamente
            found = FindInChildren(rig, name);
            if (found != null)
            {
                return found;
            }
        }
        
        // Buscar la primera cámara
        Camera cam = rig.GetComponentInChildren<Camera>();
        if (cam != null)
        {
            return cam.transform;
        }
        
        return null;
    }
    
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
    
    private void Update()
    {
        // Chequeo periódico de movimiento
        if (Time.time >= nextCheckTime && centerEyeAnchor != null)
        {
            nextCheckTime = Time.time + checkInterval;
            
            Vector3 currentPos = centerEyeAnchor.position;
            Vector3 movement = currentPos - lastPlayerPos;
            
            if (movement.magnitude > 0.001f)
            {
                Debug.Log($"<color=cyan><b>[DIAGNOSTIC]</b> 📊 Movimiento detectado en CenterEyeAnchor:</color>");
                Debug.Log($"<color=cyan>   X={movement.x:F3}m, Y={movement.y:F3}m, Z={movement.z:F3}m (Total: {movement.magnitude:F3}m)</color>");
                Debug.Log($"<color=cyan>   Posición actual: X={currentPos.x:F2}m, Y={currentPos.y:F2}m, Z={currentPos.z:F2}m</color>");
                
                if (virtualWorld != null)
                {
                    Debug.Log($"<color=cyan>   Posición VirtualWorld: X={virtualWorld.position.x:F3}m</color>");
                }
            }
            
            lastPlayerPos = currentPos;
        }
    }
}
