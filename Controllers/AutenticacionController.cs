using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Quokka.Models;
using Quokka.Services;
using System.Text.RegularExpressions;

namespace Quokka.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutenticacionController : ControllerBase
    {
        private readonly IMongoCollection<Usuario> _usuarios;
        private readonly JwtService _jwtService;

        public AutenticacionController(IMongoDatabase database, JwtService jwtService)
        {
            _usuarios = database.GetCollection<Usuario>("Usuarios");
            _jwtService = jwtService;
        }

        // Registro de usuario
        [AllowAnonymous]
        [HttpPost("registro")]
        public IActionResult Registro([FromBody] Usuario nuevoUsuario)
        {
            // Validar si el correo tiene el formato correcto
            if (!Regex.IsMatch(nuevoUsuario.Correo, @"^[^@]+@[^@]+\.[^@]+$"))
            {
                return BadRequest(new { message = "El correo no tiene un formato válido." });
            }

            // Validar si el correo ya está registrado
            var usuarioExistente = _usuarios.Find(u => u.Correo == nuevoUsuario.Correo).FirstOrDefault();
            if (usuarioExistente != null)
            {
                return BadRequest(new { message = "El correo ya está registrado." });
            }

            // Validar campos obligatorios
            if (string.IsNullOrWhiteSpace(nuevoUsuario.Nombre) || 
                string.IsNullOrWhiteSpace(nuevoUsuario.Correo) || 
                string.IsNullOrWhiteSpace(nuevoUsuario.Contrasena))
            {
                return BadRequest(new { message = "Todos los campos son obligatorios." });
            }

            // Hashear la contraseña
            nuevoUsuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(nuevoUsuario.Contrasena);

            // Insertar el nuevo usuario en la base de datos
            _usuarios.InsertOne(nuevoUsuario);

            return Ok(new { message = "Usuario registrado exitosamente." });
        }

        // Login de usuario
        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] UsuarioLogin usuarioLogin)
        {
            // Buscar el usuario por correo
            var usuario = _usuarios.Find(u => u.Correo == usuarioLogin.Correo).FirstOrDefault();

            if (usuario == null)
            {
                return Unauthorized(new { message = "Correo o contraseña incorrectos." });
            }

            // Verificar contraseña
            if (BCrypt.Net.BCrypt.Verify(usuarioLogin.Contrasena, usuario.Contrasena))
            {
                var token = _jwtService.GenerateToken(usuario.Id.ToString());
                return Ok(new { token });
            }

            return Unauthorized(new { message = "Correo o contraseña incorrectos." });
        }
    }
}
