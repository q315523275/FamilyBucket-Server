using System.Collections.Concurrent;

namespace Bucket.Config.Dto
{
    public class GetConfigOutput : BaseOutput
    {
        public string AppName { get; set; }
        /// <summary>
        /// 当前最大版本
        /// </summary>
        public long Version { get; set; }
        /// <summary>
        /// Key/Value
        /// </summary>
        public ConcurrentDictionary<string, string> KV { set; get; }
    }
}
