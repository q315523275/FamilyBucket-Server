using Bucket.Logging;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace Bucket.Config.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebHost.CreateDefaultBuilder(args)
                   .ConfigureLogging((hostingContext, logging) =>
                   {
                       logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging")).ClearProviders()
                              .AddBucketLog(hostingContext.Configuration.GetValue<string>("Project:Name"));
                   })
                   .UseStartup<Startup>()
                   .Build()
                   .Run();
        }
    }
}
