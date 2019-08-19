using System;
namespace PROXY_MELI_DATABASE.Mongo
{
    public class ProxyMeliMongoDatabaseSettings : IProxyMeliMongoDatabaseSettings
    {
        public ProxyMeliMongoDatabaseSettings()
        {
        }

        public string ConnectionString { get; set; }
        public string DataBaseName { get; set; }
        public string RequestsCollectionName { get; set; }
        public string ErrorsCollectionName { get; set; }
    }
}
