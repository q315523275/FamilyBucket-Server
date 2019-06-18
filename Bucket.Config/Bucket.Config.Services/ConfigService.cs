using Bucket.Config.IService;
using Bucket.Config.Model;
using Bucket.Exceptions;
using System;

namespace Bucket.Config.Services
{
    public class ConfigService : IConfigService
    {
        public string GetConfigTableName(string environment)
        {
            Enum.TryParse<ConfigEnvironment>(environment, out var env);
            var tableName = "tb_appconfig_test";
            switch (env)
            {
                case ConfigEnvironment.dev:
                    tableName = $"tb_appconfig_{ConfigEnvironment.dev.ToString()}";
                    break;
                case ConfigEnvironment.pro:
                    tableName = $"tb_appconfig_{ConfigEnvironment.pro.ToString()}";
                    break;
                case ConfigEnvironment.prepro:
                    tableName = $"tb_appconfig_{ConfigEnvironment.prepro.ToString()}";
                    break;
                case ConfigEnvironment.uat:
                    tableName = $"tb_appconfig_{ConfigEnvironment.uat.ToString()}";
                    break;
                default:
                    throw new BucketException("config_06", "环境不存在");
            }
            return tableName;
        }
    }
}
