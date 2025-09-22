using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppHost.Configs;

public class BasketApiConfig
{
    public string? Environment { get; set; }
    public int? HttpPort { get; set; }
    public int? HttpsPort { get; set; }
    public BasketConnectionStrings? ConnectionStrings { get; set; }
    public class BasketConnectionStrings
    {
        public string? Marten { get; set; }
        public string? Redis { get; set; }
    }
}
