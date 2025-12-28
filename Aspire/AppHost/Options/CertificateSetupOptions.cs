namespace AppHost.Options;

public class CertificateSetupOptions
{
    public bool Enabled { get; set; }
    public bool AutoSetup { get; set; }
    public bool ForceRegenerate { get; set; }
}
