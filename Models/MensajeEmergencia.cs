using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Quokka.Models
{
    public class MensajeEmergencia
    {
        [BsonId]
        public ObjectId Id { get; set; }  // El ID de MongoDB

        public string Titulo { get; set; } // Título del mensaje
        public string Contenido { get; set; } // Contenido del mensaje
        public DateTime FechaEnvio { get; set; } // Fecha de envío del mensaje
        public bool Enviado { get; set; } // Estado de envío (enviado/no enviado)
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> ContactosSeleccionados { get; set; } // IDs de contactos seleccionados
        [BsonRepresentation(BsonType.ObjectId)]
        public string UsuarioId { get; set; } // ID del usuario que creó el mensaje
    }
}
