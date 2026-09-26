using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class MachineProgressVisibility : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float visibleRadius = 3f;

    private Canvas progressCanvas;
    private Camera mainCamera;

    private void Awake()
    {
        progressCanvas = GetComponent<Canvas>();
        progressCanvas.enabled = false;
    }

    private void Update()
    {
        if (player == null)
            return;

        Vector3 difference = player.position - transform.position;
        difference.y = 0f;
        progressCanvas.enabled = difference.sqrMagnitude <= visibleRadius * visibleRadius;
    }

    private void LateUpdate()
    {
        if (!progressCanvas.enabled)
            return;

        if (mainCamera == null)
            mainCamera = Camera.main;
        if (mainCamera != null)
            transform.rotation = mainCamera.transform.rotation;
    }
}
