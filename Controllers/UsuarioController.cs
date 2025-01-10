using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Quokka.Models;  // Asegúrate de que el espacio de nombres sea correcto
using Quokka.Services;  // Asegúrate de que el espacio de nombres sea correcto
using Microsoft.AspNetCore.Authorization; // Asegúrate de tener este namespace

namespace Quokka.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly MongoService _mongoService;

        public UsuarioController(MongoService mongoService)
        {
            _mongoService = mongoService;
        }

        // Crear un nuevo usuario - No requiere autenticación (sin token)
        [AllowAnonymous]  // Permite que esta ruta no requiera autenticación
        [HttpPost]
        public async Task<IActionResult> CrearUsuario([FromBody] Usuario usuario)
        {
            // Verificar si el usuario ya existe
            var existingUser = await _mongoService.Usuarios
                                                  .Find(u => u.Correo == usuario.Correo)
                                                  .FirstOrDefaultAsync();

            if (existingUser != null)
            {
                return Conflict("El correo electrónico ya está en uso.");
            }

            // Insertar el nuevo usuario
            await _mongoService.Usuarios.InsertOneAsync(usuario);

            return CreatedAtAction(nameof(CrearUsuario), new { id = usuario.Id }, usuario);
        }

        // Obtener todos los usuarios - Requiere autenticación
        [Authorize] // Requiere que el usuario esté autenticado con un token
        [HttpGet]
        public async Task<IActionResult> GetUsuarios()
        {
            var usuarios = await _mongoService.Usuarios.Find(new BsonDocument()).ToListAsync();
            return Ok(usuarios);
        }

        // Obtener un usuario por su ID - Requiere autenticación
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUsuarioById(string id)
        {
            var usuario = await _mongoService.Usuarios
                                              .Find(u => u.Id == new ObjectId(id))
                                              .FirstOrDefaultAsync();

            if (usuario == null)
            {
                return NotFound($"No se encontró el usuario con ID {id}");
            }

            return Ok(usuario);
        }

        // Actualizar un usuario existente - Requiere autenticación
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarUsuario(string id, [FromBody] Usuario usuario)
        {
            var updateDefinition = Builders<Usuario>.Update
                .Set(u => u.Nombre, usuario.Nombre)
                .Set(u => u.Correo, usuario.Correo)
                .Set(u => u.Telefono, usuario.Telefono)
                .Set(u => u.Contrasena, usuario.Contrasena);

            var result = await _mongoService.Usuarios
                                            .UpdateOneAsync(u => u.Id == new ObjectId(id), updateDefinition);

            if (result.ModifiedCount == 0)
            {
                return NotFound($"No se encontró el usuario con ID {id}");
            }

            return NoContent();  // Indica que la actualización fue exitosa
        }

        // Eliminar un usuario - Requiere autenticación
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarUsuario(string id)
        {
            var result = await _mongoService.Usuarios
                                            .DeleteOneAsync(u => u.Id == new ObjectId(id));

            if (result.DeletedCount == 0)
            {
                return NotFound($"No se encontró el usuario con ID {id}");
            }

            return NoContent();  // Indica que la eliminación fue exitosa
        }
    }
}
