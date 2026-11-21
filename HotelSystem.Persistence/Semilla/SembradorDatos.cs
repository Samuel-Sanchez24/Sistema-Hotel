using System.Globalization;
using HotelSystem.Application.Abstracciones;
using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Enumeraciones;
using Microsoft.EntityFrameworkCore;

namespace HotelSystem.Persistence.Semilla;

/// <summary>
/// Carga los datos de demostración la primera vez que se crea la base de datos. Son los mismos
/// del prototipo (el antiguo MockData), pero con fechas relativas al día en que se siembran:
/// "hoy" tiene llegadas, salidas y tours, sin importar cuándo se ejecute el sistema.
///
/// Todo se crea con las operaciones del dominio (reservar, confirmar, hacer check-in, bloquear
/// cupos...), así que los datos iniciales cumplen las mismas reglas que los datos reales.
/// </summary>
public sealed class SembradorDatos(HotelDbContext db, IHasherContrasenas hasher, IReloj reloj)
{
    public const string ClaveHuespedes = "guest2026";

    private const string RespaldoPago = "Visa **** 4242";
    private const string NotaDano = "Dano reportado al ingresar turno";

    // Número, estado inicial y hora de la última actualización de cada habitación.
    private static readonly (string Numero, EstadoHabitacion Estado, string Hora)[] HabitacionesIniciales =
    [
        ("101", EstadoHabitacion.Disponible, "09:12"), ("102", EstadoHabitacion.Ocupada, "07:30"), ("103", EstadoHabitacion.Disponible, "08:45"),
        ("104", EstadoHabitacion.Disponible, "10:00"), ("105", EstadoHabitacion.EnLimpieza, "11:02"), ("106", EstadoHabitacion.Disponible, "09:30"),
        ("107", EstadoHabitacion.Ocupada, "08:00"), ("108", EstadoHabitacion.Disponible, "10:15"), ("109", EstadoHabitacion.Mantenimiento, "06:00"),
        ("110", EstadoHabitacion.Disponible, "09:50"), ("201", EstadoHabitacion.Disponible, "09:10"), ("202", EstadoHabitacion.Ocupada, "07:45"),
        ("203", EstadoHabitacion.EnLimpieza, "11:30"), ("204", EstadoHabitacion.Disponible, "10:05"), ("205", EstadoHabitacion.Ocupada, "08:20"),
        ("206", EstadoHabitacion.Disponible, "09:55"), ("207", EstadoHabitacion.Disponible, "10:10"), ("208", EstadoHabitacion.Ocupada, "07:55"),
        ("209", EstadoHabitacion.Disponible, "10:20"), ("210", EstadoHabitacion.Disponible, "09:40"), ("301", EstadoHabitacion.Disponible, "09:05"),
        ("302", EstadoHabitacion.Disponible, "09:48"), ("303", EstadoHabitacion.Ocupada, "07:35"), ("304", EstadoHabitacion.Mantenimiento, "05:30"),
        ("305", EstadoHabitacion.Disponible, "10:25"), ("306", EstadoHabitacion.Ocupada, "08:10"), ("307", EstadoHabitacion.Disponible, "09:52"),
        ("308", EstadoHabitacion.Disponible, "10:30"), ("309", EstadoHabitacion.EnLimpieza, "11:45"), ("310", EstadoHabitacion.Disponible, "10:00"),
        ("401", EstadoHabitacion.Ocupada, "07:40"), ("402", EstadoHabitacion.Disponible, "09:15"), ("403", EstadoHabitacion.Disponible, "09:30"),
        ("404", EstadoHabitacion.Ocupada, "07:50"), ("405", EstadoHabitacion.Disponible, "10:00"), ("406", EstadoHabitacion.Disponible, "09:45"),
        ("407", EstadoHabitacion.Disponible, "10:05"), ("408", EstadoHabitacion.Ocupada, "08:05"), ("409", EstadoHabitacion.Disponible, "09:20"),
        ("410", EstadoHabitacion.EnLimpieza, "11:15"), ("501", EstadoHabitacion.Disponible, "09:25"), ("502", EstadoHabitacion.Ocupada, "07:25"),
        ("503", EstadoHabitacion.Disponible, "09:50"), ("504", EstadoHabitacion.Disponible, "10:10"), ("505", EstadoHabitacion.Ocupada, "08:15"),
        ("506", EstadoHabitacion.Disponible, "10:20"), ("507", EstadoHabitacion.Disponible, "09:35"), ("508", EstadoHabitacion.Disponible, "10:40"),
        ("509", EstadoHabitacion.Mantenimiento, "04:00"), ("510", EstadoHabitacion.Disponible, "09:55"), ("601", EstadoHabitacion.Ocupada, "08:00"),
        ("602", EstadoHabitacion.Disponible, "10:00"), ("603", EstadoHabitacion.Disponible, "09:45"), ("604", EstadoHabitacion.Disponible, "10:05"),
        ("605", EstadoHabitacion.Disponible, "09:30"), ("606", EstadoHabitacion.Disponible, "10:15"), ("607", EstadoHabitacion.Disponible, "09:55"),
        ("608", EstadoHabitacion.Disponible, "10:20"), ("609", EstadoHabitacion.Mantenimiento, "03:30"), ("610", EstadoHabitacion.Disponible, "10:00"),
    ];

