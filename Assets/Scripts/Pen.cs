using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
using System.Collections.Generic;
using Ubiq.Messaging;

public class Pen : MonoBehaviour
{
    private NetworkContext context;
    private bool owner;
    private Transform nib;
    private Material drawingMaterial;
    private GameObject currentDrawing;
    private List<GameObject> drawings = new List<GameObject>(); // Almacena los trazos
    public bool eraseMode = false; // Modo borrar activado o desactivado
    public GameObject telonObject; // Referencia al objeto Telón
    private bool canDraw = false; // Controlar si se permite dibujar solo en el Telón

    private InputDevice rightController; // Controlador derecho (Oculus)
    private bool buttonPressedLastFrame = false; // Para evitar múltiples alternancias con una pulsación

    private struct Message
    {
        public Vector3 position;  // Posición del lápiz
        public Quaternion rotation;  // Rotación del lápiz
        public bool isDrawing;  // Estado de dibujo
        public bool eraseMode;  // Si está en modo borrado

        public Message(Transform transform, bool isDrawing, bool eraseMode)
        {
            this.position = transform.position;
            this.rotation = transform.rotation;
            this.isDrawing = isDrawing;
            this.eraseMode = eraseMode; // Indicar si está en modo borrado
        }
    }

    private void Start()
    {
        nib = transform.Find("Nib");

        var shader = Shader.Find("Sprites/Default");
        drawingMaterial = new Material(shader);

        var grab = GetComponent<XRGrabInteractable>();
        grab.activated.AddListener(XRGrabInteractable_Activated);
        grab.deactivated.AddListener(XRGrabInteractable_Deactivated);

        grab.selectEntered.AddListener(XRGrabInteractable_SelectEntered);
        grab.selectExited.AddListener(XRGrabInteractable_SelectExited);

        context = NetworkScene.Register(this);

        TryInitializeController();
    }

    private void TryInitializeController()
    {
        // Inicializar el controlador derecho (característica del controlador derecho)
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller, devices);

        if (devices.Count > 0)
        {
            rightController = devices[0]; // Asignar el controlador derecho de Oculus
        }
    }

    private void FixedUpdate()
    {
        if (!rightController.isValid) // Si no es válido, intentar inicializarlo nuevamente
        {
            TryInitializeController();
        }

        // Alternar el modo de borrado con el botón A (en el controlador derecho de Oculus)
        bool buttonPressed;
        if (rightController.TryGetFeatureValue(CommonUsages.primaryButton, out buttonPressed) && buttonPressed)
        {
            if (!buttonPressedLastFrame) // Solo alternar cuando el botón se presiona por primera vez
            {
                ToggleEraseMode();
                buttonPressedLastFrame = true; // Marcar que el botón fue presionado
            }
        }
        else
        {
            buttonPressedLastFrame = false; // Restablecer cuando el botón se suelta
        }

        if (owner)
        {
            // Enviar la posición, el estado de dibujo y si está en modo de borrado
            context.SendJson(new Message(transform, isDrawing: currentDrawing, eraseMode: eraseMode));
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage msg)
    {
        var data = msg.FromJson<Message>();
        transform.position = data.position;
        transform.rotation = data.rotation;

        // Si el lápiz está en modo de borrado, eliminar el último trazo
        if (data.eraseMode)
        {
            DeleteLastDrawing(); // Eliminar el último trazo en todos los clientes
        }
        else
        {
            // Sincronizar el inicio o fin del dibujo según el estado de la red
            if (data.isDrawing && !currentDrawing)
            {
                BeginDrawing();
            }
            if (!data.isDrawing && currentDrawing)
            {
                EndDrawing();
            }
        }
    }

    private void ToggleEraseMode()
    {
        eraseMode = !eraseMode; // Alternar entre modo de dibujo y borrado
        Debug.Log("Modo borrado: " + eraseMode);
    }

    private void XRGrabInteractable_Activated(ActivateEventArgs eventArgs)
    {
        if (canDraw) // Solo permitir el dibujo si el lápiz está en contacto con el Telón
        {
            if (eraseMode)
            {
                DeleteLastDrawing(); // Eliminar el trazo localmente
            }
            else
            {
                BeginDrawing();
            }
        }
    }

    private void XRGrabInteractable_Deactivated(DeactivateEventArgs eventArgs)
    {
        if (!eraseMode) // Solo detener el dibujo si no estamos borrando
        {
            EndDrawing();
        }
    }

    private void XRGrabInteractable_SelectEntered(SelectEnterEventArgs arg0)
    {
        owner = true;
    }

    private void XRGrabInteractable_SelectExited(SelectExitEventArgs eventArgs)
    {
        owner = false;
    }

    private void BeginDrawing()
    {
        currentDrawing = new GameObject("Drawing");
        var trail = currentDrawing.AddComponent<TrailRenderer>();
        trail.time = Mathf.Infinity;
        trail.material = drawingMaterial;
        trail.startWidth = .005f;
        trail.endWidth = .005f;
        trail.minVertexDistance = .002f;

        currentDrawing.transform.parent = nib.transform;
        currentDrawing.transform.localPosition = Vector3.zero;
        currentDrawing.transform.localRotation = Quaternion.identity;

        drawings.Add(currentDrawing); // Añadir el trazo a la lista de trazos
    }

    private void EndDrawing()
    {
        if (currentDrawing != null)
        {
            currentDrawing.transform.parent = null;
            currentDrawing.GetComponent<TrailRenderer>().emitting = false;
            currentDrawing = null;
        }
    }

    // Eliminar el último trazo dibujado
    private void DeleteLastDrawing()
    {
        if (drawings.Count > 0)
        {
            GameObject lastDrawing = drawings[drawings.Count - 1];
            Destroy(lastDrawing); // Destruir el último trazo
            drawings.RemoveAt(drawings.Count - 1); // Remover de la lista
        }
    }

    // Eliminar todos los trazos
    private void DeleteAllDrawings()
    {
        foreach (var drawing in drawings)
        {
            Destroy(drawing);
        }
        drawings.Clear(); // Limpiar la lista
    }

    // Detectar cuando el lápiz entra en contacto con el Telón
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == telonObject) // Verificar si entró en el Telón
        {
            canDraw = true; // Permitir el dibujo cuando está en el Telón
            Debug.Log("Lápiz tocando el Telón. Puede dibujar.");
        }
    }

    // Detectar cuando el lápiz sale del Telón
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == telonObject) // Verificar si salió del Telón
        {
            canDraw = false; // No permitir el dibujo cuando está fuera del Telón
            Debug.Log("Lápiz salió del Telón. No puede dibujar.");
        }
    }
}
