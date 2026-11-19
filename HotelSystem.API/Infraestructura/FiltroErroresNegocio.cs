using HotelSystem.API.Contratos;
using HotelSystem.Domain.Excepciones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HotelSystem.API.Infraestructura;

/// <summary>
/// Convierte las reglas de negocio incumplidas en respuestas 4xx con el cuerpo
/// <c>{ "mensaje": "...", "regla": "RN-01" }</c>, que el front muestra tal cual.
/// Asi los controladores no necesitan try/catch: solo llaman al servicio.
/// </summary>
public sealed class FiltroErroresNegocio : IExceptionFilter
{
    public void OnException(ExceptionContext contexto)
    {
        if (contexto.Exception is not ReglaNegocioException ex) return;

        var estado = ex is EntidadNoEncontradaException
            ? StatusCodes.Status404NotFound
            : StatusCodes.Status400BadRequest;

        contexto.Result = new ObjectResult(new RespuestaError(ex.Message, ex.Regla)) { StatusCode = estado };
        contexto.ExceptionHandled = true;
    }
}
