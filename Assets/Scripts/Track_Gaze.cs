using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;  // Para manejar el formato de los decimales

public class GazeInteractionLogger : MonoBehaviour
{
    GameObject gazeInteractor;  // Referencia al Gaze Interactor
    Vector3 gazePos;
    Quaternion gazeRot;

    private StreamWriter csvWriter;
    private float frameInterval = 1f / 30f;  // Intervalo para 30 FPS
    private float timeSinceLastCapture = 0f;  // Tiempo acumulado para la captura de datos

    void Start()
    {
        // Busca el objeto Gaze Interactor en la jerarquía
        gazeInteractor = GameObject.Find("Gaze Interactor");

        if (gazeInteractor != null)
        {
            Debug.Log("Se encontró el objeto Gaze Interactor.");
        }
        else
        {
            Debug.LogError("No se encontró el objeto Gaze Interactor. Verifica el nombre.");
        }

        // Generar un nombre único para el archivo CSV usando la marca de tiempo
        string fileName = "gaze_interaction_data_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        Debug.Log("Guardando el archivo CSV en: " + filePath);

        try
        {
            csvWriter = new StreamWriter(filePath);

            // Escribir la cabecera en el archivo CSV
            csvWriter.WriteLine("rotationX, rotationY, rotationZ, rotationW, positionX, positionY, positionZ");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error al crear el archivo CSV: " + ex.Message);
        }
    }

    void Update()
    {
        // Acumula el tiempo desde la última captura
        timeSinceLastCapture += Time.deltaTime;

        // Si ha pasado suficiente tiempo (para 30 FPS)
        if (timeSinceLastCapture >= frameInterval)
        {
            // Captura los datos de posición y rotación
            CaptureGazeData();
            timeSinceLastCapture = 0f;  // Reinicia el contador
        }
    }

    void CaptureGazeData()
    {
        if (gazeInteractor != null)
        {
            gazePos = gazeInteractor.transform.position;
            gazeRot = gazeInteractor.transform.rotation;

            try
            {
                // Escribir los datos de posición y rotación en el archivo CSV
                csvWriter.WriteLine(string.Format(CultureInfo.InvariantCulture,
                    "{0}, {1}, {2}, {3}, {4}, {5}, {6}",
                    gazeRot.x, gazeRot.y, gazeRot.z, gazeRot.w,
                    gazePos.x, gazePos.y, gazePos.z));

                // Forzar la escritura inmediata en el archivo
                csvWriter.Flush();
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error al escribir en el archivo CSV: " + ex.Message);
            }
        }
        else
        {
            Debug.LogError("No se encontró el Gaze Interactor para capturar los datos.");
        }
    }

    private void OnDestroy()
    {
        // Cerrar el archivo CSV cuando se cierre la aplicación
        if (csvWriter != null)
        {
            csvWriter.Close();
            Debug.Log("El archivo CSV ha sido cerrado correctamente.");
        }
    }
}
