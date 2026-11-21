# Sistema de Gestión Hotelera — Hotel Fast and Furious

Proyecto de **Diseño de Sistemas de Información** (ITM, 2026). Solución en **C# / .NET 10** con
arquitectura por capas, base de datos real (Entity Framework Core), los **5 patrones GoF** del
entregable 2 y las **reglas de negocio RN-01 a RN-11** aplicadas en el backend.

> **Importante:** este código se escribió en un entorno sin el SDK de .NET, así que **no se ha
> compilado todavía**. Antes de presentarlo, ejecute `verificar.cmd` (sección 2). Si aparece
> algún error, el archivo `verificacion.log` dice exactamente dónde está.

---

## 1. Cómo ejecutarlo

**Requisitos:** Visual Studio 2026 con la carga de trabajo *Desarrollo de ASP.NET y web*
(incluye .NET 10 y SQL Server LocalDB). La primera compilación descarga los paquetes NuGet,
así que necesita internet esa vez.

1. Abra `HotelSystem.slnx` en Visual Studio.
2. Clic derecho en **HotelSystem.Web** → *Establecer como proyecto de inicio*.
3. Presione **F5**. La primera vez se crea la base de datos y se cargan los datos de demostración.
4. En el login elija un perfil: el usuario ya viene escrito y la contraseña aparece debajo.

| Perfil | Usuario | Contraseña |
|---|---|---|
| Huésped (estancia en curso, hab. 408) | `valentina.lagos` | `guest2026` |
| Recepción | `mlopez` | `recep2026` |
| Housekeeping | `amartinez` | `hk2026` |
| Coordinador de tours | `ltorres` | `coord2026` |
| Servicios | `pgomez` | `serv2026` |
| Administrador | `rsilva` | `admin2026` |
| Cuenta desactivada (HU-02 Esc.3) | `jherrera` | `hk2026` |
| Huésped menor de edad (RN-10) | `tomas.lagos` | `guest2026` |

**Para ejecutar la web y la API al tiempo:** clic derecho en la solución → *Configurar proyectos
de inicio* → *Varios proyectos de inicio* → marcar *Iniciar* en HotelSystem.Web y HotelSystem.API.

**Dejar los datos como nuevos antes de una sustentación:** en el login aparece el enlace
*Reiniciar datos de demostración* (solo en desarrollo). También se puede iniciar con el argumento
`--reiniciar-datos` o poner `"ReiniciarAlIniciar": true` en `appsettings.json`.

Las fechas de los datos de demostración se calculan a partir del día en que se cargan (siempre hay
llegadas y salidas "hoy"). Para repetir una demostración con fechas fijas, ponga una fecha en
`"Hotel": { "FechaActual": "2026-11-20" }`.

## 2. Verificar que compila y que pasan las pruebas

Doble clic en **`verificar.cmd`** (en la carpeta de la solución). Hace `dotnet build` de toda la
solución y `dotnet test` de las pruebas automatizadas, y guarda todo en `verificacion.log`.

Desde Visual Studio: *Prueba → Explorador de pruebas → Ejecutar todas*.

Hay **59 pruebas** (61 casos, xUnit) en `HotelSystem.Tests`. Usan el sistema completo (mismas capas y misma
inyección de dependencias) sobre una base SQLite temporal, con un reloj fijo:

| Archivo | Qué prueba |
|---|---|
| `DominioTests` | Patrón State (transiciones permitidas y rechazadas), Observer, `ICalculable`/`IPagable`, RN-11, cruce de fechas |
| `UsuariosTests` | Login (HU-02 Esc.1 a 3), Factory Method, contraseña temporal, cambio de rol (RN-08), reactivación (HU-01 Esc.5), RN-10 |
| `ReservasTests` | Reserva con noches reales, correo HU-04, RN-01, RN-04, RN-09, RN-10, RN-11, check-in, concurrencia (RNF-07) |
| `CuentaTests` | Facade: prefactura consolidada (RN-07), cierre de cuenta todo-o-nada, RN-06 |
| `ToursTests` | Cupos (RN-05, RN-11), guía y vehículo con horario cruzado (RN-03), tours cargados a la cuenta |
| `HabitacionesTests` | Housekeeping: RN-02 desde el servicio, incidencia con nota, paneles Observer, plan de limpieza |
| `ReportesTests` | Reporte con datos reales, periodo sin datos (HU-13 Esc.2), PDF y Excel válidos, Strategy + Liskov |

