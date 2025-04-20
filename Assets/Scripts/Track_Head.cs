using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Ubiq.Messaging;
using Ubiq.Rooms;
using System.Globalization;
using TMPro;

public class Track_Headset_Position : MonoBehaviour
{
    public TMP_Text timerText;  // Referencia al objeto TextMeshPro en la UI
    GameObject gazeInteractor;
    Vector3 gazePos;
    Quaternion gazeRot;

    private StreamWriter csvWriter;
    private RoomClient roomClient;
    private NetworkContext context;
    private float frameInterval = 1f / 30f;
    private float timeSinceLastCapture = 0f;

    public GameObject tablero;  // Referencia al objeto del telón
    private float roomCreationTime = 0f;  // Tiempo de creación de la sala en segundos
    private float userJoinTime = 0f;      // Tiempo en el que el usuario se unió a la sala
    private bool isInRoom = false;
    private bool roomCreationTimeReceived = false; // Indica si ya se ha recibido el tiempo de vida de la sala
    private bool isWaitingForLifeTimeResponse = false;
    private float lifeTimeRequestTimeout = 1.0f; // Tiempo máximo de espera para la respuesta
    private float lifeTimeRequestTime = 0f;      // Marca de tiempo de la solicitud

    void Start()
    {
        gazeInteractor = GameObject.Find("Main Camera");

        if (gazeInteractor != null)
        {
            Debug.Log("Se encontró el objeto Main Camera.");
        }
        else
        {
            Debug.LogError("No se encontró el objeto Main Camera. Verifica el nombre.");
        }

        roomClient = GetComponentInParent<RoomClient>();
        context = NetworkScene.Register(this); // Registro para enviar y recibir mensajes de red

        if (roomClient != null)
        {
            roomClient.OnJoinedRoom.AddListener(OnJoinedRoom);
        }
        else
        {
            Debug.LogError("RoomClient no encontrado en este GameObject.");
        }

        string fileName = "head_information_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        Debug.Log("Guardando el archivo CSV en: " + filePath);

        try
        {
            csvWriter = new StreamWriter(filePath);
            csvWriter.WriteLine("Estado, TiempoDesdeEntrada, TiempoVidaSala, rotationX, rotationY, rotationZ, rotationW, positionX, positionY, positionZ, curtainPositionX, curtainPositionY, curtainPositionZ");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error al crear el archivo CSV: " + ex.Message);
        }
    }

    private void OnDestroy()
    {
        if (csvWriter != null)
        {
            csvWriter.Close();
            Debug.Log("El archivo CSV ha sido cerrado correctamente.");
        }
    }

    void Update()
    {
        timeSinceLastCapture += Time.deltaTime;

        // Verificar si estamos esperando una respuesta de tiempo de vida de la sala
        if (isWaitingForLifeTimeResponse && !roomCreationTimeReceived)
        {
            if (Time.time - lifeTimeRequestTime > lifeTimeRequestTimeout)
            {
                // Si no se ha recibido respuesta dentro del tiempo de espera, asumimos que es el primer usuario en la sala
                roomCreationTime = Time.time;
                roomCreationTimeReceived = true;
                isWaitingForLifeTimeResponse = false;
                Debug.Log("No se recibió respuesta. Estableciendo el tiempo de vida de la sala en 0.");
            }
        }

        // Tiempo de vida de la sala: tiempo desde su creación
        float roomLifeTime = roomCreationTimeReceived ? Time.time - roomCreationTime : 0f;

        // Tiempo desde la entrada del usuario a la sala
        float userElapsedTime = isInRoom ? Time.time - userJoinTime : 0f;

        if (timerText != null)
        {
            timerText.text = $"Tiempo desde entrada: {userElapsedTime:F2} s, Vida de la sala: {roomLifeTime:F2} s";
        }

        // Llama a CaptureData en el intervalo establecido
        if (timeSinceLastCapture >= frameInterval)
        {
            CaptureData(userElapsedTime, roomLifeTime);
            timeSinceLastCapture = 0f;
        }
    }

