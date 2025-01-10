using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Quokka.Models
{
    public class AlertaEmergencia
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string TipoAlerta { get; set; } 
        public DateTime FechaActivacion { get; set; } 
        public DateTime? FechaResolucion { get; set; } 
        public string Estado { get; set; } 
        public string Localizacion { get; set; } 
        
        [BsonRepresentation(BsonType.ObjectId)]
        public string UsuarioId { get; set; } 
    }
}
