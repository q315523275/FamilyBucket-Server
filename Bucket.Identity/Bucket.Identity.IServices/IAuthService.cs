using Bucket.Identity.IServices.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bucket.Identity.IServices
{
    public interface IAuthService
    {
        /// <summary>
        /// 创建jwt token
        /// </summary>
        /// <param name="userInfo"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        string CreateAccessToken(UserTokenDto userInfo, List<string> roles);
        /// <summary>
        /// 短息发送
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="smsTemplateName"></param>
        Task<string> SendSmsCodeAsync(string mobile, string smsTemplateName);
        /// <summary>
        /// 验证短信验证码
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="smsCode"></param>
        /// <returns></returns>
        Task VerifySmsCodeAsync(string mobile, string smsCode);
        /// <summary>
        /// 获取token有效时长，单位秒
        /// </summary>
        /// <param name="houer"></param>
        /// <returns></returns>
        double GetExpireInValue(int houer);
    }
}
