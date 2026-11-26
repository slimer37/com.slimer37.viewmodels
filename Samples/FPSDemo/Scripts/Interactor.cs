using UnityEngine;
using UnityEngine.InputSystem;

namespace Slimer37.Viewmodels.Samples
{
    public class Interactor : MonoBehaviour
    {
        [SerializeField] private InputAction _interactionAction;
        [SerializeField] private Viewmodel _viewmodel;

        private readonly Hand _hand = new();

        private void Awake()
        {
            _interactionAction.performed += OnInteract;
            _hand.OnPickup += OnPickupItem;
            _hand.OnRemove += OnRemoveItem;
        }

        private void OnRemoveItem(Item item)
        {
            _viewmodel.RemoveObject(item);
        }

        private void OnPickupItem(Item item)
        {
            _viewmodel.AddObject(item);
        }

        private void OnInteract(InputAction.CallbackContext ctx)
        {
            Ray ray = Camera.main.ViewportPointToRay(Vector2.one * 0.5f);
            if (Physics.Raycast(ray, out var hit))
            {
                hit.collider.GetComponent<Item>()?.Pickup(_hand);
            }
        }

        private void OnEnable()
        {
            _interactionAction.Enable();
        }

        private void OnDisable()
        {
            _interactionAction.Disable();
        }

        private void OnDestroy()
        {
            _interactionAction.Dispose();
        }
    }
}