## 3. Arquitectura

```
HotelSystem.Domain          Entidades, enums, patrón State, interfaces ICalculable / IPagable / IObservadorHabitacion
HotelSystem.Application     Servicios por módulo (C4 nivel 3), reglas de negocio, Facade, Factory Method, Strategy (contexto), Observer (paneles)
HotelSystem.Persistence     Entity Framework Core: HotelDbContext, repositorios, unidad de trabajo, datos de demostración
HotelSystem.Infrastructure  Reloj, contraseñas (PBKDF2), correo, exportadores PDF y Excel (estrategias)
HotelSystem.Web             ASP.NET Core MVC (las pantallas del prototipo, ahora con datos reales)
HotelSystem.API             API REST con JWT para el front HTML/JS
HotelSystem.Tests           Pruebas automatizadas (xUnit)
```

Dependencias: `Domain ← Application ← Persistence / Infrastructure ← Web / API`. El dominio no
conoce la base de datos; los servicios dependen de interfaces (`IReservaHabitacionRepository`,
`IServicioCorreo`, `IReloj`...) y la inyección de dependencias conecta las implementaciones.

Los seis componentes del **C4 nivel 3** son las carpetas de `HotelSystem.Application`:
`Usuarios` (Usuarios y Accesos), `Reservas`, `ServiciosAdicionales`, `Tours`, `Housekeeping` y
`CierreCuenta`.

**Diferencia con el C4 nivel 2:** el diagrama dibuja la web llamando a la API. En el código, la web
MVC usa la capa de aplicación directamente (en el mismo proceso) y la API expone esos mismos servicios
por REST para el front HTML/JS. Las reglas están en un solo lugar y las dos entradas las comparten.
Conviene ajustar la flecha del diagrama o mencionarlo en la sustentación.

## 4. Patrones GoF

| Patrón | Dónde está | Qué resuelve |
|---|---|---|
| **State** | `Domain/Estados/` (`IEstadoHabitacion`, `EstadoDisponible`, `EstadoOcupada`, `EstadoEnLimpieza`, `EstadoMantenimiento`) y `Habitacion` | Cada estado decide qué transiciones permite (RN-02). Una transición inválida lanza la excepción con el mensaje de la regla |
| **Observer** | Sujeto: `Habitacion` (`Suscribir`, notifica en cada cambio). Observadores: `PanelRecepcion`, `PanelHousekeeping` (`Application/Reservas/`) | Recepción y housekeeping se enteran de los cambios de estado. Las pantallas consultan `/Eventos/Version` cada 3 s y se recargan solas |
| **Facade** | `CierreCuenta/CuentaFacade.cs` | Una sola llamada (`CerrarCuenta`) consolida la cuenta, valida el pago, cierra la cuenta, finaliza la reserva, deja la habitación en limpieza y genera la prefactura, en una transacción |
| **Factory Method** | `Usuarios/CreadorUsuario.cs` (creador abstracto), `CreadorHuesped`, `CreadorEmpleado` | El flujo de alta es común; cada subclase decide qué `Persona` crea y sus validaciones (RN-10 para huésped, RN-08 para empleado) |
| **Strategy** | Interfaz `CierreCuenta/IExportadorReporte.cs`; estrategias `Infrastructure/Reportes/ExportadorPDF.cs` y `ExportadorExcel.cs`; contexto `GeneradorReportes` | El formato del reporte (HU-13) se elige en tiempo de ejecución. Un formato nuevo es una clase nueva y una línea en la inyección de dependencias |

