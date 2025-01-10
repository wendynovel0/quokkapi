using MongoDB.Driver;
using Microsoft.Extensions.Options;
using Quokka.Models;

namespace Quokka.Services
{
    public class MongoService
    {
        private readonly IMongoDatabase _database;
        public MongoService(IOptions<MongoSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
           _database = client.GetDatabase(settings.Value.DatabaseName);
        }

      public IMongoCollection<ConfiguracionEmergencia> ConfiguracionesEmergencia => _database.GetCollection<ConfiguracionEmergencia>("ConfiguracionesEmergencia");
      public IMongoCollection<AlertaEmergencia> AlertasEmergencias => _database.GetCollection<AlertaEmergencia>("AlertasEmergencias");
      public IMongoCollection<Usuario> Usuarios => _database.GetCollection<Usuario>("Usuarios");
      public IMongoCollection<MensajeEmergencia> MensajesEmergencia => _database.GetCollection<MensajeEmergencia>("MensajesEmergencia");
      public IMongoCollection<Mascota> Mascotas => _database.GetCollection<Mascota>("Mascotas");
      public IMongoCollection<ContactoEmergencia> ContactosEmergencia => _database.GetCollection<ContactoEmergencia>("ContactosEmergencia");

    }
}