namespace CTFServer.Repositories.Interface;

public interface IFileRepository : IRepository
{
    /// <summary>
    /// Create or update a file
    /// </summary>
    /// <param name="file">File object</param>
    /// <param name="fileName">Fle name</param>
    /// <param name="token">Cancelleation token</param>
    /// <returns>File id</returns>
    public Task<LocalFile> CreateOrUpdateFile(IFormFile file, string? fileName = null, CancellationToken token = default);

    /// <summary>
    /// Delete a file
    /// </summary>
    /// <param name="file">File object</param>
    /// <param name="token">Cancelleation token</param>
    /// <returns>Task status</returns>
    public Task<TaskStatus> DeleteFile(LocalFile file, CancellationToken token = default);

    /// <summary>
    /// Delete a file by hash
    /// </summary>
    /// <param name="fileHash">File hash</param>
    /// <param name="token">Cancelleation token</param>
    /// <returns>Task status</returns>
    public Task<TaskStatus> DeleteFileByHash(string fileHash, CancellationToken token = default);

    /// <summary>
    /// Get file by hash
    /// </summary>
    /// <param name="fileHash">File hash</param>
    /// <param name="token">Cancelleation token</param>
    /// <returns>File object</returns>
    public Task<LocalFile?> GetFileByHash(string? fileHash, CancellationToken token = default);

    /// <summary>
    /// Get all files
    /// </summary>
    /// <param name="count">Number of files to return</param>
    /// <param name="skip">Number of files to skip</param>
    /// <param name="token">Cancelleation token</param>
    /// <returns>List of file objects</returns>
    public Task<List<LocalFile>> GetFiles(int count, int skip, CancellationToken token = default);
}