using System;

namespace AppHost.Options;

public enum DatabaseType
{
    PostgreSQL,
    SqlServer,
    MySQL,
    MongoDB
}

public class DatabaseOptions
{
    public DatabaseType Type { get; set; } = DatabaseType.PostgreSQL;
    public string Username { get; set; } = "postgres";
    public string Password { get; set; } = "dev_password_123";
    public string DatabaseName { get; set; } = "appdb";
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5432;
    public string Image { get; set; } = "postgres:16-alpine";
    public string VolumeName { get; set; } = "catalog_data";
    public string DataPath { get; set; } = "/var/lib/postgresql/data";

    // SQL Server specific
    public string SqlServerImage { get; set; } = "mcr.microsoft.com/mssql/server:2022-latest";
    public string SqlServerPassword { get; set; } = "YourStrong!Passw0rd";

    // MySQL specific
    public string MySqlImage { get; set; } = "mysql:8.0";

    // MongoDB specific
    public string MongoImage { get; set; } = "mongo:7.0";

    public string GetConnectionString()
    {
        return Type switch
        {
            DatabaseType.PostgreSQL => $"Host={Host};Port={Port};Database={DatabaseName};Username={Username};Password={Password}",
            DatabaseType.SqlServer => $"Server={Host},{Port};Database={DatabaseName};User Id={Username};Password={Password};TrustServerCertificate=true",
            DatabaseType.MySQL => $"Server={Host};Port={Port};Database={DatabaseName};User={Username};Password={Password}",
            DatabaseType.MongoDB => $"mongodb://{Username}:{Password}@{Host}:{Port}/{DatabaseName}",
            _ => throw new NotSupportedException($"Database type {Type} is not supported")
        };
    }

    public string GetDockerImage()
    {
        return Type switch
        {
            DatabaseType.PostgreSQL => Image,
            DatabaseType.SqlServer => SqlServerImage,
            DatabaseType.MySQL => MySqlImage,
            DatabaseType.MongoDB => MongoImage,
            _ => throw new NotSupportedException($"Database type {Type} is not supported")
        };
    }

    public int GetDefaultPort()
    {
        return Type switch
        {
            DatabaseType.PostgreSQL => 5432,
            DatabaseType.SqlServer => 1433,
            DatabaseType.MySQL => 3306,
            DatabaseType.MongoDB => 27017,
            _ => throw new NotSupportedException($"Database type {Type} is not supported")
        };
    }
}
