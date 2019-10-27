using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Codestellation.Appulse.Tests
{
    public static class Resources
    {
        public const int Port = 52334;

        public class Startup
        {
            public void Configure(IApplicationBuilder app)
                => app.Run(c => c.Response.WriteAsync(EditorConfigContent, Encoding.UTF8));
        }

        public static readonly string EditorConfigContent = @"# http://editorconfig.org
# it's a stub for testing purposes. Don't add here any values.";

        public static IWebHost CreateWebServer() =>
            new WebHostBuilder()
                .UseKestrel(c =>
                {
                    c.AddServerHeader = false;
                    c.ListenAnyIP(Port);
                })
                .UseStartup<Startup>()
                .Build();
    }
}