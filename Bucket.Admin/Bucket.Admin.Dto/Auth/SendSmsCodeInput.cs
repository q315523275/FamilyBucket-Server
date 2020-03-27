namespace Bucket.Admin.Dto.Auth
{
    /// <summary>
    /// 发送短信入参
    /// </summary>
    public class SendSmsCodeInput
    {
        /// <summary>
        /// 账号
        /// </summary>
        [NotEmpty("GO_0005002", "手机号不能为空")]
        [Mobile("GO_0005003", "请输入有效手机号")]
        public string Mobile { set; get; }
        /// <summary>
        /// 短信模板名称
        /// </summary>
        public string SmsTemplateName { get; set; } = "smscode";
        /// <summary>
        /// 随机
        /// </summary>
        public string Guid { get; set; }
        /// <summary>
        /// 图形信息
        /// </summary>
        public string ImgCode { get; set; }
    }
}
