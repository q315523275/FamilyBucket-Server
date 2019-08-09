using Bucket.Identity.Dto.Validators;

namespace Bucket.Identity.Dto.Third
{
    /// <summary>
    /// 小程序解绑入参
    /// </summary>
    public class WxMiniUnBindingInput
    {
        /// <summary>
        /// 小程序code
        /// </summary>
        [NotEmpty("WxMini_003", "授权Code不能为空")]
        public string Code { set; get; }
        /// <summary>
        /// 小程序appId
        /// </summary>
        [NotEmpty("identity_001", "AppId不能为空")]
        public string AppId { set; get; } = "wxf722c01492f27e62";
        /// <summary>
        /// 小程序openId
        /// </summary>
        public string OpenId { set; get; }
    }
}
