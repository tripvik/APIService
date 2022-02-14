using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace APIService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }


        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel();
                    webBuilder.UseIISIntegration();
                    webBuilder.UseUrls("https://localhost:65001");
                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                        serverOptions.ListenAnyIP(65001, listenOptions =>
                        {
                            listenOptions.UseHttps(httpsOptions =>
                            {
                                var localhostCert = CertificateLoader.LoadFromStoreCert(
                                    "localhost", "My", StoreLocation.LocalMachine,
                                    allowInvalid: true);
                                var certs = new Dictionary<string, X509Certificate2>(
                                    StringComparer.OrdinalIgnoreCase)
                                {
                                    ["localhost"] = localhostCert
                                };

                                httpsOptions.ServerCertificateSelector = (connectionContext, name) =>
                                {
                                    if (name is not null && certs.TryGetValue(name, out var cert))
                                    {
                                        return cert;
                                    }

                                    return localhostCert;
                                };
                            });
                        });

                    });
                    webBuilder.UseStartup<Startup>();
                })
                .UseWindowsService(options =>
                {
                    options.ServiceName = ".NET Joke Service";
                });
    }
}
