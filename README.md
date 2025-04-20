# Ubiq_Twin

**Ubiq_Twin** es parte de un proyecto de tesis de maestría en Ingeniería de Sistemas y Computación titulado *"Exploración de la Dirección de la Mirada como Indicador de Atención en Ambientes con Realidad Extendida"*.
<img width="1470" alt="Captura de pantalla 2024-11-12 a la(s) 12 04 24 p m" src="https://github.com/user-attachments/assets/bc7f481b-d865-4f45-b6a1-ffc06d73063d" />

## Resumen del Proyecto

Este proyecto investiga cómo la dirección de la mirada puede servir como un indicador de atención en distintos entornos. Este repositorio se enfoca en la **realidad virtual** (RV), donde se desarrolló un entorno virtual en Unity utilizando el paquete **[Ubiq](https://github.com/UCL-VR/ubiq)** para habilitar la conectividad multiusuario y la interacción en red entre los participantes. El estudio forma parte de una investigación más amplia que también incluye entornos presenciales y de telepresencia. 

### Objetivo Principal

El objetivo del proyecto es analizar cómo la orientación de la cabeza y la dirección de la mirada pueden reflejar el nivel de atención de los participantes dentro de un entorno de realidad virtual, permitiendo así nuevas formas de evaluar la interacción en entornos virtuales.

## Tecnologías Utilizadas

Este proyecto hace uso de varias tecnologías para crear una experiencia de realidad virtual inmersiva:

- **Entorno Virtual Desarrollado en Unity**: El espacio de realidad virtual fue creado con Unity, usando el paquete **[Ubiq](https://github.com/UCL-VR/ubiq)**, que facilita la conectividad multiusuario y la interacción en red entre los participantes.
- **Modelo 3D (Gemelo Digital)**: El modelo 3D utilizado en el proyecto es un gemelo digital recuperado del repositorio **[ColivriDigitalTwin](https://github.com/imagine-uniandes/ColivriDigitalTwin)**, que representa un espacio interactivo dentro del entorno virtual.
- **Oculus Quest 2**: El proyecto está diseñado para ejecutarse en el dispositivo de realidad virtual **Oculus Quest 2**, ofreciendo una experiencia inmersiva y natural de interacción dentro del entorno virtual.

## Realidad Virtual

En la fase de realidad virtual, los participantes interactúan dentro de un entorno virtual utilizando el **Oculus Quest 2**. La dirección de la mirada se mide y se analiza, tomando en cuenta las restricciones del campo visual del dispositivo. Durante las pruebas, los participantes se enfocan principalmente en los elementos visibles en su entorno, lo que permite estudiar cómo los objetos en el campo visual impactan su atención en la interacción.

### Principales Observaciones
- **Condicionamiento por el campo visual**: La dirección de la mirada está influenciada por las limitaciones físicas del Oculus Quest 2, lo que provoca que los participantes se concentren en los objetos directamente visibles.
- **Atención visual**: El análisis de la dirección de la mirada se utiliza como una métrica para evaluar la distribución de la atención de los participantes en el entorno virtual.

## Requisitos
- **Unity 2022.3.16f1**: Versión recomendada de Unity.
- **Git LFS**: Git Large File Storage debe estar configurado para manejar archivos grandes como modelos 3D.
- **Oculus Quest 2:** Configurado y vinculado correctamente a tu máquina para pruebas en realidad virtual.
