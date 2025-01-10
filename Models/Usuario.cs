using MongoDB.Bson; 
using MongoDB.Bson.Serialization.Attributes;  

namespace Quokka.Models
{
    public class Usuario
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public string Contrasena { get; set; }
    }
}
