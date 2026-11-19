using HotelSystem.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace HotelSystem.Persistence;

/// <summary>
/// Contexto de Entity Framework Core: traduce las entidades del dominio a tablas.
/// El dominio no sabe nada de la base de datos; todo el mapeo está aquí.
/// </summary>
public class HotelDbContext(DbContextOptions<HotelDbContext> options) : DbContext(options)
{
    public DbSet<Persona> Personas => Set<Persona>();

    public DbSet<Huesped> Huespedes => Set<Huesped>();

    public DbSet<Empleado> Empleados => Set<Empleado>();

    public DbSet<Habitacion> Habitaciones => Set<Habitacion>();

    public DbSet<ReservaHabitacion> ReservasHabitacion => Set<ReservaHabitacion>();

    public DbSet<Cuenta> Cuentas => Set<Cuenta>();

    public DbSet<ConsumoServicio> Consumos => Set<ConsumoServicio>();

    public DbSet<ServicioAdicional> Servicios => Set<ServicioAdicional>();

    public DbSet<SolicitudServicio> Solicitudes => Set<SolicitudServicio>();

    public DbSet<Tour> Tours => Set<Tour>();

    public DbSet<Guia> Guias => Set<Guia>();

    public DbSet<Vehiculo> Vehiculos => Set<Vehiculo>();

    public DbSet<ReservaTour> ReservasTour => Set<ReservaTour>();

    public DbSet<Prefactura> Prefacturas => Set<Prefactura>();

    public DbSet<RegistroOperacion> Operaciones => Set<RegistroOperacion>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Valores por defecto para todo el modelo: dinero con dos decimales y textos acotados.
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
        configurationBuilder.Properties<string>().HaveMaxLength(200);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Persona, Huesped y Empleado comparten una sola tabla con una columna que indica el tipo.
        modelBuilder.Entity<Persona>(b =>
        {
            b.ToTable("Personas");
            b.HasDiscriminator<string>("TipoPersona")
                .HasValue<Huesped>("Huesped")
                .HasValue<Empleado>("Empleado");
            b.HasIndex(p => p.Usuario).IsUnique();
            b.HasIndex(p => p.Correo).IsUnique();
            b.Ignore(p => p.RolNombre);
        });

        // Los enums se guardan como texto para que las tablas se puedan leer a simple vista.
        modelBuilder.Entity<Empleado>(b =>
        {
            b.Property(e => e.Rol).HasConversion<string>().HasMaxLength(30);
        });

        modelBuilder.Entity<Habitacion>(b =>
        {
            b.ToTable("Habitaciones");
            b.Property(h => h.Numero).HasMaxLength(10);
            b.HasIndex(h => h.Numero).IsUnique();
            b.Property(h => h.Tipo).HasConversion<string>().HasMaxLength(20);
            b.Property(h => h.Estado).HasConversion<string>().HasMaxLength(20);
            b.Property(h => h.NotaMantenimiento).HasMaxLength(500);
            b.Ignore(h => h.EstadoActual);
        });

        modelBuilder.Entity<ReservaHabitacion>(b =>
        {
            b.ToTable("ReservasHabitacion");
            b.Property(r => r.Estado).HasConversion<string>().HasMaxLength(20);
            b.Property(r => r.MedioPago).HasConversion<string>().HasMaxLength(20);
            b.HasOne(r => r.Huesped).WithMany().HasForeignKey(r => r.HuespedId);
            b.HasOne(r => r.Habitacion).WithMany().HasForeignKey(r => r.HabitacionId);
            b.HasIndex(r => new { r.HabitacionId, r.FechaEntrada, r.FechaSalida });
            b.Ignore(r => r.Codigo);
            b.Ignore(r => r.Noches);
            b.Ignore(r => r.ValorTotalAlojamiento);
            b.Ignore(r => r.EstaActiva);
        });

