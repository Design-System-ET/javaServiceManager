# Jar Service Manager NSSM

**Jar Service Manager** es una aplicación de escritorio en **C# Windows Forms** que permite instalar, gestionar y monitorear archivos `.jar` como servicios de Windows utilizando **NSSM** (Non-Sucking Service Manager). Facilita la instalación automática, logs, auto-restart y gestión de múltiples JARs como servicios.

---

## Tabla de contenido

- [Características](#características)  
- [Requisitos](#requisitos)  
- [Instalación](#instalación)  
- [Uso](#uso)  
  - [Agregar un JAR como servicio](#agregar-un-jar-como-servicio)  
  - [Filtros y búsqueda](#filtros-y-búsqueda)  
  - [Gestión de servicios](#gestión-de-servicios)  
  - [Ver logs](#ver-logs)  
- [Funciones principales](#funciones-principales)  
- [Estructura del proyecto](#estructura-del-proyecto)  
- [Licencia](#licencia)  

---

## Características

- Instala archivos `.jar` como servicios de Windows con NSSM.  
- Auto-inicio de servicios al arrancar Windows.  
- Reinicio automático de servicios en caso de fallo.  
- Logs separados para stdout y stderr de cada JAR.  
- Monitoreo de estado del servicio (`Running` o `Stopped`).  
- Visualización de uptime de cada servicio.  
- Gestión de múltiples servicios desde una interfaz gráfica.  
- Filtro en tiempo real por nombre de servicio.  
- Acceso rápido a los logs del servicio.  

---

## Requisitos

- Windows 10 o superior.  
- .NET Framework 4.8 o superior.  
- Java Runtime Environment (JRE) instalado.  
- NSSM (incluido en la carpeta `nssm` del proyecto).  

---

## Instalación

1. Clona el repositorio o descarga el proyecto.  
2. Asegúrate de que la carpeta `nssm` contenga `nssm.exe`.  
3. Compila el proyecto en Visual Studio.  
4. Ejecuta `JarServiceManager.exe` como administrador.  

---

## Uso

### Agregar un JAR como servicio

1. Haz clic en el botón `+ JAR` y selecciona el archivo `.jar`.  
2. Opcionalmente, edita el campo `New Service` para personalizar el nombre del servicio.  
3. Haz clic en `Install Jar`.  
4. El servicio se instalará automáticamente con logs habilitados y auto-restart.  

### Filtros y búsqueda

- Escribe en el campo `Find` para filtrar los servicios por nombre en tiempo real.  
- La búsqueda es insensible a mayúsculas/minúsculas.  

### Gestión de servicios

- **Refresh**: Actualiza la lista de servicios y sus estados.  
- **Start**: Inicia el servicio seleccionado.  
- **Stop**: Detiene el servicio seleccionado.  
- **Restart**: Reinicia el servicio seleccionado.  
- **Uninstall**: Detiene y elimina el servicio seleccionado de Windows.  

### Ver logs

- Haz clic en `View Log` para abrir la carpeta de logs del servicio seleccionado.  
- Los logs se almacenan en:

```text
C:\ProgramData\JarServiceManager\Logs\<ServiceName>\
```

- Incluye stdout.log y stderr.log.

### Funciones Principales
- CargarServicios():	Actualiza la grilla DataGridView con todos los servicios JAR que comienzan con JSM-JAR-, mostrando nombre, estado y tiempo de ejecución.

- GetServiceStatus(string serviceName):	Devuelve "Running" o "Stopped" consultando NSSM para el servicio especificado.
- GetServiceUptime(string serviceName):	Calcula el tiempo que el JAR ha estado corriendo consultando el proceso java.exe correspondiente.
- GetCommandLine(Process process):	Obtiene la línea de comandos de un proceso usando WMI.
- ProcessStart(string fileName, string arguments):	Ejecuta un proceso externo (NSSM o cualquier comando) como administrador y espera a que finalice.
- txtFiltro_TextChanged:	Evento para filtrar servicios en tiempo real según el texto ingresado.
- btnBrowse_Click:	Abre un diálogo para seleccionar un archivo .jar.
- btnInstall_Click:	Instala y arranca un servicio JAR usando NSSM, configura logs y auto-restart.
- btnUninstall_Click:	Detiene y elimina un servicio JAR de Windows.
- btnStart_Click:	Inicia un servicio JAR seleccionado.
- btnStop_Click:	Detiene un servicio JAR seleccionado.
- btnRestart_Click:	Reinicia un servicio JAR seleccionado.
- btnViewLog_Click:	Abre la carpeta de logs del servicio seleccionado.
- btnRefresh_Click:	Refresca la lista de servicios y sus estados.

### Estructura del Proyecto
```JarServiceManager/
├─ Form1.cs
├─ Form1.Designer.cs
├─ Program.cs
├─ nssm/
│   └─ nssm.exe
├─ Properties/
│   └─ Resources.resx
├─ README.md
````

- - Form1.cs: Lógica de negocio y control de servicios.

- Form1.Designer.cs: Diseño de la interfaz gráfica.

- nssm/nssm.exe: Ejecutable de NSSM para manejar servicios de Windows.

- Properties/Resources.resx: Recursos de la aplicación (iconos, imágenes)

### Licencia
Este proyecto está bajo la licencia MIT.

Ver LICENSE para más detalles.