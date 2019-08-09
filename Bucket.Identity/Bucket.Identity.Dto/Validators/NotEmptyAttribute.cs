using System.ComponentModel.DataAnnotations;

namespace Bucket.Identity.Dto.Validators
{
    /// <summary>
    /// 对象不能为空
    /// </summary>
    public class NotEmptyAttribute : ValidationAttribute
    {
        private string ErrorCode { get; set; }
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="errorMessage"></param>
        public NotEmptyAttribute(string errorCode, string errorMessage)
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
                return new ValidationResult(errormsg);
            if (string.IsNullOrWhiteSpace(value.ToString()))
                return new ValidationResult(errormsg);
            return ValidationResult.Success;
        }
    }
}
