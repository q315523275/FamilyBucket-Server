namespace Bucket.Identity.Dto.Auth
{
    /// <summary>
    /// 登录出参
    /// </summary>
    public class LoginOutput : BaseOutput
    {
        /// <summary>
        /// 内容
        /// </summary>
        public dynamic Data { set; get; }
    }
}
