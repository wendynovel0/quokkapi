using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Quokka.Models;
using Quokka.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Quokka.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ContactoEmergenciaController : ControllerBase
    {
        private readonly MongoService _mongoService;

        public ContactoEmergenciaController(MongoService mongoService)
        {
            _mongoService = mongoService;
        }

        // Crear un nuevo contacto de emergencia vinculado al usuario autenticado
        [HttpPost]
        public async Task<IActionResult> CrearContactoEmergencia([FromBody] ContactoEmergencia contacto)
        {
            // Obtener el ID del usuario autenticado desde el token
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (usuarioId == null)
            {
                return Unauthorized(new { message = "Usuario no autenticado." });
            }

            // Vincular el contacto con el usuario autenticado
            contacto.UsuarioId = usuarioId;

            await _mongoService.ContactosEmergencia.InsertOneAsync(contacto);
            return CreatedAtAction(nameof(ObtenerContactoEmergenciaPorId), new { id = contacto.Id }, contacto);
        }

        // Obtener todos los contactos de emergencia del usuario autenticado
        [HttpGet]
        public async Task<IActionResult> ObtenerContactosEmergencia()
        {
            // Obtener el ID del usuario autenticado desde el token
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (usuarioId == null)
            {
                return Unauthorized(new { message = "Usuario no autenticado." });
            }

            // Filtrar contactos por UsuarioId
            var filter = Builders<ContactoEmergencia>.Filter.Eq(c => c.UsuarioId, usuarioId);
            var contactos = await _mongoService.ContactosEmergencia.Find(filter).ToListAsync();

            return Ok(contactos);
        }

        // Obtener un contacto de emergencia por ID (validando que pertenezca al usuario autenticado)
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerContactoEmergenciaPorId(string id)
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (usuarioId == null)
            {
                return Unauthorized(new { message = "Usuario no autenticado." });
            }

            var filter = Builders<ContactoEmergencia>.Filter.And(
                Builders<ContactoEmergencia>.Filter.Eq(c => c.Id, ObjectId.Parse(id)),
                Builders<ContactoEmergencia>.Filter.Eq(c => c.UsuarioId, usuarioId)
            );

            var contacto = await _mongoService.ContactosEmergencia.Find(filter).FirstOrDefaultAsync();
            if (contacto == null)
                return NotFound();

            return Ok(contacto);
        }

        // Actualizar un contacto de emergencia
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarContactoEmergencia(string id, [FromBody] ContactoEmergencia contacto)
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (usuarioId == null)
            {
                return Unauthorized(new { message = "Usuario no autenticado." });
            }

            var filter = Builders<ContactoEmergencia>.Filter.And(
                Builders<ContactoEmergencia>.Filter.Eq(c => c.Id, ObjectId.Parse(id)),
                Builders<ContactoEmergencia>.Filter.Eq(c => c.UsuarioId, usuarioId)
            );

            var contactoExistente = await _mongoService.ContactosEmergencia.Find(filter).FirstOrDefaultAsync();
            if (contactoExistente == null)
                return NotFound();

            contacto.Id = contactoExistente.Id; // Mantenemos el mismo ID para la actualización
            contacto.UsuarioId = usuarioId; // Aseguramos que el UsuarioId no cambie

            await _mongoService.ContactosEmergencia.ReplaceOneAsync(filter, contacto);
            return NoContent();
        }

        // Eliminar un contacto de emergencia
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarContactoEmergencia(string id)
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (usuarioId == null)
            {
                return Unauthorized(new { message = "Usuario no autenticado." });
            }

            var filter = Builders<ContactoEmergencia>.Filter.And(
                Builders<ContactoEmergencia>.Filter.Eq(c => c.Id, ObjectId.Parse(id)),
                Builders<ContactoEmergencia>.Filter.Eq(c => c.UsuarioId, usuarioId)
            );

            var contactoExistente = await _mongoService.ContactosEmergencia.Find(filter).FirstOrDefaultAsync();
            if (contactoExistente == null)
                return NotFound();

            await _mongoService.ContactosEmergencia.DeleteOneAsync(filter);
            return NoContent();
        }
    }
}
