using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Quokka.Models
{
    public class Mascota
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string Nombre { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string UsuarioId { get; set; }
        public int NivelEnergia { get; set; }
        public int NivelHambre { get; set; }
        public DateTime UltimaAlimentacion { get; set; }
        public DateTime UltimaAtencion { get; set; }
        public bool EstaViva { get; set; }
    }
}
