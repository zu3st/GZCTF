using CTFServer.Extensions;
using CTFServer.Models.Internal;
using CTFServer.Models.Request.Info;
using CTFServer.Repositories.Interface;
using CTFServer.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CTFServer.Controllers;

/// <summary>
/// Global information interface
/// </summary>
[Route("api")]
[ApiController]
public class InfoController : ControllerBase
{
    private readonly IOptionsSnapshot<AccountPolicy> accountPolicy;
    private readonly IOptionsSnapshot<GlobalConfig> globalConfig;
    private readonly IPostRepository postRepository;
    private readonly IRecaptchaExtension recaptchaExtension;

    public InfoController(IPostRepository _postRepository,
        IRecaptchaExtension _recaptchaExtension,
        IOptionsSnapshot<GlobalConfig> _globalConfig,
        IOptionsSnapshot<AccountPolicy> _accountPolicy)
    {
        globalConfig = _globalConfig;
        accountPolicy = _accountPolicy;
        postRepository = _postRepository;
        recaptchaExtension = _recaptchaExtension;
    }

    /// <summary>
    /// Get latest posts
    /// </summary>
    /// <remarks>
    /// Gets the latest posts
    /// </remarks>
    /// <param name="token"></param>
    /// <response code="200">Latest 20 posts</response>
    [HttpGet("Posts/Latest")]
    [ProducesResponseType(typeof(PostInfoModel[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLatestPosts(CancellationToken token)
        => Ok((await postRepository.GetPosts(token)).Take(20).Select(PostInfoModel.FromPost));

    /// <summary>
    /// Get all posts
    /// </summary>
    /// <remarks>
    /// Gets all posts
    /// </remarks>
    /// <param name="token"></param>
    /// <response code="200">List of all posts</response>
    [HttpGet("Posts")]
    [ProducesResponseType(typeof(PostInfoModel[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPosts(CancellationToken token)
        => Ok((await postRepository.GetPosts(token)).Select(PostInfoModel.FromPost));

    /// <summary>
    /// Get Post by id
    /// </summary>
    /// <remarks>
    /// Gets a post by id
    /// </remarks>
    /// <param name="id"></param>
    /// <param name="token"></param>
    /// <response code="200">Post</response>
    [HttpGet("Posts/{id}")]
    [ProducesResponseType(typeof(PostDetailModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPost(string id, CancellationToken token)
    {
        var post = await postRepository.GetPostByIdFromCache(id, token);

        if (post is null)
            return NotFound(new RequestResponse("Post not found", 404));

        return Ok(PostDetailModel.FromPost(post));
    }

    /// <summary>
    /// Get global configuation
    /// </summary>
    /// <remarks>
    /// Gets global configuation
    /// </remarks>
    /// <response code="200">Global configuration</response>
    [HttpGet("Config")]
    [ProducesResponseType(typeof(GlobalConfig), StatusCodes.Status200OK)]
    public IActionResult GetGlobalConfig() => Ok(globalConfig.Value);

    /// <summary>
    /// Get Recaptcha SiteKey
    /// </summary>
    /// <remarks>
    /// Gets Recaptcha SiteKey
    /// </remarks>
    /// <response code="200">Recaptcha SiteKey</response>
    [HttpGet("SiteKey")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public IActionResult GetRecaptchaSiteKey()
        => Ok(accountPolicy.Value.UseGoogleRecaptcha ? recaptchaExtension.SiteKey() : "NOTOKEN");
}