    void CaptureData(float userElapsedTime, float roomLifeTime)
    {
        // Captura la posición y rotación del headset y del tablero
        gazePos = gazeInteractor != null ? gazeInteractor.transform.position : Vector3.zero;
        gazeRot = gazeInteractor != null ? gazeInteractor.transform.rotation : Quaternion.identity;
        Vector3 curtainPosition = tablero != null ? tablero.transform.position : Vector3.zero;

        string status = isInRoom ? "En sala" : "No en sala";

        try
        {
            csvWriter.WriteLine(string.Format(CultureInfo.InvariantCulture,
                "{0}, {1:F2}, {2:F2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}",
                status, userElapsedTime, roomLifeTime,
                gazeRot.x, gazeRot.y, gazeRot.z, gazeRot.w,
                gazePos.x, gazePos.y, gazePos.z,
                curtainPosition.x, curtainPosition.y, curtainPosition.z));

            csvWriter.Flush();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error al escribir en el archivo CSV: " + ex.Message);
        }
    }

    private void OnJoinedRoom(IRoom room)
    {
        isInRoom = true;
        userJoinTime = Time.time; // Marca el tiempo de entrada del usuario en la sala
        Debug.Log("El usuario se unió a la sala.");

        // Al unirse a la sala, envía una solicitud de tiempo de vida de la sala
        context.SendJson(new RoomCreationRequestMessage());
        isWaitingForLifeTimeResponse = true;
        lifeTimeRequestTime = Time.time; // Marca el tiempo en que se envió la solicitud
        Debug.Log("Solicitud de tiempo de vida de la sala enviada, esperando respuesta...");
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        try
        {
            string rawMessage = System.Text.Encoding.UTF8.GetString(message.bytes);
            Debug.Log("Mensaje JSON recibido (tamaño " + message.bytes.Length + "): " + rawMessage);

            int jsonStartIndex = rawMessage.IndexOf('{');
            if (jsonStartIndex != -1)
            {
                string json = rawMessage.Substring(jsonStartIndex);
                Debug.Log("Mensaje JSON después de limpiar: " + json);

                if (json.Contains("requestLifeTime"))
                {
                    // Si ya tenemos el tiempo de vida de la sala, respondemos a la solicitud
                    if (roomCreationTimeReceived)
                    {
                        float currentLifeTime = Time.time - roomCreationTime;
                        context.SendJson(new RoomLifeTimeMessage(currentLifeTime));
                        Debug.Log("Tiempo de vida de la sala reenviado: " + currentLifeTime);
                    }
                }
                else if (json.Contains("lifeTime"))
                {
                    RoomLifeTimeMessage msg = JsonUtility.FromJson<RoomLifeTimeMessage>(json);

                    // Ajustar el tiempo de vida de la sala si no se ha recibido previamente
                    if (msg != null && !roomCreationTimeReceived)
                    {
                        roomCreationTime = Time.time - msg.lifeTime;
                        roomCreationTimeReceived = true;
                        isWaitingForLifeTimeResponse = false; // Ya no estamos esperando una respuesta
                        Debug.Log("Tiempo de vida de la sala sincronizado: " + msg.lifeTime);
                    }
                }
            }
            else
            {
                Debug.LogError("No se encontró un JSON válido en el mensaje recibido.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error al procesar el mensaje JSON: " + ex.Message);
        }
    }

    // Clase de mensaje para solicitar el tiempo de vida de la sala
    [System.Serializable]
    public class RoomCreationRequestMessage
    {
        public string requestLifeTime = "requestLifeTime";
    }

    // Clase de mensaje para enviar el tiempo de vida de la sala
    [System.Serializable]
    public class RoomLifeTimeMessage
    {
        public float lifeTime;
        public RoomLifeTimeMessage(float lifeTime)
        {
            this.lifeTime = lifeTime;
        }
    }
}