        modelBuilder.Entity<Cuenta>(b =>
        {
            b.ToTable("Cuentas");
            b.Property(c => c.Estado).HasConversion<string>().HasMaxLength(20);
            b.Property(c => c.MedioPago).HasConversion<string>().HasMaxLength(20);

            // RN-07: una reserva tiene exactamente una cuenta.
            b.HasOne(c => c.Reserva).WithOne(r => r.Cuenta).HasForeignKey<Cuenta>(c => c.ReservaHabitacionId);
            b.HasMany(c => c.Consumos).WithOne(x => x.Cuenta).HasForeignKey(x => x.CuentaId);
            b.HasMany(c => c.ReservasTour).WithOne(t => t.Cuenta).HasForeignKey(t => t.CuentaId);

            b.Ignore(c => c.EstaAbierta);
            b.Ignore(c => c.EstaPagada);
            b.Ignore(c => c.TotalAlojamiento);
            b.Ignore(c => c.TotalServicios);
            b.Ignore(c => c.TotalTours);
        });

        modelBuilder.Entity<ConsumoServicio>(b =>
        {
            b.ToTable("Consumos");
            b.HasOne(c => c.Servicio).WithMany().HasForeignKey(c => c.ServicioAdicionalId);
            b.HasOne<Empleado>().WithMany().HasForeignKey(c => c.RegistradoPorId);
            b.Ignore(c => c.Subtotal);
        });

        modelBuilder.Entity<ServicioAdicional>(b =>
        {
            b.ToTable("ServiciosAdicionales");
            b.HasIndex(s => s.Nombre).IsUnique();
            b.Property(s => s.Descripcion).HasMaxLength(300);
            b.Ignore(s => s.TienePrecioFijo);
        });

        modelBuilder.Entity<SolicitudServicio>(b =>
        {
            b.ToTable("SolicitudesServicio");
            b.Property(s => s.Estado).HasConversion<string>().HasMaxLength(20);
            b.HasOne(s => s.Cuenta).WithMany().HasForeignKey(s => s.CuentaId);
            b.HasOne(s => s.Servicio).WithMany().HasForeignKey(s => s.ServicioAdicionalId);
        });

        modelBuilder.Entity<Guia>(b => b.ToTable("Guias"));

        modelBuilder.Entity<Vehiculo>(b => b.ToTable("Vehiculos"));

        modelBuilder.Entity<Tour>(b =>
        {
            b.ToTable("Tours");
            b.HasOne(t => t.Guia).WithMany().HasForeignKey(t => t.GuiaId);
            b.HasOne(t => t.Vehiculo).WithMany().HasForeignKey(t => t.VehiculoId);
            b.Ignore(t => t.CuposDisponibles);
            b.Ignore(t => t.EstaLleno);
            b.Ignore(t => t.Inicio);
        });

        modelBuilder.Entity<ReservaTour>(b =>
        {
            b.ToTable("ReservasTour");
            b.Property(r => r.Estado).HasConversion<string>().HasMaxLength(20);
            b.Property(r => r.MedioPago).HasConversion<string>().HasMaxLength(20);
            b.HasOne(r => r.Tour).WithMany().HasForeignKey(r => r.TourId);
            b.HasOne(r => r.Huesped).WithMany().HasForeignKey(r => r.HuespedId);
            b.Ignore(r => r.Codigo);
            b.Ignore(r => r.EstaVigente);
        });

        modelBuilder.Entity<Prefactura>(b =>
        {
            b.ToTable("Prefacturas");
            b.HasOne(p => p.Cuenta).WithMany().HasForeignKey(p => p.CuentaId);
        });

        modelBuilder.Entity<RegistroOperacion>(b =>
        {
            b.ToTable("Operaciones");
            b.Property(o => o.Tipo).HasConversion<string>().HasMaxLength(40);
            b.Property(o => o.Detalle).HasMaxLength(300);
            b.HasOne<Empleado>().WithMany().HasForeignKey(o => o.EmpleadoId);
            b.HasIndex(o => o.FechaHora);
        });

        // Ninguna relación borra en cascada: el sistema no elimina registros (se cancelan o se
        // desactivan), y SQL Server rechaza los modelos con varias rutas de borrado en cascada.
        foreach (var relacion in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            relacion.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }
}
