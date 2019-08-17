using System;
namespace PROXY_MELI_DATABASE.Mongo
{
    public interface IProxyMeliMongoDatabaseSettings
    {

        string ConnectionString { get; set; }
        string DataBaseName { get; set; }
        string RequestsCollectionName { get; set; }
        string ConfigCollectionName { get; set; }
    }
}
