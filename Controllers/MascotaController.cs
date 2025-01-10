using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Quokka.Models;
using Quokka.Services;
using System.Security.Claims;
using System;
using System.Threading.Tasks;

namespace Quokka.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MascotaController : ControllerBase
    {
        private readonly MongoService _mongoService;

        public MascotaController(MongoService mongoService)
        {
            _mongoService = mongoService;
        }

        // Crear una nueva mascota asociada al usuario autenticado
        [HttpPost]
        public async Task<IActionResult> CrearMascota([FromBody] Mascota mascota)
        {
            var usuarioId = ObtenerUsuarioIdDesdeToken();
            if (usuarioId == null)
                return Unauthorized(new { message = "No se pudo determinar el usuario autenticado." });

            mascota.UsuarioId = usuarioId; // Asociar la mascota al usuario autenticado
            mascota.UltimaAlimentacion = DateTime.UtcNow; // Fecha inicial
            mascota.UltimaAtencion = DateTime.UtcNow; // Fecha inicial
            mascota.EstaViva = true; // La mascota inicia como viva
            await _mongoService.Mascotas.InsertOneAsync(mascota);

            return CreatedAtAction(nameof(ObtenerMascotaPorId), new { id = mascota.Id }, mascota);
        }

        // Obtener todas las mascotas del usuario autenticado
        [HttpGet]
        public async Task<IActionResult> ObtenerMascotas()
        {
            var usuarioId = ObtenerUsuarioIdDesdeToken();
            if (usuarioId == null)
                return Unauthorized(new { message = "No se pudo determinar el usuario autenticado." });

            var filter = Builders<Mascota>.Filter.Eq(m => m.UsuarioId, usuarioId);
            var mascotas = await _mongoService.Mascotas.Find(filter).ToListAsync();
            return Ok(mascotas);
        }

        // Obtener una mascota específica del usuario autenticado
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerMascotaPorId(string id)
        {
            var usuarioId = ObtenerUsuarioIdDesdeToken();
            if (usuarioId == null)
                return Unauthorized(new { message = "No se pudo determinar el usuario autenticado." });

            var filter = Builders<Mascota>.Filter.And(
                Builders<Mascota>.Filter.Eq(m => m.Id, ObjectId.Parse(id)),
                Builders<Mascota>.Filter.Eq(m => m.UsuarioId, usuarioId)
            );

            var mascota = await _mongoService.Mascotas.Find(filter).FirstOrDefaultAsync();
            if (mascota == null)
                return NotFound();
            
            return Ok(mascota);
        }

        // Actualizar una mascota específica del usuario autenticado
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarMascota(string id, [FromBody] Mascota mascota)
        {
            var usuarioId = ObtenerUsuarioIdDesdeToken();
            if (usuarioId == null)
                return Unauthorized(new { message = "No se pudo determinar el usuario autenticado." });

            var filter = Builders<Mascota>.Filter.And(
                Builders<Mascota>.Filter.Eq(m => m.Id, ObjectId.Parse(id)),
                Builders<Mascota>.Filter.Eq(m => m.UsuarioId, usuarioId)
            );

            var mascotaExistente = await _mongoService.Mascotas.Find(filter).FirstOrDefaultAsync();
            if (mascotaExistente == null)
                return NotFound();

            mascota.Id = mascotaExistente.Id; // Mantener el mismo ID
            mascota.UsuarioId = usuarioId; // Asegurar que sigue asociada al usuario
            await _mongoService.Mascotas.ReplaceOneAsync(filter, mascota);

            return NoContent();
        }

        // Eliminar una mascota específica del usuario autenticado
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarMascota(string id)
        {
            var usuarioId = ObtenerUsuarioIdDesdeToken();
            if (usuarioId == null)
                return Unauthorized(new { message = "No se pudo determinar el usuario autenticado." });

            var filter = Builders<Mascota>.Filter.And(
                Builders<Mascota>.Filter.Eq(m => m.Id, ObjectId.Parse(id)),
                Builders<Mascota>.Filter.Eq(m => m.UsuarioId, usuarioId)
            );

            var mascotaExistente = await _mongoService.Mascotas.Find(filter).FirstOrDefaultAsync();
            if (mascotaExistente == null)
                return NotFound();

            await _mongoService.Mascotas.DeleteOneAsync(filter);
            return NoContent();
        }

        // Método auxiliar para obtener el usuario autenticado desde el token JWT
        private string ObtenerUsuarioIdDesdeToken()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
