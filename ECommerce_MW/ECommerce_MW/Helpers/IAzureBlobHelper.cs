namespace ECommerce_MW.Helpers
{
    public interface IAzureBlobHelper
    {
        //Método UploadBlobAsync Sobrecargado (Overloaded)
        Task<Guid> UploadAzureBlobAsync(IFormFile file, string containerName);

        Task<Guid> UploadAzureBlobAsync(string image, string containerName);

        Task DeleteAzureBlobAsync(Guid id, string containerName);
    }
}
