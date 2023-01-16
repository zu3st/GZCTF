using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using CTFServer.Middlewares;
using CTFServer.Repositories.Interface;
using CTFServer.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace CTFServer.Controllers;

/// <summary>
/// Asset-related Interfaces
/// </summary>
[ApiController]
[ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
public class AssetsController : ControllerBase
{
    private readonly ILogger<AssetsController> logger;
    private readonly IFileRepository fileRepository;
    private readonly IConfiguration configuration;
    private readonly string basepath;
    private FileExtensionContentTypeProvider extProvider = new();

    public AssetsController(IFileRepository _fileeService,
        IConfiguration _configuration,
        ILogger<AssetsController> _logger)
    {
        fileRepository = _fileeService;
        configuration = _configuration;
        logger = _logger;
        basepath = configuration.GetSection("UploadFolder").Value ?? "uploads";
    }

    /// <summary>
    /// Download File by Hash
    /// </summary>
    /// <remarks>
    /// Downloads files by hash
    /// </remarks>
    /// <param name="hash">File Hash</param>
    /// <param name="filename">Downloaded File Name</param>
    /// <response code="200">Download File</response>
    /// <response code="404">File not found</response>
    [HttpGet("[controller]/{hash:length(64)}/{filename:minlength(1)}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    public IActionResult GetFile([RegularExpression("[0-9a-f]{64}")] string hash, string filename)
    {
        var path = $"{hash[..2]}/{hash[2..4]}/{hash}";
        path = Path.GetFullPath(Path.Combine(basepath, path));

        if (!System.IO.File.Exists(path))
        {
            logger.Log($"Tried to get non-existent file [{hash[..8]}] {filename}", HttpContext.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0", TaskStatus.NotFound, LogLevel.Warning);
            return NotFound(new RequestResponse("File not found", 404));
        }

        if (!extProvider.TryGetContentType(filename, out string? contentType))
            contentType = "application/octet-stream";

        return new PhysicalFileResult(path, contentType)
        {
            FileDownloadName = filename
        };
    }

    /// <summary>
    /// Upload File
    /// </summary>
    /// <remarks>
    /// Uploads one or more files
    /// </remarks>
    /// <param name="files"></param>
    /// <param name="filename"></param>
    /// <param name="token"></param>
    /// <response code="200">Upload File Path</response>
    /// <response code="400">Failed to upload file</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [RequireAdmin]
    [HttpPost("api/[controller]")]
    [ProducesResponseType(typeof(List<LocalFile>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload(List<IFormFile> files, [FromQuery] string? filename, CancellationToken token)
    {
        try
        {
            List<LocalFile> results = new();
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var res = await fileRepository.CreateOrUpdateFile(file, filename, token);
                    logger.SystemLog($"Updated file [{res.Hash[..8]}] {filename ?? file.FileName} - {file.Length} Bytes", TaskStatus.Success, LogLevel.Debug);
                    results.Add(res);
                }
            }
            return Ok(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return BadRequest(new RequestResponse("Encountered IO error while uploading file"));
        }
    }

    /// <summary>
    /// Delete File
    /// </summary>
    /// <remarks>
    /// Deletes file by hash
    /// </remarks>
    /// <param name="hash"></param>
    /// <param name="token"></param>
    /// <response code="200">File deleted successfully</response>
    /// <response code="400">Failed to delete file</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    [RequireAdmin]
    [HttpDelete("api/[controller]/{hash:length(64)}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(string hash, CancellationToken token)
    {
        var result = await fileRepository.DeleteFileByHash(hash, token);

        logger.SystemLog($"Deleted file [{hash[..8]}]...", result, LogLevel.Information);

        return result switch
        {
            TaskStatus.Success => Ok(),
            TaskStatus.NotFound => NotFound(),
            _ => BadRequest(new RequestResponse("Failed to delete file"))
        };
    }
}