using UnityEngine;
using Ubiq.Rooms;

public class RoomClientTimer : MonoBehaviour
{
    private RoomClient roomClient;
    private float timer = 0f;
    private bool isCounting = false;

    private void Awake()
    {
        // Obtén la referencia a RoomClient en el mismo GameObject
        roomClient = GetComponent<RoomClient>();

        // Asegúrate de que roomClient no sea nulo y añade el listener
        if (roomClient != null)
        {
            roomClient.OnJoinedRoom.AddListener(StartTimer);
        }
    }

    private void Update()
    {
        if (isCounting)
        {
            timer += Time.deltaTime; // Incrementa el temporizador si está contando
        }
    }

    private void StartTimer(IRoom room)
    {
        isCounting = true; // Comienza a contar cuando se une a una sala
        Debug.Log("Timer started for room: " + room.Name);
    }

    public float GetElapsedTime()
    {
        return timer; // Devuelve el tiempo transcurrido
    }
}
