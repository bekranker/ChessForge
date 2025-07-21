using UnityEngine;

public class CardTiltEffect2D : MonoBehaviour
{
    public float tiltAmount = 15f;     // Dönüş açısı
    public float smoothSpeed = 5f;     // Yumuşatma hızı
    public float hoverDistance = 1.5f; // Mouse yakınlık mesafesi

    public Quaternion originalRotation;
    private Camera mainCamera;

    void Start()
    {
        originalRotation = transform.rotation;
        mainCamera = Camera.main;
    }

    void Update()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = transform.position.z; // Aynı derinlikte karşılaştır

        float distance = Vector2.Distance(mouseWorldPos, transform.position);

        if (distance < hoverDistance)
        {
            Vector3 direction = mouseWorldPos - transform.position;

            // Normalize ederek 0–1 arası değerle çalış
            float tiltX = -direction.y * tiltAmount;
            float tiltY = direction.x * tiltAmount;

            Quaternion targetRotation = Quaternion.Euler(tiltX, tiltY, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * smoothSpeed);
        }
        else
        {
            // Mouse uzakta: orijinal rotasyona dön
            transform.rotation = Quaternion.Lerp(transform.rotation, originalRotation, Time.deltaTime * smoothSpeed);
        }
    }
}