    public void Sembrar()
    {
        if (db.Habitaciones.Any()) return;

        var ahora = reloj.Ahora;
        var hoy = reloj.Hoy;
        var fechaAlta = ahora.AddMonths(-6);

        using var transaccion = db.Database.BeginTransaction();

        /* ═══ Etapa 1: datos maestros ═══ */

        // Personal (HU-01). Un rol por empleado (RN-08). jherrera está desactivado (HU-02 Esc.3).
        var rsilva = NuevoEmpleado("Roberto Silva", "rsilva", "admin2026", RolEmpleado.Administrador, fechaAlta);
        var mlopez = NuevoEmpleado("Maria Lopez", "mlopez", "recep2026", RolEmpleado.Recepcion, fechaAlta);
        var cruiz = NuevoEmpleado("Carlos Ruiz", "cruiz", "recep2026", RolEmpleado.Recepcion, fechaAlta);
        var amartinez = NuevoEmpleado("Ana Martinez", "amartinez", "hk2026", RolEmpleado.Housekeeping, fechaAlta);
        var ltorres = NuevoEmpleado("Luis Torres", "ltorres", "coord2026", RolEmpleado.CoordinadorTours, fechaAlta);
        var pgomez = NuevoEmpleado("Paula Gomez", "pgomez", "serv2026", RolEmpleado.Servicios, fechaAlta);
        var jherrera = NuevoEmpleado("Juan Herrera", "jherrera", "hk2026", RolEmpleado.Housekeeping, fechaAlta);
        jherrera.Desactivar();
        db.Empleados.AddRange(rsilva, mlopez, cruiz, amartinez, ltorres, pgomez, jherrera);

        // Huéspedes. Todos entran con la contraseña guest2026.
        var huespedes = new Dictionary<string, Huesped>();
        (string Nombre, string Usuario, int Edad)[] datosHuespedes =
        [
            ("Valentina Lagos", "valentina.lagos", 29),
            ("Carlos Mejia", "carlos.mejia", 41),
            ("Sandra Velez", "sandra.velez", 35),
            ("Familia Ramos", "familia.ramos", 44),
            ("Maria Torres", "maria.torres", 31),
            ("Pedro Salinas", "pedro.salinas", 52),
            ("Familia Gutierrez", "familia.gutierrez", 47),
            ("Andres Castro", "andres.castro", 28),
            ("Laura Pena", "laura.pena", 33),
            ("Jorge Ruiz", "jorge.ruiz", 39),
            ("Familia Morales", "familia.morales", 45),
            ("Gustavo Vargas", "gustavo.vargas", 36),
            ("Luna de Miel Diaz", "luna.diaz", 27),
            ("Esperanza Rios", "esperanza.rios", 58),
            ("Familia Pedraza", "familia.pedraza", 42),
            ("Camila Duarte", "camila.duarte", 30),
            // Menor de edad: cuenta de prueba para demostrar la RN-10 al reservar.
            ("Tomas Lagos", "tomas.lagos", 16),
        ];
        foreach (var dato in datosHuespedes)
        {
            var nacimiento = hoy.AddYears(-dato.Edad).AddDays(-40);
            var huesped = new Huesped(dato.Nombre, $"{dato.Usuario}@email.com", dato.Usuario, hasher.Hash(ClaveHuespedes), nacimiento, fechaAlta);
            huespedes[dato.Nombre] = huesped;
            db.Huespedes.Add(huesped);
        }

        // Catálogo de servicios adicionales. Precio 0 = precio variable según el consumo.
        var servicios = new[]
        {
            new ServicioAdicional("Restaurante", "Gastronomia", "Servicio de restaurante en el hotel", 0m),
            new ServicioAdicional("Spa", "Bienestar", "Masajes y tratamientos de spa", 80000m),
            new ServicioAdicional("Bar", "Gastronomia", "Bar y bebidas", 0m),
            new ServicioAdicional("Lavanderia", "Habitacion", "Servicio de lavado y planchado de ropa", 25000m),
            new ServicioAdicional("Transporte Aeropuerto", "Transporte", "Traslado al aeropuerto internacional", 60000m),
            new ServicioAdicional("Room Service", "Gastronomia", "Servicio de comidas a la habitacion", 0m),
            new ServicioAdicional("Minibar", "Gastronomia", "Recarga de minibar", 0m, activo: false),
            new ServicioAdicional("Tour Privado", "Actividades", "Excursiones personalizadas", 150000m),
        }.ToDictionary(s => s.Nombre);
        db.Servicios.AddRange(servicios.Values);

        // Guías y vehículos (HU-10).
        var guias = new[] { "Alberto Cruz", "Mario Rios", "Elena Mora", "Carmen Vega", "Diego Pardo" }
            .Select(nombre => new Guia(nombre))
            .ToDictionary(g => g.Nombre);
        db.Guias.AddRange(guias.Values);

        var vehiculos = new[]
        {
            new Vehiculo("Van #001", "TUR-001", "Van de 15 pasajeros", 15),
            new Vehiculo("Van #002", "TUR-002", "Van de 15 pasajeros", 15),
            new Vehiculo("Van #003", "TUR-003", "Van de 15 pasajeros", 15),
            new Vehiculo("Bus #003", "TUR-103", "Bus de 30 pasajeros", 30),
            new Vehiculo("Lancha #01", "LAN-001", "Lancha de 12 pasajeros", 12),
            new Vehiculo("Lancha #02", "LAN-002", "Lancha de 12 pasajeros", 12),
        }.ToDictionary(v => v.Nombre);
        db.Vehiculos.AddRange(vehiculos.Values);

        // 60 habitaciones: pisos 1-2 simples, 3-4 dobles, 5-6 suites.
        var habitaciones = new Dictionary<string, Habitacion>();
        foreach (var inicial in HabitacionesIniciales)
        {
            var piso = inicial.Numero[0] - '0';
            var (tipo, capacidad, precio) = piso switch
            {
                <= 2 => (TipoHabitacion.Simple, 2, 120000m),
                <= 4 => (TipoHabitacion.Doble, 3, 150000m),
                _ => (TipoHabitacion.Suite, 4, 250000m),
            };
            var hora = TimeOnly.ParseExact(inicial.Hora, "HH:mm", CultureInfo.InvariantCulture);
            var actualizada = Momento(hoy, hora.Hour, hora.Minute, ahora);

            var habitacion = new Habitacion(inicial.Numero, tipo, piso, capacidad, precio, actualizada);
            if (inicial.Estado == EstadoHabitacion.EnLimpieza) habitacion.EnviarALimpieza(actualizada);
            if (inicial.Estado == EstadoHabitacion.Mantenimiento) habitacion.EnviarAMantenimiento(NotaDano, actualizada);
            // Las ocupadas pasan a ese estado más abajo, con el check-in de su estancia.

            habitaciones[inicial.Numero] = habitacion;
            db.Habitaciones.Add(habitacion);
        }

        db.SaveChanges();

        /* ═══ Etapa 2: operación del hotel ═══ */

        // Estancias en curso. Entrada y Salida son días respecto a hoy. Dos salen hoy.
        (string Numero, string Huesped, int Entrada, int Salida, int Personas)[] estancias =
        [
            ("102", "Carlos Mejia", -4, 0, 1),
            ("107", "Sandra Velez", -2, 1, 1),
            ("202", "Familia Ramos", -3, 2, 2),
            ("205", "Maria Torres", -1, 3, 2),
            ("208", "Pedro Salinas", -2, 1, 1),
            ("303", "Familia Gutierrez", -4, 0, 3),
            ("306", "Andres Castro", -1, 2, 2),
            ("401", "Laura Pena", -3, 1, 2),
            ("404", "Jorge Ruiz", -2, 3, 2),
            ("408", "Valentina Lagos", 0, 4, 2),
            ("502", "Familia Morales", -4, 3, 4),
            ("505", "Gustavo Vargas", 0, 2, 2),
            ("601", "Luna de Miel Diaz", -1, 5, 2),
        ];

        // Cuenta de la estancia principal de cada huésped: ahí se cargan sus consumos y tours (RN-07).
        var cuentas = new Dictionary<string, Cuenta>();
        var recepcionistas = new[] { mlopez, cruiz };
        var turno = 0;

        foreach (var estancia in estancias)
        {
            var habitacion = habitaciones[estancia.Numero];
            var huesped = huespedes[estancia.Huesped];
            var entrada = hoy.AddDays(estancia.Entrada);
            var checkIn = estancia.Entrada < 0 ? entrada.ToDateTime(new TimeOnly(15, 30)) : ahora.AddHours(-3);
            var fechaReserva = checkIn.AddDays(-10);

            var reserva = new ReservaHabitacion(huesped, habitacion, entrada, hoy.AddDays(estancia.Salida), estancia.Personas, fechaReserva);
            reserva.Confirmar(TipoPago.Tarjeta, RespaldoPago);
            habitacion.Ocupar(checkIn);
            reserva.RegistrarCheckIn(checkIn);

            var cuenta = new Cuenta(reserva, fechaReserva);
            db.ReservasHabitacion.Add(reserva);
            db.Cuentas.Add(cuenta);
            cuentas[estancia.Huesped] = cuenta;

            var recepcionista = recepcionistas[turno % recepcionistas.Length];
            turno++;
            db.Operaciones.Add(new RegistroOperacion(recepcionista.Id, TipoOperacion.CheckIn, $"Check-in Hab. {habitacion.Numero} - {huesped.Nombre}", checkIn));
        }

        // Reservas confirmadas que aún no llegan. La 203 está en limpieza: su check-in se rechaza
        // hasta que housekeeping la marque disponible (RN-02) y encabeza el plan de limpieza (HU-11 Esc.5).
        // La 210 de Valentina entra mañana: cancelarla genera cargo (RN-09). La 602 es en 35 días: sin cargo.
        (string Numero, string Huesped, int Entrada, int Salida, int Personas)[] llegadas =
        [
            ("305", "Esperanza Rios", 0, 3, 2),
            ("501", "Familia Pedraza", 0, 5, 4),
            ("203", "Camila Duarte", 0, 2, 1),
            ("210", "Valentina Lagos", 1, 3, 1),
            ("602", "Valentina Lagos", 35, 40, 2),
        ];

        foreach (var llegada in llegadas)
        {
            var fechaReserva = ahora.AddDays(-7);
            var reserva = new ReservaHabitacion(huespedes[llegada.Huesped], habitaciones[llegada.Numero], hoy.AddDays(llegada.Entrada), hoy.AddDays(llegada.Salida), llegada.Personas, fechaReserva);
            reserva.Confirmar(TipoPago.Tarjeta, RespaldoPago);

            var cuenta = new Cuenta(reserva, fechaReserva);
            db.ReservasHabitacion.Add(reserva);
            db.Cuentas.Add(cuenta);
            cuentas.TryAdd(llegada.Huesped, cuenta);
        }

        // Tours. Tres se cruzan mañana en la mañana, lo que permite probar la RN-03.
        var playa = new Tour("Playa Volcanica", "Playa El Zafiro", hoy.AddDays(1), new TimeOnly(8, 0), new TimeOnly(12, 0), 15, 85000m);
        var natural = new Tour("Reserva Natural", "Reserva El Mangle", hoy.AddDays(1), new TimeOnly(7, 0), new TimeOnly(11, 0), 20, 70000m);
        var lancha = new Tour("Paseo en Lancha", "Bahia Cristal", hoy, new TimeOnly(15, 0), new TimeOnly(18, 0), 12, 95000m);
        var ciudad = new Tour("City Tour Costero", "Centro Historico", hoy.AddDays(2), new TimeOnly(9, 0), new TimeOnly(13, 0), 25, 55000m);
        var sierra = new Tour("Senderismo Sierra", "Parque Sierra Alta", hoy.AddDays(1), new TimeOnly(6, 0), new TimeOnly(12, 0), 10, 75000m);
        db.Tours.AddRange(playa, natural, lancha, ciudad, sierra);

        // Reservas de tour de los huéspedes: cada una bloquea sus cupos (RN-05) y se carga a su cuenta.
        (Tour Tour, string Huesped, int Personas)[] reservasTour =
        [
            (lancha, "Familia Ramos", 4), (lancha, "Familia Morales", 4), (lancha, "Luna de Miel Diaz", 2),
            (playa, "Familia Ramos", 4), (playa, "Maria Torres", 2), (playa, "Familia Morales", 4),
            (natural, "Jorge Ruiz", 2), (natural, "Luna de Miel Diaz", 2), (natural, "Gustavo Vargas", 2),
            (ciudad, "Familia Pedraza", 5), (ciudad, "Esperanza Rios", 2), (ciudad, "Familia Morales", 4),
            (ciudad, "Maria Torres", 2), (ciudad, "Luna de Miel Diaz", 2),
            (sierra, "Valentina Lagos", 2), (sierra, "Laura Pena", 2),
        ];

        foreach (var item in reservasTour)
        {
            item.Tour.BloquearCupos(item.Personas);
            db.ReservasTour.Add(new ReservaTour(item.Tour, huespedes[item.Huesped], cuentas[item.Huesped], item.Personas, TipoPago.Tarjeta, RespaldoPago, ahora.AddDays(-2)));
        }

        // Cupos bloqueados a mano por el coordinador (ventas en recepción). El paseo en lancha queda lleno.
        playa.BloquearCupos(2);
        natural.BloquearCupos(2);
        lancha.BloquearCupos(2);
        ciudad.BloquearCupos(3);
        sierra.BloquearCupos(1);

        playa.AsignarRecursos(guias["Alberto Cruz"], vehiculos["Van #001"]);
        lancha.AsignarRecursos(guias["Mario Rios"], vehiculos["Lancha #02"]);
        sierra.AsignarRecursos(guias["Elena Mora"], vehiculos["Van #002"]);

        // Consumos de servicios adicionales (HU-08). Dia es respecto a hoy.
        (string Huesped, string Servicio, int Cantidad, decimal Precio, int Dia, int Hora, int Minuto)[] consumos =
        [
            ("Maria Torres", "Restaurante", 1, 45000m, -1, 13, 20),
            ("Maria Torres", "Spa", 1, 80000m, 0, 10, 0),
            ("Andres Castro", "Lavanderia", 1, 25000m, 0, 9, 30),
            ("Valentina Lagos", "Bar", 1, 35000m, 0, 21, 0),
            ("Valentina Lagos", "Transporte Aeropuerto", 1, 60000m, 0, 22, 0),
            ("Carlos Mejia", "Restaurante", 1, 62000m, -3, 20, 15),
            ("Carlos Mejia", "Lavanderia", 1, 25000m, -2, 11, 0),
            ("Familia Gutierrez", "Restaurante", 1, 120000m, -2, 13, 40),
            ("Familia Gutierrez", "Spa", 2, 80000m, -1, 16, 0),
            ("Familia Morales", "Room Service", 1, 48000m, -1, 22, 10),
        ];

        foreach (var consumo in consumos)
        {
            var momento = Momento(hoy.AddDays(consumo.Dia), consumo.Hora, consumo.Minuto, ahora);
            cuentas[consumo.Huesped].RegistrarConsumo(servicios[consumo.Servicio], consumo.Cantidad, consumo.Precio, momento, pgomez.Id);
            db.Operaciones.Add(new RegistroOperacion(pgomez.Id, TipoOperacion.RegistroConsumo, $"{consumo.Servicio} - {consumo.Huesped}", momento));
        }

        // Bitácora de housekeeping y del coordinador, para que el reporte de personal tenga datos (HU-13).
        foreach (var numero in new[] { "101", "103", "104", "106", "108", "110" })
        {
            db.Operaciones.Add(new RegistroOperacion(amartinez.Id, TipoOperacion.CambioEstadoHabitacion, $"Hab. {numero}: Disponible", Momento(hoy, 9, 30, ahora)));
        }

        foreach (var numero in new[] { "109", "304", "509", "609" })
        {
            db.Operaciones.Add(new RegistroOperacion(amartinez.Id, TipoOperacion.Incidencia, $"Hab. {numero}: En mantenimiento", Momento(hoy, 6, 0, ahora)));
        }

        foreach (var tour in new[] { playa, lancha, sierra })
        {
            db.Operaciones.Add(new RegistroOperacion(ltorres.Id, TipoOperacion.AsignacionRecursos, $"{tour.Nombre}: recursos asignados", ahora.AddDays(-1)));
        }

        db.SaveChanges();
        transaccion.Commit();
    }

    private Empleado NuevoEmpleado(string nombre, string usuario, string clave, RolEmpleado rol, DateTime fechaAlta) =>
        new(nombre, $"{usuario}@hotel.com", usuario, hasher.Hash(clave), rol, fechaAlta);

    /// <summary>Fecha y hora indicadas, sin pasarse del momento actual (no se siembran datos en el futuro).</summary>
    private static DateTime Momento(DateOnly fecha, int hora, int minuto, DateTime ahora)
    {
        var momento = fecha.ToDateTime(new TimeOnly(hora, minuto));
        return momento > ahora ? ahora.AddMinutes(-5) : momento;
    }
}
