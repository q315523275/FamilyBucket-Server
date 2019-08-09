using Bucket.Config;
using Bucket.Logging;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bucket.Identity.WebApi
{
    /// <summary>
    /// 应用程序
    /// </summary>
    public class Program
    {
        /// <summary>
        /// 应用程序入口点
        /// </summary>
        /// <param name="args">入口点参数</param>
        public static void Main(string[] args)
        {
            WebHost.CreateDefaultBuilder(args)
                   .UseWebRoot("")
                   .ConfigureAppConfiguration((hostingContext, x) =>
                   {
                       var option = new BucketConfigOptions();
                       x.Build().GetSection("ConfigServer").Bind(option);
                       x.AddBucketConfig(option);
                   })
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
