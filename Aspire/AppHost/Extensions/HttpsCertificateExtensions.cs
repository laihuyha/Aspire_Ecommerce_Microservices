using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using AppHost.Options;

namespace AppHost.Extensions;

    /// <summary>
    /// Extension methods for configuring baked-in HTTPS certificates for .NET Aspire services.
    /// </summary>
    public static class HttpsCertificateExtensions
    {
        /// <summary>
        /// Configures the service to use a baked-in HTTPS certificate for Docker deployments.
        /// This assumes the certificate file 'aspnetapp.pfx' is located at '/app/certs/aspnetapp.pfx'
        /// inside the Docker container and has been generated using the generate-aspire-cert.sh script.
        /// </summary>
        /// <param name="builder">The project resource builder.</param>
        /// <param name="options">The HTTPS certificate configuration options.</param>
        /// <returns>The updated resource builder.</returns>
        public static IResourceBuilder<ProjectResource> WithBakedInHttpsCertificate(
            this IResourceBuilder<ProjectResource> builder,
            HttpsCertificateOptions options = null)
        {
            options ??= new HttpsCertificateOptions();

            return builder.PublishAsDockerComposeService((resource, service) =>
            {
                // Configure Kestrel to use the baked-in certificate
                service.Environment["ASPNETCORE_Kestrel__Certificates__Default__Path"] = options.CertificatePath;
                service.Environment["ASPNETCORE_Kestrel__Certificates__Default__Password"] = options.CertificatePassword;
                service.Environment["ASPNETCORE_Kestrel__Certificates__Default__AllowInvalid"] = options.AllowInvalid.ToString().ToLowerInvariant();
            });
        }
    }
