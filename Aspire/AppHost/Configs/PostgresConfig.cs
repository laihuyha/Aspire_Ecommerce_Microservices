using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppHost.Configs;

public class PostgresConfig
{
    public string? Image { get; set; }
    public string? User { get; set; }
    public string? Password { get; set; }
    public string? Db { get; set; }
    public int? Port { get; set; }
    public string? Volume { get; set; }
}
