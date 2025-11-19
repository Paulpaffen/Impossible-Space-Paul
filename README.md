Impossible Spaces VR – Espacios Imposibles en Realidad Virtual

🧪 Tecnologías usadas

Unity 2022+ (URP)

Meta XR All-in-One SDK (Quest 3)

C# (Unity scripting)

GitHub / Git

Shader Graph / URP Materials

🔧 Pre-requisitos

Para poder abrir o modificar este proyecto necesitas instalar:

Unity 6000.2.8f1

Android Build Support

Meta XR all in one SDK

🎮 Descripción y características

Impossible Spaces VR es una experiencia inmersiva desarrollada para Meta Quest 3, basada en investigación de Change Blindness. El objetivo del proyecto es demostrar cómo un usuario puede recorrer un espacio virtual mucho más grande que el espacio físico disponible, sin darse cuenta, gracias a técnicas de redirección.

Características destacadas

🔹 Habitaciones modulares que cambian dinámicamente sin que el usuario lo perciba.

🔹 Técnica de “impossible spaces” basada en distorsión del entorno.

🔹 Espacio físico reducido (4×4 m) que simula un laberinto mucho mayor.

🔹 Mini-juego de puzles para distraer al usuario.

🔹 Diseño optimizado para Quest 3 (URP, materiales Unlit/Optimized).

📥 Instrucciones de descarga del código (para desarrollo)

1. Clonar el repositorio
git clone https://github.com/Paulpaffen/Impossible-Space-Paul.git

2. Abrir en Unity

Abrir Unity Hub

“Add project from disk”

Seleccionar la carpeta clonada

Asegurarse de usar la misma versión exacta de Unity

3. Instalar dependencias

El proyecto debería importar automáticamente:

Meta XR All-in-One SDK

XR Interaction Base del Meta SDK

Si Unity pregunta por “missing packages”, aceptar las recomendaciones.

▶️ Instrucciones de uso del ejecutable (APK)

1. Descargar APK

El APK se encuentra en la sección Releases del repositorio:

Releases → ImpossibleSpacesVR.apk

2. Instalarlo en las Quest 3

Puedes usar cualquiera de estos métodos:

✔ Meta Quest Developer Hub
✔ SideQuest → Install APK
✔ ADB:

adb install ImpossibleSpacesVR.apk

3. Ejecutar

Colocarte el visor

Asigna una zona guardian de 4x4 metros

Ir a Apps → Unknown Sources

Seleccionar Impossible Spaces VR

Antes de empezar ubicate en la zona central del guardian

4. Cómo jugar

El usuario comienza en la “sala inicial”.

Debe resolver los puzles que se le presentan.

Al atravesar puertas, el espacio cambia sin que el jugador lo perciba.

Al completar el recorrido, regresa a la sala inicial.
