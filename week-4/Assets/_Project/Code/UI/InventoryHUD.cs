using _Project.Code.Gameplay.Items;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Code.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class InventoryHUD : MonoBehaviour
    {
        [field: SerializeField] public float DisplaySeconds { get; private set; } = 2f;
        [field: SerializeField] public Font Font { get; private set; }

        private VisualElement _notice;
        private Label _label;
        private float _hideAt = -1f;

        private void Awake()
        {
            UIDocument doc = GetComponent<UIDocument>();
            VisualElement root = doc.rootVisualElement;
            _notice = root.Q<VisualElement>("pickup-notice");
            _label = root.Q<Label>("pickup-label");

            if (_label != null && Font != null)
            {
                _label.style.unityFont = new StyleFont(Font);
            }
        }

        private void OnEnable()
        {
            Inventory.Instance.OnItemAdded += HandleItemAdded;
        }

        private void OnDisable()
        {
            if (Inventory.Instance != null)
            {
                Inventory.Instance.OnItemAdded -= HandleItemAdded;
            }
        }

        private void Update()
        {
            if (_hideAt > 0f && Time.time >= _hideAt)
            {
                _notice?.RemoveFromClassList("is-visible");
                _hideAt = -1f;
            }
        }

        private void HandleItemAdded(Item item)
        {
            if (_label == null || _notice == null || item == null)
            {
                return;
            }
            _label.text = $"{item.DisplayName} picked up";
            _notice.AddToClassList("is-visible");
            _hideAt = Time.time + DisplaySeconds;
        }
    }
}
