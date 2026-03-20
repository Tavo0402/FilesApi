namespace FilesApi.Services
{
    public interface IMinioservice
    {
        Task<List<string>> UploadFilesAsync(List<IFormFile> files);
    }
}
