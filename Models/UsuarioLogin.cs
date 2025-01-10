using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
namespace Quokka.Models
{
    public class UsuarioLogin
    {
        public string Correo { get; set; }
        public string Contrasena { get; set; }
    }
}
