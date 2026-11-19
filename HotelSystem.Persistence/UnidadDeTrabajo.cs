using System.Data;
using HotelSystem.Application.Abstracciones;
using HotelSystem.Domain.Excepciones;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HotelSystem.Persistence;

public sealed class UnidadDeTrabajo(HotelDbContext db) : IUnidadDeTrabajo
{
    public const string MensajeConcurrencia =
        "Otra persona estaba haciendo una operacion sobre los mismos datos en ese momento. Intente de nuevo (RNF-07).";

    public void GuardarCambios() => db.SaveChanges();

    public void EnTransaccion(Action accion) =>
        EnTransaccion<object?>(() =>
        {
            accion();
            return null;
        });

    public T EnTransaccion<T>(Func<T> accion)
    {
        // Si ya hay una transacción abierta (por ejemplo, la de la fachada de cierre de cuenta),
        // la operación se une a ella en lugar de abrir otra.
        if (db.Database.CurrentTransaction is not null)
        {
            return accion();
        }

        // Serializable: nadie más puede insertar una reserva que se cruce mientras esta
        // transacción comprueba la disponibilidad y guarda (RN-01, RN-05, RNF-07).
        using var transaccion = db.Database.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            var resultado = accion();
            transaccion.Commit();
            return resultado;
        }
        catch (Exception ex)
        {
            try
            {
                transaccion.Rollback();
            }
            catch (Exception)
            {
                // Si la conexión se cayó, la base de datos ya descartó la transacción.
            }

            // Se descartan los cambios que quedaron en memoria para que el resto de la
            // petición no vea datos que nunca llegaron a la base.
            db.ChangeTracker.Clear();

            // Si dos transacciones serializables chocan, la base de datos cancela una de ellas.
            // Para el usuario es un mensaje normal, no un error del sistema.
            if (EsConflictoDeConcurrencia(ex))
            {
                throw new ReglaNegocioException(MensajeConcurrencia, "RNF-07");
            }

            throw;
        }
    }

    private static bool EsConflictoDeConcurrencia(Exception? ex)
    {
        for (var actual = ex; actual is not null; actual = actual.InnerException)
        {
            switch (actual)
            {
                case DbUpdateConcurrencyException:
                    return true;
                case SqlException sql when sql.Number is 1205 or 3960:   // interbloqueo / conflicto de instantánea
                    return true;
                case SqliteException sqlite when sqlite.SqliteErrorCode is 5 or 6: // SQLITE_BUSY / SQLITE_LOCKED
                    return true;
            }
        }

        return false;
    }
}
