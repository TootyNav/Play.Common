using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Play.Common.Settings;

namespace Play.Common.Repositories;

public static class Extensions
{
    public static IServiceCollection AddMongo(this IServiceCollection services)
    {

        //Change guid and date to readable string in mango db
        BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
        BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

        services.AddSingleton(serviceProvider =>
        {
            var configuration = serviceProvider.GetService<IConfiguration>(); // get config from service provider
            var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
            var mongoDbSettings = configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
            var mongoClient = new MongoClient(mongoDbSettings.COnnectionString);
            return mongoClient.GetDatabase(serviceSettings.ServiceName);
        });

        return services;
    }

    public static IServiceCollection AddMongoRepository<T>(this IServiceCollection services, string collectionName) where T : IEntity
    {
        services.AddSingleton<IRepository<T>>(serviceProvider =>
        {
            //you have to pass on databse  class implementation to mongorepository 
            //this is how you get it from the service provider.
            var database = serviceProvider.GetService<IMongoDatabase>();
            return new MongoRepository<T>(database, collectionName);
        }
    );
        return services;
    }
}