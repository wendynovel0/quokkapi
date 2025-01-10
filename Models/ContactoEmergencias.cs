using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Quokka.Models
{
    public class ContactoEmergencia
    {
        [BsonId]
        public ObjectId Id { get; set; }  // El ID de MongoDB

        public string Nombre { get; set; }   // Nombre del contacto de emergencia
        public string Telefono { get; set; } // Número de teléfono
        public string Correo { get; set; }   // Correo electrónico
        public string Relacion { get; set; } // Relación con el usuario
        [BsonRepresentation(BsonType.ObjectId)]
        public string UsuarioId { get; set; } 
    }
}
