namespace AppHost.Options;

public class HttpsCertificateOptions
{
    public string CertificatePath { get; set; } = "/app/certs/aspnetapp.pfx";
    public string CertificatePassword { get; set; } = "AspireSecure2024!";
    public bool AllowInvalid { get; set; } = true;
}
