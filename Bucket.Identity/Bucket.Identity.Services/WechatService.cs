using Bucket.Config;
using Bucket.DbContext.SqlSugar;
using Bucket.Exceptions;
using Bucket.Identity.IServices;
using Bucket.Identity.IServices.Dto;
using Bucket.Identity.Model;
using Bucket.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Bucket.Identity.Services
{
    public class WechatService : IWechatService
    {
        private readonly ISqlSugarDbContextFactory _sqlSugarDbContextFactory;
        private readonly BucketSqlSugarClient _superDbContext;
        private readonly IConfig _config;
        private readonly IHttpClientFactory _httpClientFactory;

        public WechatService(ISqlSugarDbContextFactory sqlSugarDbContextFactory, IConfig config, IHttpClientFactory httpClientFactory)
        {
            _sqlSugarDbContextFactory = sqlSugarDbContextFactory;
            _superDbContext = _sqlSugarDbContextFactory.Get("super");
            _config = config;
            _httpClientFactory = httpClientFactory;
        }


        /// <summary>
        /// 查询微信小程序openid
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task<QueryOpenIdOutput> QueryAppletOpenIdAsync(string code, string appid)
        {
            var apibaseurl = _config.StringGet("WxMiniApiUrl");
            // app信息
            var appinfo = _superDbContext.Queryable<WechatAppInfo>().First(it => it.AppId == appid);
            if (appinfo == null)
                throw new NotImplementedException();
            var wxapiurl = $"{apibaseurl}/sns/jscode2session?appid={appid}&secret={appinfo.AppSecret}&js_code={code}&grant_type=authorization_code";
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(wxapiurl);
            response.EnsureSuccessStatusCode();
            var jobject = JObject.Parse(await response.Content.ReadAsStringAsync());
            if (jobject.Property("errcode") != null)
                throw new BucketException($"wxmini_{jobject.GetValue("errcode")}", "微信小程序code无效");
            var output = new QueryOpenIdOutput();
            if (jobject.Property("openid") != null)
                output.OpenId = jobject.GetValue("openid").SafeString();
            if (jobject.Property("session_key") != null)
                output.SessionKey = jobject.GetValue("session_key").SafeString();
            if (jobject.Property("unionid") != null)
                output.UnionId = jobject.GetValue("unionid").SafeString();
            return output;
        }


        /// <summary>
        /// 微信小程序 encryptedData 解密
        /// </summary>
        /// <param name="encryptedDataStr"></param>
        /// <param name="key">session_key</param>
        /// <param name="iv">iv</param>
        /// <returns></returns>
        public WxUserInfoDto AppletAesDecrypt(string encryptedDataStr, string key, string iv)
        {
            try
            {
                var rijalg = Aes.Create();
                // RijndaelManaged rijalg = new RijndaelManaged();
                //----------------- 
                //设置 cipher 格式 AES-128-CBC 

                rijalg.KeySize = 128;

                rijalg.Padding = PaddingMode.PKCS7;
                rijalg.Mode = CipherMode.CBC;

                rijalg.Key = Convert.FromBase64String(key);
                rijalg.IV = Convert.FromBase64String(iv);

                byte[] encryptedData = Convert.FromBase64String(encryptedDataStr);
                //解密 
                ICryptoTransform decryptor = rijalg.CreateDecryptor(rijalg.Key, rijalg.IV);

                string result;

                using (MemoryStream msDecrypt = new MemoryStream(encryptedData))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            result = srDecrypt.ReadToEnd();
                        }
                    }
                }
                if (string.IsNullOrWhiteSpace(result))
                    throw new BucketException("identity_021", "用户数据验证失败,请稍后再试");

                return JsonConvert.DeserializeObject<WxUserInfoDto>(result);
            }
            catch
            {
                throw new BucketException("identity_021", "用户数据验证失败,请稍后再试");
            }
        }
    }
}
