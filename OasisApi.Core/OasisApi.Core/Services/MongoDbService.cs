using MongoDB.Driver;
using MongoDB.Bson;

namespace OasisApi.Core.Services
{
    public class MongoDbService
    {
        private readonly IMongoCollection<BsonDocument> _collection;

        // O construtor pega as configurações do appsettings.json
        public MongoDbService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("MongoDbConnection");
            var databaseName = configuration["MongoDbSettings:DatabaseName"];
            var collectionName = configuration["MongoDbSettings:CollectionName"];

            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);

            _collection = database.GetCollection<BsonDocument>(collectionName);
        }

        // Método que importa o JSON vindo do Oracle
        public async Task ImportarJsonAsync(string jsonDataset)
        {
            // 1. Limpa a coleção (bom para testes)
            await _collection.DeleteManyAsync(Builders<BsonDocument>.Filter.Empty);

            // 2. Converte a string JSON em um documento BSON
            var bsonDocument = BsonDocument.Parse(jsonDataset);

            // 3. Pega o array "usuarios" de dentro do JSON
            var usuariosArray = bsonDocument["usuarios"].AsBsonArray;

            if (usuariosArray.Any())
            {
                // 4. Converte o array em uma lista de BsonDocument
                var documentos = usuariosArray.Select(u => u.AsBsonDocument);

                // 5. Insere TODOS os documentos no MongoDB
                await _collection.InsertManyAsync(documentos);
            }
        }
    }
}