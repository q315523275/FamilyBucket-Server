using Bucket.Caching.Abstractions;
using Bucket.Config;
using Bucket.Exceptions;
using Bucket.Utility;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Bucket.Identity.WebApi.Controllers
{
    /// <summary>
    /// 工具
    /// </summary>
    public class ToolController : Controller
    {
        private readonly ICachingProviderFactory _cachingProviderFactory;
        private readonly IConfig _config;
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cachingProviderFactory"></param>
        /// <param name="config"></param>
        public ToolController(ICachingProviderFactory cachingProviderFactory, IConfig config)
        {
            _cachingProviderFactory = cachingProviderFactory;
            _config = config;
        }

        /// <summary>
        /// 图形验证码
        /// </summary>
        /// <returns></returns>
        [HttpGet("/ValidateCode")]
        public IActionResult ValidateCode(string guid, int width = 100, int height = 32)
        {
            if (guid.IsEmpty())
                throw new BucketException("pz_001", "请输入用户标识");
            var redis = _cachingProviderFactory.GetCachingProvider("default_redis");
            var code = Bucket.Utility.Helpers.Randoms.CreateRandomValue(4, false);
            redis.SetAsync($"ImgCode:{guid}", code, new TimeSpan(0, 0, 0, 300));
            var st = Bucket.ImgVerifyCode.VerifyCode.CreateByteByImgVerifyCode(code, width, height);
            return File(st, "image/jpeg");
        }
    }
}