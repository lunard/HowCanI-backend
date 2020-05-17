using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HowCanI
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
                    webBuilder.UseKestrel((ctx, options) =>
                    {
                        var config = ctx.Configuration;

                        options.Limits.MaxRequestBodySize = (long)1024 * 1024 * 1024 * 2; // 2 GByte
                        //options.Limits.MinRequestBodyDataRate =
                        //    new MinDataRate(bytesPerSecond: 100,
                        //        gracePeriod: TimeSpan.FromSeconds(10));
                        //options.Limits.MinResponseDataRate =
                        //    new MinDataRate(bytesPerSecond: 100,
                        //        gracePeriod: TimeSpan.FromSeconds(10));
                        options.Limits.RequestHeadersTimeout =
                            TimeSpan.FromMinutes(2);
                    });

                    webBuilder.UseStartup<Startup>();
                });
    }
}
