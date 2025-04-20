using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PenController : MonoBehaviour
{
    public ActionBasedController controller; // Cambiado a Action-based Controller
    public Transform penTip;
    public GameObject drawingSurface;
    public Vector3 offset;

    private LineRenderer lineRenderer;
    private bool isDrawing = false;
    private bool canDraw = false;
    private Vector3 previousPosition;

    void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        previousPosition = penTip.position;
    }

    void Update()
    {
        Vector3 penPosition = controller.transform.position + offset;
        penTip.position = penPosition;

        // Leer el valor del botón de disparo (trigger) usando el sistema Action-based
        if (controller.activateAction.action.ReadValue<float>() > 0.5f && canDraw)
        {
            if (!isDrawing)
            {
                StartDrawing();
            }

            Vector3 smoothPosition = Vector3.Lerp(previousPosition, penTip.position, 0.5f);
            lineRenderer.positionCount++;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, smoothPosition);
            previousPosition = smoothPosition;
        }
        else
        {
            if (isDrawing)
            {
                EndDrawing();
            }
        }
    }

    private void StartDrawing()
    {
        isDrawing = true;
        lineRenderer.positionCount = 0;
    }

    private void EndDrawing()
    {
        isDrawing = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == drawingSurface)
        {
            canDraw = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == drawingSurface)
        {
            canDraw = false;
            EndDrawing();
        }
    }
}
