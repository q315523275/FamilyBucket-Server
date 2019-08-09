using Bucket.Identity.Dto.Validators;

namespace Bucket.Identity.Dto.Auth
{
    /// <summary>
    /// 登录入参
    /// </summary>
    public class LoginByMobileInput
    {
        /// <summary>
        /// 手机号
        /// </summary>
        [NotEmpty("GO_0005002", "手机号不能为空")]
        [Mobile("GO_0005003", "请输入有效手机号")]
        public string Mobile { set; get; }
        /// <summary>
        /// 渠道
        /// </summary>
        public string ServiceChannel { set; get; } = string.Empty;
    }
}
