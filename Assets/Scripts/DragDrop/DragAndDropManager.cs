using UnityEngine;

public class DragAndDropManager : MonoBehaviour
{
    [SerializeField] private LayerMask _dragLayerMask;

    private GameObject _previousDraggedObject;
    private GameObject _currentDraggedObject;
    private IDragObject _currentDragObjectInterface;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 100, _dragLayerMask);
            if (hit.collider != null)
            {
                _currentDraggedObject = hit.collider.gameObject;
                _currentDragObjectInterface = _currentDraggedObject.GetComponent<IDragObject>();
                _currentDragObjectInterface?.DragStart();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (_currentDraggedObject != null)
                _currentDragObjectInterface?.DragEnd();
            _currentDraggedObject = null;
            _currentDragObjectInterface = null;
        }

        if (_currentDraggedObject != null)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0; // Ensure the z-coordinate is zero for 2D
            _currentDraggedObject.transform.position = mousePosition;
            _previousDraggedObject = _currentDraggedObject;
        }
        else
        {
            _previousDraggedObject = null;
        }
    }

}