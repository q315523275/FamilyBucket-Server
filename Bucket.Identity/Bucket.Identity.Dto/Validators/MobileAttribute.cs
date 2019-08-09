using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Bucket.Identity.Dto.Validators
{
    /// <summary>
    /// 对象不能为空
    /// </summary>
    public class MobileAttribute : ValidationAttribute
    {
        //手机号正则表达式
        private static Regex _mobileregex = new Regex("^(13|14|15|16|17|18|19)[0-9]{9}$");
        private string ErrorCode { get; set; }
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="errorMessage"></param>
        public MobileAttribute(string errorCode, string errorMessage)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }
        /// <summary>
        /// 获取错误描述json string
        /// </summary>
        /// <returns></returns>
        private string GetErrorMessage()
        {
            return string.Concat("{", string.Format("\"ErrorCode\":\"{0}\",\"Message\":\"{1}\"", ErrorCode, ErrorMessage), "}");
        }
        /// <summary>
        /// 验证
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var errormsg = GetErrorMessage();
            if (value == null)
                return ValidationResult.Success;
            if (string.IsNullOrWhiteSpace(value.ToString()))
                return ValidationResult.Success;
            if (_mobileregex.IsMatch(value.ToString()))
                return ValidationResult.Success;
            else
                return new ValidationResult(errormsg);
        }
    }
}
