using Bucket.Admin.IServices;
using Bucket.Admin.IServices.Dto;
using Bucket.Authorize.Abstractions;
using Bucket.Caching.Abstractions;
using Bucket.Config;
using Bucket.Utility;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Bucket.Admin.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfig _config;
        private readonly ITokenBuilder _tokenBuilder;
        private readonly ICachingProviderFactory _cachingProviderFactory;

        public AuthService(IConfig config, ITokenBuilder tokenBuilder, ICachingProviderFactory cachingProviderFactory)
        {
            _config = config;
            _tokenBuilder = tokenBuilder;
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
