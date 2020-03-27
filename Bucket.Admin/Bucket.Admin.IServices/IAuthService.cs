using Bucket.Admin.IServices.Dto;
using System.Collections.Generic;

namespace Bucket.Admin.IServices
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
        /// 获取token有效时长，单位秒
        /// </summary>
        /// <param name="houer"></param>
        /// <returns></returns>
        double GetExpireInValue(int houer);
    }
}
