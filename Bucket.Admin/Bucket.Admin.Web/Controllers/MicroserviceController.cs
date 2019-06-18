using Bucket.Admin.Dto;
using Bucket.Admin.Dto.Microservice;
using Bucket.Admin.Model.Microservice;
using Bucket.Caching.Abstractions;
using Bucket.DbContext.SqlSugar;
using Bucket.Exceptions;
using Bucket.ServiceDiscovery;
using Bucket.Utility;
using Bucket.Utility.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocelot.Configuration.File;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bucket.Admin.Web.Controllers
{
    /// <summary>
    /// 微服务管理
    /// </summary>
    [ApiController]
    [Authorize("permission")]
    public class MicroserviceController : ControllerBase
    {
        private readonly ISqlSugarDbContextFactory _sqlSugarDbContextFactory;
        private readonly IServiceDiscovery _serviceDiscovery;
        private readonly BucketSqlSugarClient _adminDbContext;
        private readonly ICachingProviderFactory _cachingProviderFactory;
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="sqlSugarDbContextFactory"></param>
        /// <param name="serviceDiscovery"></param>
        /// <param name="cachingProviderFactory"></param>
        public MicroserviceController(ISqlSugarDbContextFactory sqlSugarDbContextFactory, IServiceDiscovery serviceDiscovery, ICachingProviderFactory cachingProviderFactory)
        {
            _sqlSugarDbContextFactory = sqlSugarDbContextFactory;
            _serviceDiscovery = serviceDiscovery;
            _cachingProviderFactory = cachingProviderFactory;
            _adminDbContext = _sqlSugarDbContextFactory.Get("admin");
        }

        /// <summary>
        /// 查询服务发现全部服务
        /// </summary>
        /// <returns></returns>
        [HttpGet("/Microservice/QueryServiceList")]
        public async Task<BaseOutput<object>> QueryServiceList([FromQuery] QueryServiceListInput input)
        {
            if (input.Name.IsEmpty())
            {
                return new BaseOutput<object> { Data = await _serviceDiscovery.FindServiceInstancesAsync() };
            }
            else
            {
                return new BaseOutput<object> { Data = await _serviceDiscovery.FindServiceInstancesAsync(input.Name) };
            }
        }
        /// <summary>
        /// 服务注册
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Microservice/SetServiceInfo")]
        public async Task<BaseOutput> SetServiceInfo([FromBody] SetServiceInfoInput input)
        {
            await _serviceDiscovery.RegisterServiceAsync(serviceName: input.Name,
                                                         version: input.Version,
                                                         serviceType: Bucket.Utility.Helpers.Enum.Parse<Bucket.Values.ServiceType>(input.ServiceType),
                                                         uri: new Uri($"http://{input.HostAndPort.Host}:{input.HostAndPort.Port}"),
                                                         healthCheckUri: new Uri(input.HealthCheckUri),
                                                         tags: input.Tags);
            return new BaseOutput { };
        }
        /// <summary>
        /// 服务移除
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Microservice/DeleteService")]
        public async Task<BaseOutput> DeleteService([FromBody] DeleteServiceInput input)
        {
            await _serviceDiscovery.DeregisterServiceAsync(input.ServiceId);
            return new BaseOutput { };
        }
        /// <summary>
        /// 查询网关配置列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("/Microservice/QueryApiGatewayConfiguration")]
        public BasePageOutput<object> QueryApiGatewayConfiguration([FromQuery] QueryApiGatewayConfigurationInput input)
        {
            var totalNumber = 0;
            var list = _adminDbContext.Queryable<ApiGatewayConfigurationModel>().ToPageList(input.PageIndex, input.PageSize, ref totalNumber);
            return new BasePageOutput<object> { CurrentPage = input.PageIndex, Total = totalNumber, Data = list };
        }
        /// <summary>
        /// 设置网关配置
        /// </summary>
        /// <returns></returns>
        [HttpPost("/Microservice/SetApiGatewayConfiguration")]
        public BaseOutput SetApiGatewayConfiguration([FromBody] SetApiGatewayConfigurationInput input)
        {
            var configInfo = new ApiGatewayConfigurationModel
            {
                GatewayId = input.GatewayId,
                BaseUrl = input.BaseUrl,
                DownstreamScheme = input.DownstreamScheme,
                GatewayKey = input.GatewayKey,
                HttpHandlerOptions = Json.ToJson(input.HttpHandlerOptions),
                LoadBalancerOptions = Json.ToJson(input.LoadBalancerOptions),
                QoSOptions = Json.ToJson(input.QoSOptions),
                RateLimitOptions = Json.ToJson(input.RateLimitOptions),
                RequestIdKey = input.RequestIdKey,
                ServiceDiscoveryProvider = Json.ToJson(input.ServiceDiscoveryProvider)
            };
            if (configInfo.GatewayId > 0)
                _adminDbContext.Updateable(configInfo).ExecuteCommand();
            else
                _adminDbContext.Insertable(configInfo).ExecuteCommand();
            return new BaseOutput { };
        }
        /// <summary>
        /// 查询网关路由列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("/Microservice/QueryApiGatewayReRouteList")]
        public BasePageOutput<object> QueryApiGatewayReRouteList([FromQuery] QueryApiGatewayReRouteListInput input)
        {
            int totalNumber = 0;
            var listResult = _adminDbContext.Queryable<ApiGatewayReRouteModel>()
                                            .WhereIF(input.GatewayId > 0, it => it.GatewayId == input.GatewayId)
                                            .WhereIF(input.State > -1, it => it.State == input.State)
                                            .ToPageList(input.PageIndex, input.PageSize, ref totalNumber);
            return new BasePageOutput<object> { CurrentPage = input.PageIndex, Total = totalNumber, Data = listResult };
        }
        /// <summary>
        /// 设置网关路由
        /// </summary>
        /// <returns></returns>
        [HttpPost("/Microservice/SetApiGatewayReRoute")]
        public BaseOutput SetApiGatewayReRoute([FromBody] SetApiGatewayReRouteInput input)
        {
            var rerouteInfo = new ApiGatewayReRouteModel
            {
                AuthenticationOptions = Json.ToJson(input.AuthenticationOptions),
                CacheOptions = Json.ToJson(input.FileCacheOptions),
                DelegatingHandlers = Json.ToJson(input.DelegatingHandlers),
                DownstreamHostAndPorts = Json.ToJson(input.DownstreamHostAndPorts),
                DownstreamPathTemplate = input.DownstreamPathTemplate,
                Id = input.Id,
                Key = input.Key,
                Priority = input.Priority,
                SecurityOptions = Json.ToJson(input.SecurityOptions),
                ServiceName = input.ServiceName,
                State = input.State,
                Timeout = input.Timeout,
                UpstreamHost = input.UpstreamHost,
                UpstreamHttpMethod = Json.ToJson(input.UpstreamHttpMethod),
                UpstreamPathTemplate = input.UpstreamPathTemplate,
                GatewayId = input.GatewayId,
                DownstreamScheme = input.DownstreamScheme,
                HttpHandlerOptions = Json.ToJson(input.HttpHandlerOptions),
                LoadBalancerOptions = Json.ToJson(input.LoadBalancerOptions),
                QoSOptions = Json.ToJson(input.QoSOptions),
                RateLimitOptions = Json.ToJson(input.RateLimitOptions),
                RequestIdKey = input.RequestIdKey,
            };
            if (rerouteInfo.Id > 0)
            {
                var route = _adminDbContext.Queryable<ApiGatewayReRouteModel>().First(it => it.UpstreamPathTemplate == rerouteInfo.UpstreamPathTemplate && it.GatewayId == rerouteInfo.GatewayId);
                if (route != null && route.Id != rerouteInfo.Id)
                    throw new BucketException("ms_003", "上游路由规则已存在");
                _adminDbContext.Updateable(rerouteInfo).ExecuteCommand();
            }
            else
            {
                // 在网关内已存在
                if (_adminDbContext.Queryable<ApiGatewayReRouteModel>().Any(it => it.UpstreamPathTemplate == rerouteInfo.UpstreamPathTemplate && it.GatewayId == rerouteInfo.GatewayId))
                    throw new BucketException("ms_003", "上游路由规则已存在");
                _adminDbContext.Insertable(rerouteInfo).ExecuteCommand();
            }
            return new BaseOutput { };
        }
        /// <summary>
        /// 同步网关配置到Consul
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpGet("/Microservice/SyncApiGatewayConfigurationToConsul")]
        public async Task<BaseOutput> SyncApiGatewayConfigurationToConsul([FromQuery] SyncApiGatewayConfigurationInput input)
        {
            var configInfo = _adminDbContext.Queryable<ApiGatewayConfigurationModel>().First(it => it.GatewayId == input.GatewayId);
            if (configInfo != null)
            {
                var data = GetGatewayData(input.GatewayId);
                await _serviceDiscovery.KeyValuePutAsync(configInfo.GatewayKey, Json.ToJson(data));
            }
            return new BaseOutput { };
        }
        /// <summary>
        /// 同步网关配置到Redis
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpGet("/Microservice/SyncApiGatewayConfigurationToRedis")]
        public async Task<BaseOutput> SyncApiGatewayConfigurationToRedis([FromQuery] SyncApiGatewayConfigurationInput input)
        {
            var configInfo = _adminDbContext.Queryable<ApiGatewayConfigurationModel>().First(it => it.GatewayId == input.GatewayId);
            if (configInfo != null)
            {
                var data = GetGatewayData(input.GatewayId);
                var redis = _cachingProviderFactory.GetCachingProvider("default_redis");
                await redis.SetAsync($"ApiGateway:{configInfo.GatewayKey}", data, TimeSpan.MaxValue);
            }
            return new BaseOutput { };
        }

        /// <summary>
        /// 查询网关配置数据
        /// </summary>
        /// <param name="gatewayId"></param>
        /// <returns></returns>
        private FileConfiguration GetGatewayData(int gatewayId)
        {
            var fileConfig = new FileConfiguration();
            var configInfo = _adminDbContext.Queryable<ApiGatewayConfigurationModel>().First(it => it.GatewayId == gatewayId);
            if (configInfo != null)
            {
                // config
                var fgc = new FileGlobalConfiguration
                {
                    BaseUrl = configInfo.BaseUrl,
                    DownstreamScheme = configInfo.DownstreamScheme,
                    RequestIdKey = configInfo.RequestIdKey,
                };
                if (!string.IsNullOrWhiteSpace(configInfo.HttpHandlerOptions))
                    fgc.HttpHandlerOptions = Json.ToObject<FileHttpHandlerOptions>(configInfo.HttpHandlerOptions);
                if (!string.IsNullOrWhiteSpace(configInfo.LoadBalancerOptions))
                    fgc.LoadBalancerOptions = Json.ToObject<FileLoadBalancerOptions>(configInfo.LoadBalancerOptions);
                if (!string.IsNullOrWhiteSpace(configInfo.QoSOptions))
                    fgc.QoSOptions = Json.ToObject<FileQoSOptions>(configInfo.QoSOptions);
                if (!string.IsNullOrWhiteSpace(configInfo.RateLimitOptions))
                    fgc.RateLimitOptions = Json.ToObject<FileRateLimitOptions>(configInfo.RateLimitOptions);
                if (!string.IsNullOrWhiteSpace(configInfo.ServiceDiscoveryProvider))
                    fgc.ServiceDiscoveryProvider = Json.ToObject<FileServiceDiscoveryProvider>(configInfo.ServiceDiscoveryProvider);
                fileConfig.GlobalConfiguration = fgc;
                // reroutes
                var reRouteResult = _adminDbContext.Queryable<ApiGatewayReRouteModel>().Where(it => it.GatewayId == configInfo.GatewayId && it.State == 1).ToList();
                if (reRouteResult.Count > 0)
                {
                    var reroutelist = new List<FileReRoute>();
                    foreach (var model in reRouteResult)
                    {
                        var m = new FileReRoute()
                        {
                            UpstreamHost = model.UpstreamHost,
                            UpstreamPathTemplate = model.UpstreamPathTemplate,
                            DownstreamPathTemplate = model.DownstreamPathTemplate,
                            DownstreamScheme = model.DownstreamScheme,
                            ServiceName = model.ServiceName,
                            Priority = model.Priority,
                            RequestIdKey = model.RequestIdKey,
                            Key = model.Key,
                            Timeout = model.Timeout,
                        };
                        if (!string.IsNullOrWhiteSpace(model.UpstreamHttpMethod))
                            m.UpstreamHttpMethod = Json.ToObject<List<string>>(model.UpstreamHttpMethod);
                        if (!string.IsNullOrWhiteSpace(model.DownstreamHostAndPorts))
                            m.DownstreamHostAndPorts = Json.ToObject<List<FileHostAndPort>>(model.DownstreamHostAndPorts);
                        if (!string.IsNullOrWhiteSpace(model.SecurityOptions))
                            m.SecurityOptions = Json.ToObject<FileSecurityOptions>(model.SecurityOptions);
                        if (!string.IsNullOrWhiteSpace(model.CacheOptions))
                            m.FileCacheOptions = Json.ToObject<FileCacheOptions>(model.CacheOptions);
                        if (!string.IsNullOrWhiteSpace(model.HttpHandlerOptions))
                            m.HttpHandlerOptions = Json.ToObject<FileHttpHandlerOptions>(model.HttpHandlerOptions);
                        if (!string.IsNullOrWhiteSpace(model.AuthenticationOptions))
                            m.AuthenticationOptions = Json.ToObject<FileAuthenticationOptions>(model.AuthenticationOptions);
                        if (!string.IsNullOrWhiteSpace(model.RateLimitOptions))
                            m.RateLimitOptions = Json.ToObject<FileRateLimitRule>(model.RateLimitOptions);
                        if (!string.IsNullOrWhiteSpace(model.LoadBalancerOptions))
                            m.LoadBalancerOptions = Json.ToObject<FileLoadBalancerOptions>(model.LoadBalancerOptions);
                        if (!string.IsNullOrWhiteSpace(model.QoSOptions))
                            m.QoSOptions = Json.ToObject<FileQoSOptions>(model.QoSOptions);
                        if (!string.IsNullOrWhiteSpace(model.DelegatingHandlers))
                            m.DelegatingHandlers = Json.ToObject<List<string>>(model.DelegatingHandlers);
                        reroutelist.Add(m);
                    }
                    fileConfig.ReRoutes = reroutelist;
                }
            }
            return fileConfig;
        }
    }
}