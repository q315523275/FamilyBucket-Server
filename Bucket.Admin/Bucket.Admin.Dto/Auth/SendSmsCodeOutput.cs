namespace Bucket.Admin.Dto.Auth
{
    /// <summary>
    /// 发送短信出参
    /// </summary>
    public class SendSmsCodeOutput : BaseOutput
    {
        /// <summary>
        /// 内容
        /// </summary>
        public dynamic Data { set; get; }
    }
}
