
using Minio;
using Minio.DataModel.Args;

namespace FilesApi.Services
{
    public class MinioService : IMinioservice
    {
        private readonly IMinioClient _minioClient;
        private readonly string _bucketName;
        private readonly string _endpoint;
        private readonly string _accessKey;
        private readonly string _secretKey;
        private readonly string _ssl;

        public MinioService(IConfiguration configuration)
        {
            var section = configuration.GetSection("Minio");

            _bucketName = section.GetValue<string>("BucketName")!;
            _endpoint = section.GetValue<string>("Endpoint")!;
            _accessKey = section.GetValue<string>("AccessKey")!;
            _secretKey = section.GetValue<string>("SecretKey")!;
            _ssl = section.GetValue<string>("UseSSL") ?? "false";

            _minioClient = new MinioClient()
                .WithEndpoint(_endpoint)
                .WithCredentials(_accessKey, _secretKey)
                .WithSSL(bool.Parse(_ssl))
                .Build();
        }

        public async Task<List<string>> UploadFilesAsync(List<IFormFile> files)
        {
            await EnsureBucketExists();

            var semaphore = new SemaphoreSlim(10);

            var tasks = files.Select(async file => {
                await semaphore.WaitAsync();
                try
                {
                    var fileName = $"{Guid.NewGuid()}_{file.FileName}";

                    using var stream = file.OpenReadStream();

                    var putObjectArgs = new PutObjectArgs()
                        .WithBucket(_bucketName)
                        .WithObject(fileName)
                        .WithStreamData(stream)
                        .WithObjectSize(file.Length)
                        .WithContentType(file.ContentType);

                    await _minioClient.PutObjectAsync(putObjectArgs);

                    return $"http://{_endpoint}/{_bucketName}/{fileName}";
                }
                finally
                {
                    semaphore.Release();
                }
            });

            return (await Task.WhenAll(tasks)).ToList();
        }

        private async Task EnsureBucketExists()
        {
            var beArgs = new BucketExistsArgs()
                .WithBucket(_bucketName);

            bool found = await _minioClient.BucketExistsAsync(beArgs);

            if (!found)
            {
                var mbArgs = new MakeBucketArgs()
                    .WithBucket(_bucketName);

                await _minioClient.MakeBucketAsync(mbArgs);
            }
        }
    }
}
