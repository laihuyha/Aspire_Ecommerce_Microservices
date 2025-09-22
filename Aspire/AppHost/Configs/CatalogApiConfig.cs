using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppHost.Configs;

public class CatalogApiConfig
{
    public string? Environment { get; set; }
    public ConnectionStringsConfig? ConnectionStrings { get; set; }
    public MongoDbConfig? MongoDb { get; set; }
    public class ConnectionStringsConfig
    {
        public string? Database { get; set; }
    }
    public class MongoDbConfig
    {
        public string? Host { get; set; }
        public CredentialsConfig? Credentials { get; set; }
        public class CredentialsConfig
        {
            public string? UserName { get; set; }
            public string? Password { get; set; }
        }
    }
}
