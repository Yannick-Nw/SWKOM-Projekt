namespace Infrastructure.FileStorage
{
    public class MinIOConfiguration
    {
        public string ServiceUrl { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string BucketName { get; set; }
    }
}