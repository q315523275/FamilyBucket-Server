namespace Bucket.Admin.Dto.Microservice
{
    public class SetServiceInfoInput
    {
        public string ServiceType { set; get; }
        public string Name { set; get; }
        public string Version { set; get; }
        public string[] Tags { set; get; }
        public HostAndPortDto HostAndPort { set; get; }
        public string HealthCheckUri { set; get; }
    }
    public class HostAndPortDto
    {
        public string Host { set; get; }
        public string Port { set; get; }
    }
}