## 5. Correcciones SOLID

| Antes (prototipo) | Ahora |
|---|---|
| `IHotelDataService` con todas las operaciones del hotel (violaba ISP) | Un servicio por módulo: `IAuthService`, `IUsuarioService`, `IReservaService`, `IHabitacionService`, `ITourService`, `IServicioAdicionalService`, `ICuentaService`, `IReporteService`. Cada controlador recibe solo los que usa |
| Reglas de negocio dentro de los controladores (SRP) | Los controladores solo leen el formulario, llaman al servicio y eligen la vista. Las reglas están en el dominio y en `Application` |
| `MockData` en memoria | Base de datos real con Entity Framework Core |
| Cálculo de cancelación fijo | `IPoliticaCancelacion` (`PoliticaCancelacion48Horas`), configurable en `appsettings.json` (OCP) |
| Totales calculados en cada pantalla | `ICalculable` (Cuenta, ReservaTour) e `IPagable` (Cuenta) en el dominio (LSP, ISP) |
| `DateTime.Now` disperso | `IReloj` inyectado (DIP): las pruebas y la demostración fijan la fecha |

## 6. Reglas de negocio

| Regla | Dónde se aplica |
|---|---|
| RN-01 Sin doble reserva | `ReservaService.ReservarHabitacion` (transacción serializable) y `BuscarDisponibles` |
| RN-02 Estados de habitación | Patrón State; check-in solo con habitación Disponible; Ocupada solo por check-in |
| RN-03 Guía/vehículo sin cruce de horario | `TourService.AsignarRecursos` y `ListarRecursos` |
| RN-04 Respaldo de pago | `ReservarHabitacion`, `ReservarTour` |
| RN-05 Cupos de tour bloqueados | `ReservarTour` bloquea en la misma transacción; `LiberarCupos` no libera cupos de huéspedes |
| RN-06 Consumos del mismo día | `ServicioAdicionalService.RegistrarConsumo` |
| RN-07 Cuenta única | `Cuenta` (alojamiento + servicios + tours); consumos y tours solo con cuenta abierta |
| RN-08 Un rol por empleado | `Empleado.Rol` único; `CambiarRol` reemplaza el anterior |
| RN-09 Cancelación < 48 h cobra 1 noche | `PoliticaCancelacion48Horas` |
| RN-10 Titular mayor de edad | Fecha de nacimiento (`Persona.EsMayorDeEdad`) además de la casilla del formulario |
| RN-11 Capacidad | `ReservaHabitacion` (capacidad de la habitación) y `Tour` (cupo máximo) |
| RNF-07 Concurrencia | `UnidadDeTrabajo`: transacciones serializables; un choque se informa como mensaje, no como error |

Los mensajes de las reglas se muestran tal cual en la web y en la API (`{ "mensaje": "..." }`).

## 7. Historias de usuario

| HU | Web (controlador) | API |
|---|---|---|
| HU-01 Gestión de usuarios | `AdminController` | `/api/usuarios` |
| HU-02 Inicio de sesión | `AccountController` | `/api/auth/login` |
| HU-03 Reservar / cancelar habitación | `HuespedController` | `/api/habitaciones/disponibles`, `/api/reservas` |
| HU-04 Correo de confirmación | `ReservaService` + `IServicioCorreo` | (al reservar) |
| HU-05 Panel de habitaciones | `RecepcionController.Index` | `/api/habitaciones` |
| HU-06 Check-in / check-out | `RecepcionController` | `/api/recepcion`, `/api/cuentas/{hab}/cerrar` |
| HU-07 Solicitar servicios | `HuespedController` | `/api/servicios`, `/api/solicitudes-servicio` |
| HU-08 Registrar consumos | `ServiciosController` | `/api/consumos` |
| HU-09 Reservar tour | `HuespedController` | `/api/tours/{id}/reservas` |
| HU-10 Cupos y recursos de tours | `ToursController` | `/api/tours/...` |
| HU-11 Housekeeping | `HousekeepingController` | `/api/habitaciones/{id}/estado`, `/api/housekeeping/plan-limpieza` |
| HU-12 Prefactura / mi cuenta | `RecepcionController.Facturacion`, `HuespedController.MiCuenta` | `/api/cuentas/...` |
| HU-13 Reportes y exportación | `AdminController.Reportes` | `/api/reportes`, `/api/reportes/exportar` |

