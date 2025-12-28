using AppHost.Utils;

namespace AppHost.PathConstants
{
    public static class Constants
    {
        public static string ServicesPath => PathHelper.GetServicesPath();
        public const string CertificateScript = "tools/generate-aspire-cert.sh";
        public const string CertificateDirectory = "certs";
        public const string CertificateFile = "aspnetapp.pfx";
    }
}
