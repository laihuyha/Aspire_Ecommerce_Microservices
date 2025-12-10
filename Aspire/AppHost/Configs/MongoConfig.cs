using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppHost.Configs;

public class MongoConfig
{
    public string? Image { get; set; }
    public string? RootUsername { get; set; }
    public string? RootPassword { get; set; }
    public int? Port { get; set; }
    public string? Volume { get; set; }
}
