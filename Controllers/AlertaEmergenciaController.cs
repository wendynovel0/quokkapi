using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Quokka.Models;
using Quokka.Services;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Quokka.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AlertaEmergenciaController : ControllerBase
    {
        private readonly MongoService _mongoService;

        public AlertaEmergenciaController(MongoService mongoService)
        {
            _mongoService = mongoService;
        }

        // Método auxiliar para obtener el ID del usuario desde el token de autenticación
        private string ObtenerUsuarioIdDesdeToken()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        // Crear una nueva alerta de emergencia
        [HttpPost]
        public async Task<IActionResult> CrearAlertaEmergencia([FromBody] AlertaEmergencia alertaEmergencia)
        {
            var usuarioId = ObtenerUsuarioIdDesdeToken();
            if (usuarioId == null)
                return Unauthorized(new { message = "No se pudo determinar el usuario autenticado." });

            alertaEmergencia.FechaActivacion = DateTime.UtcNow; // Asignar la fecha de activación de la alerta
            alertaEmergencia.UsuarioId = usuarioId; // Asignar el ID del usuario

            await _mongoService.AlertasEmergencias.InsertOneAsync(alertaEmergencia);
            return CreatedAtAction(nameof(ObtenerAlertaEmergenciaPorId), new { id = alertaEmergencia.Id }, alertaEmergencia);
        }

        // Obtener todas las alertas de emergencia
        [HttpGet]
        public async Task<IActionResult> ObtenerAlertasEmergencias()
        {
            var alertasEmergencias = await _mongoService.AlertasEmergencias.Find(new BsonDocument()).ToListAsync();
            return Ok(alertasEmergencias);
        }

        // Obtener una alerta de emergencia por ID
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerAlertaEmergenciaPorId(string id)
        {
            var filter = Builders<AlertaEmergencia>.Filter.Eq(a => a.Id, ObjectId.Parse(id));
            var alertaEmergencia = await _mongoService.AlertasEmergencias.Find(filter).FirstOrDefaultAsync();
            if (alertaEmergencia == null)
                return NotFound();
            return Ok(alertaEmergencia);
        }

        // Actualizar una alerta de emergencia
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarAlertaEmergencia(string id, [FromBody] AlertaEmergencia alertaEmergencia)
        {
            var filter = Builders<AlertaEmergencia>.Filter.Eq(a => a.Id, ObjectId.Parse(id));
            var alertaExistente = await _mongoService.AlertasEmergencias.Find(filter).FirstOrDefaultAsync();
            if (alertaExistente == null)
                return NotFound();

            alertaEmergencia.Id = alertaExistente.Id; // Mantener el mismo ID para la actualización
            alertaEmergencia.UsuarioId = alertaExistente.UsuarioId; // Mantener el ID de usuario existente

            await _mongoService.AlertasEmergencias.ReplaceOneAsync(filter, alertaEmergencia);
            return NoContent(); // Respuesta sin contenido (200 OK)
        }

        // Eliminar una alerta de emergencia
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarAlertaEmergencia(string id)
        {
            var filter = Builders<AlertaEmergencia>.Filter.Eq(a => a.Id, ObjectId.Parse(id));
            var alertaExistente = await _mongoService.AlertasEmergencias.Find(filter).FirstOrDefaultAsync();
            if (alertaExistente == null)
                return NotFound();

            await _mongoService.AlertasEmergencias.DeleteOneAsync(filter);
            return NoContent(); // Respuesta sin contenido (200 OK)
        }
    }
}
