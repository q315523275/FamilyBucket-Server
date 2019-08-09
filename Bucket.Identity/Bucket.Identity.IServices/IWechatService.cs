using Bucket.Identity.IServices.Dto;
using System.Threading.Tasks;

namespace Bucket.Identity.IServices
{
    public interface IWechatService
    {
        Task<QueryOpenIdOutput> QueryAppletOpenIdAsync(string code, string appId);
        WxUserInfoDto AppletAesDecrypt(string encryptedDataStr, string key, string iv);
    }
}
