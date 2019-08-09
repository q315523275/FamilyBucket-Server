using Bucket.Identity.Dto.Validators;

namespace Bucket.Identity.Dto.Third
{
    /// <summary>
    /// 小程序Code验证入参
    /// </summary>
    public class WxMiniCodeVerifyInput
    {
        /// <summary>
        /// Code
        /// </summary>
        [NotEmpty("WxMini_003", "授权Code不能为空")]
        public string Code { set; get; }
        /// <summary>
        /// 小程序AppId
        /// </summary>
        [NotEmpty("identity_001", "AppId不能为空")]
        public string AppId { set; get; } = "wxf722c01492f27e62";
    }
}
