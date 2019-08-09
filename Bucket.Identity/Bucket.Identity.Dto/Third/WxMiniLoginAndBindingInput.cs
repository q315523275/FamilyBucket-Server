using Bucket.Identity.Dto.Validators;

namespace Bucket.Identity.Dto.Third
{
    /// <summary>
    /// 小程序登录并绑定入参
    /// </summary>
    public class WxMiniLoginAndBindingInput
    {
        /// <summary>
        /// 小程序Code
        /// </summary>
        [NotEmpty("WxMini_003", "授权Code不能为空")]
        public string Code { set; get; }
        /// <summary>
        /// 手机号
        /// </summary>
        [NotEmpty("GO_0005002", "手机号不能为空")]
        [Mobile("GO_0005003", "请输入有效手机号")]
        public string Mobile { set; get; }
        /// <summary>
        /// 短信验证码
        /// </summary>
        [NotEmpty("GO_0005004", "短信验证码不能为空")]
        public string SmsCode { set; get; }
        /// <summary>
        /// 小程序AppId
        /// </summary>
        [NotEmpty("identity_001", "AppId不能为空")]
        public string AppId { set; get; } = "wxf722c01492f27e62";
        /// <summary>
        /// 小程序openId
        /// </summary>
        //[NotEmpty("WxMini_004", "用户标识不能为空")]
        public string OpenId { set; get; }
    }
}
