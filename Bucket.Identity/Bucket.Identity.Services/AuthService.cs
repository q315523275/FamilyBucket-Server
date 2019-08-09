using Bucket.Authorize.Abstractions;
using Bucket.Caching.Abstractions;
using Bucket.Config;
using Bucket.Exceptions;
using Bucket.Identity.IServices;
using Bucket.Identity.IServices.Dto;
using Bucket.Utility;
using Bucket.Utility.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Bucket.Identity.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfig _config;
        private readonly ITokenBuilder _tokenBuilder;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICachingProviderFactory _cachingProviderFactory;

        public AuthService(IConfig config, ITokenBuilder tokenBuilder, IHttpClientFactory httpClientFactory, ICachingProviderFactory cachingProviderFactory)
        {
            _config = config;
            _tokenBuilder = tokenBuilder;
            _httpClientFactory = httpClientFactory;
            _cachingProviderFactory = cachingProviderFactory;
        }

        /// <summary>
        /// 创建Jwt Token
        /// </summary>
        /// <param name="userInfo"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        public string CreateAccessToken(UserTokenDto userInfo, List<string> roles)
        {
            // 用户基本信息
            var claims = new List<Claim> {
                new Claim("Uid", userInfo.Id.ToString()),
                new Claim("Uids", userInfo.Ids),
                new Claim("Name", userInfo.RealName.SafeString()),
                new Claim("MobilePhone", userInfo.Mobile.SafeString()),
                new Claim("Email", userInfo.Email.SafeString())
            };
            // 角色数据
            foreach (var info in roles)
            {
                claims.Add(new Claim("scope", info));
            }
            var expires = _config.StringGet("TokenExpires", "4");
            var token = _tokenBuilder.BuildJwtToken(claims, DateTime.UtcNow.AddMinutes(-3), DateTime.Now.AddHours(Convert.ToInt32(expires)));
            // accessToken
            return token.TokenValue;
        }
        /// <summary>
        /// 短息发送
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="smsTemplateName"></param>
        public async Task<string> SendSmsCodeAsync(string mobile, string smsTemplateName)
        {
            var errCountKey = string.Format(CacheKeys.SmsCodeVerifyErr, mobile);
            var sendCountKey = string.Format(CacheKeys.SmsCodeSendIdentity, mobile);
            var loginCodeKey = string.Format(CacheKeys.SmsCodeLoginCode, mobile);

            var redis = _cachingProviderFactory.GetCachingProvider("default_redis");

            // 错误次数过多
            var errCount = await redis.GetAsync<int>(errCountKey);
            if (errCount > 5)
                throw new BucketException("GO_0005055", "登陆错误次数过多，请30分钟后再试");

            // 验证一分钟发一条
            if (await redis.ExistsAsync(sendCountKey))
                throw new BucketException("GO_2001", "一分钟只能发送一条短信");

            // 生成验证码
            string loginCode = Randoms.CreateRandomValue(6, true);

            // 基础键值
            // @event.MobIp.Split(',')[0] 当多层代理时x-forwarded-for多ip
            // 第三方短信发送

            // 验证码缓存
            await redis.SetAsync(loginCodeKey, loginCode, new TimeSpan(0, 0, 0, 300));
            // 发送人缓存(60秒发一次)
            await redis.SetAsync(sendCountKey, loginCode, new TimeSpan(0, 0, 0, 60));

            return loginCode;
        }
        /// <summary>
        /// 验证短信验证码
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="smsCode"></param>
        /// <returns></returns>
        public async Task VerifySmsCodeAsync(string mobile, string smsCode)
        {
            if (smsCode.IsEmpty())
                throw new BucketException("GO_0005014", "验证码不存在");

            // 配置中心验证是否验证码
            var isVerify = _config.StringGet("IsVerifySmsCode");
            if (!isVerify.IsEmpty() && isVerify == "2")
                return;
            // keys
            var errCountKey = string.Format(CacheKeys.SmsCodeVerifyErr, mobile);
            var loginCodeKey = string.Format(CacheKeys.SmsCodeLoginCode, mobile);
            var sendCountKey = string.Format(CacheKeys.SmsCodeSendIdentity, mobile);

            var redis = _cachingProviderFactory.GetCachingProvider("default_redis");

            // 错误次数判断
            var errCount = await redis.GetAsync<int>(errCountKey);
            if (errCount > 5)
                throw new BucketException("GO_0005055", "登陆错误次数过多，请30分钟后再试");

            // 短信验证码验证
            var verifyValue = await redis.GetAsync<string>(loginCodeKey);
            if (verifyValue.IsEmpty())
                throw new BucketException("GO_0005014", "验证码不存在");

            if (verifyValue.SafeString().ToLower() != smsCode.SafeString().ToLower())
            {
                // 记录错误次数,30分钟
                await redis.SetAsync(errCountKey, (errCount + 1), new TimeSpan(0, 30, 0));
                // 抛出异常
                throw new BucketException("GO_0005015", "验证码错误");
            }
            // 清除次数
            var delKeys = new List<string> { errCountKey, loginCodeKey, sendCountKey };
            await redis.RemoveAllAsync(delKeys);
            // 有人担保，非开发人员
        }
        /// <summary>
        /// 获取token有效时长，单位秒
        /// </summary>
        /// <param name="houer"></param>
        /// <returns></returns>
        public double GetExpireInValue(int houer)
        {
            return new TimeSpan(houer, 0, 0).TotalSeconds;
        }
    }
}
