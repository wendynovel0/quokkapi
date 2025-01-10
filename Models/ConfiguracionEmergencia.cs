using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Quokka.Models
{
    public class ConfiguracionEmergencia
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public bool NotificarAlerta { get; set; }  // Indica si se notificará la alerta
        public bool ActivarZonaSegura { get; set; } // Indica si se activará la zona segura
        public string MensajeAlerta { get; set; }   // Mensaje de alerta predeterminado
        public DateTime FechaUltimaActualizacion { get; set; }  // Fecha de la última actualización
        
        [BsonRepresentation(BsonType.ObjectId)]
        public string UsuarioId { get; set; }  // ID del usuario responsable
    }
}
