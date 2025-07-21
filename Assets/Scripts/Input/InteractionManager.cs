using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    [SerializeField] private LayerMask _interactionLayers;
    private IInteractable _currentInteractable;

    void Update()
    {
        InteractWithClick();
    }
    void InteractWithClick()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button
        {
            RaycastHit2D hit2D = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, _interactionLayers);

            if (hit2D.collider != null)
            {
                if (hit2D.collider.TryGetComponent<IInteractable>(out IInteractable interactable))
                {
                    _currentInteractable = interactable;
                    _currentInteractable.InteractDown();
                }
            }
        }
        if (Input.GetMouseButtonUp(0)) // Left mouse button release
        {
            if (_currentInteractable != null)
            {
                bool passed = _currentInteractable.InteractUP();
                if (!passed) _currentInteractable.CanceledInteraction();
                _currentInteractable = null; // Reset current interactable
            }
        }
    }
}