namespace CTFServer.Repositories.Interface;

public interface IPostRepository : IRepository
{
    /// <summary>
    /// Create a post
    /// </summary>
    /// <param name="post">Post object</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Post> CreatePost(Post post, CancellationToken token = default);

    /// <summary>
    /// Update a post
    /// </summary>
    /// <param name="post">Post object</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task UpdatePost(Post post, CancellationToken token = default);

    /// <summary>
    /// Remove a post
    /// </summary>
    /// <param name="post">文章对象</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task RemovePost(Post post, CancellationToken token = default);

    /// <summary>
    /// Get all posts
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Post[]> GetPosts(CancellationToken token = default);

    /// <summary>
    /// Get a post by id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Post?> GetPostById(string id, CancellationToken token = default);

    /// <summary>
    /// Get a post by id from cache
    /// </summary>
    /// <param name="id"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Post?> GetPostByIdFromCache(string id, CancellationToken token = default);
}