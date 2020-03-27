using Bucket.Caching.Abstractions;
using Bucket.Exceptions;
using Bucket.Utility;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bucket.Admin.WebApi.Controllers
{
    /// <summary>
    /// 工具
    /// </summary>
    public class ToolController : Controller
    {
        private readonly ICachingProviderFactory _cachingProviderFactory;
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cachingProviderFactory"></param>
        public ToolController(ICachingProviderFactory cachingProviderFactory)
        {
            _cachingProviderFactory = cachingProviderFactory;
        }

        /// <summary>
        /// 图形验证码
        /// </summary>
        /// <returns></returns>
        [HttpGet("/Tool/ValidateCode")]
        public async Task<IActionResult> ValidateCode(string guid, int width = 100, int height = 32)
        {
            if (guid.IsEmpty())
                throw new BucketException("pz_001", "请输入用户标识");
            var code = Bucket.Utility.Helpers.Randoms.CreateRandomValue(4, false);
            var st = Bucket.ImgVerifyCode.VerifyCode.CreateByteByImgVerifyCode(code, width, height);
            var redis = _cachingProviderFactory.GetCachingProvider("default");
            await redis.SetAsync($"ImgCode", code, new TimeSpan(0, 5, 0));
            return File(st, "image/jpeg");
        }
    }
}