using Bucket.Admin.Dto.Auth;
using Bucket.Admin.IServices;
using Bucket.Admin.IServices.Dto;
using Bucket.Admin.Model.Setting;
using Bucket.Admin.Model.User;
using Bucket.Caching.Abstractions;
using Bucket.Core;
using Bucket.DbContext.SqlSugar;
using Bucket.Exceptions;
using Bucket.Utility;
using Bucket.Utility.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using System.Linq;
using System.Threading.Tasks;

namespace Bucket.Admin.Web.Controllers
{
    /// <summary>
    /// 账户登录
    /// </summary>
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ISqlSugarDbContextFactory _sqlSugarDbContextFactory;
        private readonly BucketSqlSugarClient _superDbContext;
        private readonly IUser _user;
        private readonly ICachingProviderFactory _cachingProviderFactory;
        private readonly IAuthService _authService;
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="sqlSugarDbContextFactory"></param>
        /// <param name="user"></param>
        /// <param name="cachingProviderFactory"></param>
        /// <param name="authService"></param>
        public AuthController(ISqlSugarDbContextFactory sqlSugarDbContextFactory, IUser user, ICachingProviderFactory cachingProviderFactory, IAuthService authService)
        {
            _sqlSugarDbContextFactory = sqlSugarDbContextFactory;
            _superDbContext = _sqlSugarDbContextFactory.Get("admin");
            _user = user;
            _cachingProviderFactory = cachingProviderFactory;
            _authService = authService;
        }

        /// <summary>
        /// 账户登陆 - 密码登陆
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Account/Login")]
        public async Task<LoginOutput> LoginAsync([FromBody] LoginInput input)
        {
            var redis = _cachingProviderFactory.GetCachingProvider("default");
            var redis_img_code = await redis.GetAsync<string>($"ImgCode");
            if (redis_img_code.SafeString().ToLower() != input.ImgCode.SafeString().ToLower())
                throw new BucketException("GO_2003", "图形验证码错误");
            // 短信验证


            // 用户验证
            var userInfo = _superDbContext.Queryable<UserModel>().First(it => it.UserName == input.UserName);
            if (userInfo == null)
                throw new BucketException("GO_0004007", "账号不存在");
            if (userInfo.State != 1)
                throw new BucketException("GO_0004008", "账号状态异常");
            if (userInfo.Password != Encrypt.SHA256(input.Password + userInfo.Salt))
                throw new BucketException("GO_4009", "账号或密码错误");
            // 用户角色
            var roleList = _superDbContext.Queryable<RoleModel, UserRoleModel>((role, urole) => new object[] { JoinType.Inner, role.Id == urole.RoleId })
                 .Where((role, urole) => urole.Uid == userInfo.Id)
                 .Where((role, urole) => role.IsDel == false)
                 .Select((role, urole) => new { role.Key })
                 .ToList();
            // token返回
            var token = _authService.CreateAccessToken(new UserTokenDto
            {
                Email = userInfo.Email,
                Id = userInfo.Id,
                Mobile = userInfo.Mobile,
                RealName = userInfo.RealName,
                Ids = userInfo.Id.ToString()
            }, roleList.Select(it => it.Key).ToList());
            return new LoginOutput
            {
                Data = new
                {
                    AccessToken = $"Bearer {token}",
                    Expire = _authService.GetExpireInValue(4),
                    RealName = userInfo.RealName.SafeString(),
                    Mobile = userInfo.Mobile.SafeString(),
                    userInfo.Id
                }
            };
        }
        /// <summary>
        /// 刷新token
        /// </summary>
        /// <returns></returns>
        [HttpGet("/Account/RefreshToken")]
        [Authorize("permission")]
        public LoginOutput RefreshToken()
        {
            var scopes = _user.Claims.Where(it => it.Type == "scope").Select(it => it.Value).ToList();
            // token返回
            var token = _authService.CreateAccessToken(new UserTokenDto
            {
                Email = string.Empty,
                Id = _user.Id.ToLong(),
                Mobile = _user.MobilePhone,
                RealName = _user.Name,
                Ids = _user.Ids
            }, scopes);
            return new LoginOutput
            {
                Data = new
                {
                    AccessToken = $"Bearer {token}",
                    Expire = _authService.GetExpireInValue(4),
                    RealName = _user.Name,
                    Mobile = _user.MobilePhone,
                    Id = _user.Id.ToLong()
                }
            };
        }
    }
}