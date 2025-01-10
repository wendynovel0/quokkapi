using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Quokka.Models;
using Quokka.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Quokka.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MensajeEmergenciaController : ControllerBase
    {
        private readonly MongoService _mongoService;

        public MensajeEmergenciaController(MongoService mongoService)
        {
            _mongoService = mongoService;
        }

        // Crear un nuevo mensaje de emergencia
        [HttpPost]
        public async Task<IActionResult> CrearMensajeEmergencia([FromBody] MensajeEmergencia mensaje)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState); // Devuelve errores de validación

            // Obtener el ID del usuario autenticado desde el token
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (usuarioId == null)
            {
                return Unauthorized(new { message = "Usuario no autenticado." });
            }

            // Asignar el ID del usuario y la fecha de envío
            mensaje.UsuarioId = usuarioId;
            mensaje.FechaEnvio = DateTime.UtcNow;
            mensaje.Enviado = false; // Marcar como no enviado inicialmente

            // Validar IDs de contactos seleccionados
            if (mensaje.ContactosSeleccionados != null)
            {
                foreach (var id in mensaje.ContactosSeleccionados)
                {
                    if (!ObjectId.TryParse(id.ToString(), out _))
                    {
                        return BadRequest($"El ID de contacto '{id}' no es válido.");
                    }
                }
            }

            await _mongoService.MensajesEmergencia.InsertOneAsync(mensaje);
            return CreatedAtAction(nameof(ObtenerMensajeEmergenciaPorId), new { id = mensaje.Id }, mensaje);
        }

        // Obtener todos los mensajes de emergencia del usuario autenticado
        [HttpGet]
        public async Task<IActionResult> ObtenerMensajesEmergencia()
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (usuarioId == null)
            {
                return Unauthorized(new { message = "Usuario no autenticado." });
            }

            var filter = Builders<MensajeEmergencia>.Filter.Eq(m => m.UsuarioId, usuarioId);
            var mensajes = await _mongoService.MensajesEmergencia.Find(filter).ToListAsync();

            return Ok(mensajes);
        }

        // Obtener un mensaje de emergencia por ID
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerMensajeEmergenciaPorId(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return BadRequest("El ID proporcionado no es válido.");

            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (usuarioId == null)
            {
                return Unauthorized(new { message = "Usuario no autenticado." });
            }

            var filter = Builders<MensajeEmergencia>.Filter.And(
                Builders<MensajeEmergencia>.Filter.Eq(m => m.Id, objectId),
                Builders<MensajeEmergencia>.Filter.Eq(m => m.UsuarioId, usuarioId)
            );

            var mensaje = await _mongoService.MensajesEmergencia.Find(filter).FirstOrDefaultAsync();
            if (mensaje == null)
                return NotFound();

            return Ok(mensaje);
        }

        // Actualizar un mensaje de emergencia
[HttpPut("{id}")]
public async Task<IActionResult> ActualizarMensajeEmergencia(string id, [FromBody] MensajeEmergencia mensaje)
{
    // Validar si el ID proporcionado es válido
    if (!ObjectId.TryParse(id, out var objectId))
        return BadRequest("El ID proporcionado no es válido.");

    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    // Obtener el ID del usuario autenticado desde el token
    var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (usuarioId == null)
    {
        return Unauthorized(new { message = "Usuario no autenticado." });
    }

    // Crear el filtro para encontrar el mensaje de emergencia del usuario autenticado
    var filter = Builders<MensajeEmergencia>.Filter.And(
        Builders<MensajeEmergencia>.Filter.Eq(m => m.Id, objectId),
        Builders<MensajeEmergencia>.Filter.Eq(m => m.UsuarioId, usuarioId)
    );

    // Buscar el mensaje existente en la base de datos
    var mensajeExistente = await _mongoService.MensajesEmergencia.Find(filter).FirstOrDefaultAsync();
    if (mensajeExistente == null)
    {
        return NotFound(new { message = "Mensaje de emergencia no encontrado o no autorizado para actualizar." });
    }

    // Asegurarse de que el ID se mantenga al actualizar y que el UsuarioId no se sobrescriba
    mensaje.Id = mensajeExistente.Id;

    // Realizar la actualización
    var updateResult = await _mongoService.MensajesEmergencia.ReplaceOneAsync(filter, mensaje);
    
    if (updateResult.MatchedCount == 0)
    {
        return NotFound(new { message = "No se pudo actualizar el mensaje de emergencia." });
    }

    return NoContent(); // Respuesta sin contenido (200 OK)
}



        // Eliminar un mensaje de emergencia
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarMensajeEmergencia(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return BadRequest("El ID proporcionado no es válido.");

            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (usuarioId == null)
            {
                return Unauthorized(new { message = "Usuario no autenticado." });
            }

            var filter = Builders<MensajeEmergencia>.Filter.And(
                Builders<MensajeEmergencia>.Filter.Eq(m => m.Id, objectId),
                Builders<MensajeEmergencia>.Filter.Eq(m => m.UsuarioId, usuarioId)
            );

            var mensajeExistente = await _mongoService.MensajesEmergencia.Find(filter).FirstOrDefaultAsync();
            if (mensajeExistente == null)
                return NotFound();

            await _mongoService.MensajesEmergencia.DeleteOneAsync(filter);
            return NoContent();
        }
    }
}
