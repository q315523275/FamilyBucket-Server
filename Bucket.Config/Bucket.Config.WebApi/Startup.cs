using Bucket.AspNetCore.Extensions;
using Bucket.AspNetCore.Filters;
using Bucket.DbContext;
using Bucket.DependencyInjection;
using Bucket.EventBus.Extensions;
using Bucket.EventBus.RabbitMQ.Extensions;
using Bucket.Logging.Events;
using Bucket.Utility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;

namespace Bucket.Config.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // 添加全家桶服务
            services.AddFamilyBucket(familyBucket =>
            {
                // 添加AspNetCore基础服务
                familyBucket.AddAspNetCore();
                // 添加数据ORM、数据仓储
                familyBucket.AddSqlSugarDbContext().AddSqlSugarDbRepository();
                // 添加事件驱动
                familyBucket.AddEventBus(builder => { builder.UseRabbitMQ(); });
                // 添加事件队列日志和告警信息
                familyBucket.AddLogEventTransport();
                // 添加工具组件
                familyBucket.AddUtil();
                // 添加应用批量注册
                familyBucket.BatchRegisterService(Assembly.Load("Bucket.Config.Services"), "Service", ServiceLifetime.Scoped);
            });
            // 添加过滤器
            services.AddMvc(option => { option.Filters.Add(typeof(WebApiActionFilterAttribute)); }).AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // 全局错误日志
            app.UseErrorLog();
            // 静态文件
            app.UseStaticFiles();
            // 使用默认路由规则
            app.UseMvc();
        }
    }
}
