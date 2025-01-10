using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Quokka.Models;
using Quokka.Services;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Quokka.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ConfiguracionEmergenciaController : ControllerBase
    {
        private readonly MongoService _mongoService;

        public ConfiguracionEmergenciaController(MongoService mongoService)
        {
            _mongoService = mongoService;
        }

        // Obtener el ID del usuario desde el token de autenticación
        private string ObtenerUsuarioId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier); // O el tipo de claim que uses para el ID del usuario
        }

        // Crear una nueva configuración de emergencia
        [HttpPost]
        public async Task<IActionResult> CrearConfiguracionEmergencia([FromBody] ConfiguracionEmergencia configuracionEmergencia)
        {
            configuracionEmergencia.FechaUltimaActualizacion = DateTime.UtcNow; // Asignar la fecha actual
            configuracionEmergencia.UsuarioId = ObtenerUsuarioId(); // Asignar el ID del usuario actual

            await _mongoService.ConfiguracionesEmergencia.InsertOneAsync(configuracionEmergencia);
            return CreatedAtAction(nameof(ObtenerConfiguracionEmergenciaPorId), new { id = configuracionEmergencia.Id }, configuracionEmergencia);
        }

        // Obtener todas las configuraciones de emergencia
        // Obtener todas las configuraciones de emergencia del usuario autenticado
[HttpGet]
public async Task<IActionResult> ObtenerConfiguracionesEmergencia()
{
    var usuarioId = ObtenerUsuarioId();
    if (usuarioId == null)
        return Unauthorized(new { message = "No se pudo determinar el usuario autenticado." });

    var filter = Builders<ConfiguracionEmergencia>.Filter.Eq(c => c.UsuarioId, usuarioId);
    var configuraciones = await _mongoService.ConfiguracionesEmergencia.Find(filter).ToListAsync();
    return Ok(configuraciones);
}

// Obtener una configuración de emergencia por ID solo si pertenece al usuario autenticado
[HttpGet("{id}")]
public async Task<IActionResult> ObtenerConfiguracionEmergenciaPorId(string id)
{
    var usuarioId = ObtenerUsuarioId();
    if (usuarioId == null)
        return Unauthorized(new { message = "No se pudo determinar el usuario autenticado." });

    var filter = Builders<ConfiguracionEmergencia>.Filter.And(
        Builders<ConfiguracionEmergencia>.Filter.Eq(c => c.Id, ObjectId.Parse(id)),
        Builders<ConfiguracionEmergencia>.Filter.Eq(c => c.UsuarioId, usuarioId)
    );

    var configuracionEmergencia = await _mongoService.ConfiguracionesEmergencia.Find(filter).FirstOrDefaultAsync();
    if (configuracionEmergencia == null)
        return NotFound();
    return Ok(configuracionEmergencia);
}


        // Actualizar una configuración de emergencia
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarConfiguracionEmergencia(string id, [FromBody] ConfiguracionEmergencia configuracionEmergencia)
        {
            var filter = Builders<ConfiguracionEmergencia>.Filter.Eq(c => c.Id, ObjectId.Parse(id));
            var configuracionExistente = await _mongoService.ConfiguracionesEmergencia.Find(filter).FirstOrDefaultAsync();
            if (configuracionExistente == null)
                return NotFound();

            configuracionEmergencia.Id = configuracionExistente.Id; // Mantener el mismo ID para la actualización
            configuracionEmergencia.FechaUltimaActualizacion = DateTime.UtcNow; // Actualizar la fecha
            configuracionEmergencia.UsuarioId = configuracionExistente.UsuarioId; // No cambiar el ID de usuario

            await _mongoService.ConfiguracionesEmergencia.ReplaceOneAsync(filter, configuracionEmergencia);
            return NoContent(); // Respuesta sin contenido (200 OK)
        }

        // Eliminar una configuración de emergencia
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarConfiguracionEmergencia(string id)
        {
            var filter = Builders<ConfiguracionEmergencia>.Filter.Eq(c => c.Id, ObjectId.Parse(id));
            var configuracionExistente = await _mongoService.ConfiguracionesEmergencia.Find(filter).FirstOrDefaultAsync();
            if (configuracionExistente == null)
                return NotFound();

            await _mongoService.ConfiguracionesEmergencia.DeleteOneAsync(filter);
            return NoContent(); // Respuesta sin contenido (200 OK)
        }
    }
}
