using System.Collections.Generic;

namespace Bucket.Identity.IServices.Dto
{
    public class UserUdcInfo
    {
        /// <summary>
        /// 真实姓名
        /// </summary>
        public string name { set; get; }
        /// <summary>
        /// 手机号
        /// </summary>
        public string mobilephone { set; get; }
        /// <summary>
        /// 编号
        /// </summary>
        public long uid { set; get; }
        public string status { set; get; }
    }
    public class UdcCont
    {
        public List<UserUdcInfo> phonePidMovelist { set; get; }
    }
}