## 8. API REST (para el front HTML/JS)

- URL: `https://localhost:7001/api` (la misma que trae `js/config.js` del front). En `js/config.js`
  cambie `USE_MOCK: false`.
- Autenticación: `POST /api/auth/login` con `{ "usuario", "contrasena" }` devuelve `{ token, expira, usuario }`.
  Las demás rutas piden `Authorization: Bearer <token>`.
- Errores: código 4xx con `{ "mensaje": "...", "regla": "RN-01" }` (400 regla incumplida, 401 sin
  sesión, 403 rol sin permiso, 404 no existe).
- Enums como texto en mayúsculas: `DISPONIBLE`, `EN_LIMPIEZA`, `COORDINADOR_TOURS`... Al enviar, la
  API también acepta `EnLimpieza` o `en limpieza`.
- CORS habilitado para `http://127.0.0.1:5500` y `http://localhost:5500` (Live Server).
- Ejemplos listos para ejecutar en `HotelSystem.API/HotelSystem.API.http` (Visual Studio los envía con un clic).
- Documento OpenAPI: `https://localhost:7001/openapi/v1.json`.

Las rutas son las del README del front. Las respuestas usan los nombres de los DTOs del backend
(por ejemplo `fechaEntrada` en "mis reservas"). Si alguna pantalla del front espera otro nombre de
campo, el ajuste se hace en `js/api.js`, que es el único punto de acceso al backend.

## 9. Base de datos

- Por defecto **SQL Server LocalDB** (`(localdb)\MSSQLLocalDB`, base `HotelFastAndFurious`), como en el C4.
- `"Proveedor": "Auto"`: si SQL Server no responde, el sistema usa un archivo **SQLite**
  (`%LOCALAPPDATA%\HotelFastAndFurious\hotel.db`) y lo indica en la consola al arrancar.
  Para forzar uno: `"Proveedor": "SqlServer"` o `"Sqlite"`.
- Las tablas se crean solas (`EnsureCreated`). No hay migraciones: si cambia el modelo, reinicie los datos.
- La web y la API comparten la misma base de datos.

## 10. Correo (HU-04)

En `appsettings.json`, sección `Correo`:

- `"Modo": "Carpeta"` (por defecto): cada correo se guarda como página HTML en
  `HotelSystem.Web/App_Data/correos` para mostrarlo en la sustentación sin enviar nada.
- `"Modo": "Smtp"`: envío real. Con Gmail se necesita una *contraseña de aplicación* en `Clave`.
  No suba esa clave al repositorio.
- `"Modo": "Desactivado"`: no envía nada; la reserva se confirma igual.

## 11. Lo que falta o conviene saber

- **RNF-02 (cifrado AES-256 de datos en reposo) no está implementado.** Las contraseñas sí se
  guardan con hash PBKDF2-SHA256 (100.000 iteraciones), nunca en texto plano.
- En la API, un cambio de rol o una desactivación aplica cuando el usuario vuelve a iniciar sesión
  (el token dura 8 horas). La clave `Jwt:Clave` de `appsettings.json` es solo para desarrollo.
- Clases que no estaban en el diagrama de clases y conviene agregar: `SolicitudServicio`,
  `RegistroOperacion` (bitácora para el reporte de personal), `Prefactura` como entidad guardada,
  los enums `EstadoCuenta`, `EstadoSolicitud` y `TipoOperacion`, y `Vehiculo.Nombre`.
- El código no se ha compilado en este entorno: ejecute `verificar.cmd` antes de entregarlo.
