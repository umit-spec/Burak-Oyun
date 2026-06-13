using UnityEngine;
using UnityEngine.EventSystems;

namespace BurakOyun.Core
{
    /// Android'de StandaloneInputModule yerine InputSystemUIInputModule olmasını garantiler.
    /// StandaloneInputModule yeni Input System ile touch event'lerini UI'ya iletmez.
    [RequireComponent(typeof(EventSystem))]
    [DefaultExecutionOrder(-500)] // EventSystem'den önce çalış
    public class EventSystemFixer : MonoBehaviour
    {
        private void Awake()
        {
#if ENABLE_INPUT_SYSTEM
            // Eski modülü kaldır
            var old = GetComponent<StandaloneInputModule>();
            if (old != null) DestroyImmediate(old);

            // Yeni modül yoksa ekle
            if (GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null)
                gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#endif
        }
    }
}
