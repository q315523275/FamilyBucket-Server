using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Bucket.Config.Dto;
using Bucket.Exceptions;
using Bucket.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Bucket.DbContext.SqlSugar;
using Bucket.Config.Model;
using Bucket.Config.IService;

namespace Bucket.Config.WebApi.Controllers
{
    [ApiController]
    public class ConfigController : ControllerBase
    {
        private readonly ISqlSugarDbContextFactory _sqlSugarDbContextFactory;
        private readonly BucketSqlSugarClient _superDbContext;
        private readonly IConfigService _configService;

        public ConfigController(ISqlSugarDbContextFactory sqlSugarDbContextFactory, IConfigService configService)
        {
            _sqlSugarDbContextFactory = sqlSugarDbContextFactory;
            _configService = configService;
            _superDbContext = _sqlSugarDbContextFactory.Get("super");
        }

        /// <summary>
        /// 查询配置信息
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="namespaceName"></param>
        /// <param name="version"></param>
        /// <param name="env"></param>
        /// <param name="sign"></param>
        /// <returns></returns>
        [HttpGet("/configs/{appId}/{namespaceName}")]
        public GetConfigOutput GetConfigs(string appId, string namespaceName, long version, string env, string sign)
        {
            #region 验证
            if (appId.IsEmpty())
                throw new BucketException("config_001", "AppId不能为空");
            if (sign.IsEmpty())
                throw new BucketException("config_002", "签名不能为空");
            if (namespaceName.IsEmpty())
                throw new BucketException("config_005", "NamespaceName不能为空");
            #endregion

            #region 项目与签名验证
            var project = _superDbContext.Queryable<AppInfo>().First(it => it.AppId == appId);
            if (project == null)
                throw new BucketException("config_003", "项目不存在");
            var sign_str = $"appId={project.AppId}&appSecret={project.Secret}&namespaceName={namespaceName}";
            var sign_res = Bucket.Utility.Helpers.Encrypt.SHA256(sign_str);
            if (sign.ToLower() != sign_res)
                throw new BucketException("config_004", "签名错误");
            #endregion

            var tableName = _configService.GetConfigTableName(env.SafeString().ToLower());

            // 同一AppId的公共参数分类
            var public_namespace_list = _superDbContext.Queryable<AppNamespaceInfo>()
                                                       .Where(it => it.AppId == project.AppId && it.IsDeleted == false && it.IsPublic == true)
                                                       .Select(it => it.Name)
                                                       .ToList();
            // 公共配置信息
            var public_config = _superDbContext.Queryable<AppConfigInfo>().AS(tableName)
                                               .Where(it => it.ConfigAppId == project.AppId && it.IsDeleted == false && public_namespace_list.Contains(it.ConfigNamespaceName) && it.Version > version)
                                               .Select(it => new { it.ConfigKey, it.ConfigValue, it.Version })
                                               .ToList();
            // 私有配置信息
            var private_config = _superDbContext.Queryable<AppConfigInfo>().AS(tableName)
                                                .Where(it => it.ConfigAppId == project.AppId && it.IsDeleted == false && it.ConfigNamespaceName == namespaceName && it.Version > version)
                                                .Select(it => new { it.ConfigKey, it.ConfigValue, it.Version })
                                                .ToList();
            // 当前最大版本号
            var maxVersion = version;
            // 配置赋值
            var dic_config = new ConcurrentDictionary<string, string>();
            public_config.ForEach(p =>
            {
                if (p.Version > maxVersion)
                    maxVersion = p.Version;
                dic_config.AddOrUpdate(p.ConfigKey, p.ConfigValue, (x, y) => p.ConfigValue);
            });
            private_config.ForEach(p =>
            {
                if (p.Version > maxVersion)
                    maxVersion = p.Version;
                dic_config.AddOrUpdate(p.ConfigKey, p.ConfigValue, (x, y) => p.ConfigValue);
            });
            // 返回
            return new GetConfigOutput { KV = dic_config, Version = maxVersion, AppName = project.Name };
        }

        /// <summary>
        /// 查询配置信息V2
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="namespaceName"></param>
        /// <param name="version"></param>
        /// <param name="env"></param>
        /// <param name="sign"></param>
        /// <returns></returns>
        [HttpGet("/configs/v2/{appId}/{namespaceName}")]
        public GetConfigOutput GetConfigsV2(string appId, string namespaceName, long version, string env, string sign)
        {
            #region 验证
            if (appId.IsEmpty())
                throw new BucketException("config_001", "AppId不能为空");
            if (sign.IsEmpty())
                throw new BucketException("config_002", "签名不能为空");
            if (namespaceName.IsEmpty())
                throw new BucketException("config_005", "NamespaceName不能为空");
            #endregion

            #region 项目与签名验证
            var project = _superDbContext.Queryable<AppInfo>().First(it => it.AppId == appId);
            if (project == null)
                throw new BucketException("config_003", "项目不存在");
            var sign_str = $"version={version}&env={env}&appId={project.AppId}&appSecret={project.Secret}&namespaceName={namespaceName}";
            var sign_res = Bucket.Utility.Helpers.Encrypt.SHA256(sign_str);
            if (sign.ToLower() != sign_res)
                throw new BucketException("config_004", "签名错误");
            #endregion

            var tableName = _configService.GetConfigTableName(env.SafeString().ToLower());

            // 同一AppId的公共参数分类
            var public_namespace_list = _superDbContext.Queryable<AppNamespaceInfo>()
                                                       .Where(it => it.AppId == project.AppId && it.IsDeleted == false && it.IsPublic == true)
                                                       .Select(it => it.Name)
                                                       .ToList();
            // 公共配置信息
            var public_config = _superDbContext.Queryable<AppConfigInfo>().AS(tableName)
                                               .Where(it => it.ConfigAppId == project.AppId && it.IsDeleted == false && public_namespace_list.Contains(it.ConfigNamespaceName) && it.Version > version)
                                               .Select(it => new { it.ConfigKey, it.ConfigValue, it.Version })
                                               .ToList();
            // 私有配置信息
            var private_config = _superDbContext.Queryable<AppConfigInfo>().AS(tableName)
                                                .Where(it => it.ConfigAppId == project.AppId && it.IsDeleted == false && it.ConfigNamespaceName == namespaceName && it.Version > version)
                                                .Select(it => new { it.ConfigKey, it.ConfigValue, it.Version })
                                                .ToList();
            // 当前最大版本号
            var maxVersion = version;
            // 配置赋值
            var dic_config = new ConcurrentDictionary<string, string>();
            public_config.ForEach(p =>
            {
                if (p.Version > maxVersion)
                    maxVersion = p.Version;
                dic_config.AddOrUpdate(p.ConfigKey, p.ConfigValue, (x, y) => p.ConfigValue);
            });
            private_config.ForEach(p =>
            {
                if (p.Version > maxVersion)
                    maxVersion = p.Version;
                dic_config.AddOrUpdate(p.ConfigKey, p.ConfigValue, (x, y) => p.ConfigValue);
            });
            // 返回
            return new GetConfigOutput { KV = dic_config, Version = maxVersion, AppName = project.Name };
        }
    }
}