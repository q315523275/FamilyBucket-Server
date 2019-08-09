using Bucket.Identity.Dto.Validators;

namespace Bucket.Identity.Dto.Third
{
    /// <summary>
    /// 小程序加密的登录入参
    /// </summary>
    public class WxMiniLoginByEncryptInput
    {
        /// <summary>
        /// 加密数据
        /// </summary>
        [NotEmpty("identity_001", "EncryptedData不能为空")]
        public string EncryptedData { set; get; }
        /// <summary>
        /// sessionkey
        /// </summary>
        public string SessionKey { set; get; }
        /// <summary>
        /// 向量
        /// </summary>
        [NotEmpty("identity_001", "密钥向量不能为空")]
        public string IV { set; get; }
        /// <summary>
        /// appId
        /// </summary>
        [NotEmpty("identity_001", "AppId不能为空")]
        public string AppId { set; get; } = "wxf722c01492f27e62";
        /// <summary>
        /// OpenId
        /// </summary>
        [NotEmpty("identity_001", "OpenId不能为空")]
        public string OpenId { set; get; }
    }
}
