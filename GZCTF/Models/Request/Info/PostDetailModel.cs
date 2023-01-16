﻿using System.ComponentModel.DataAnnotations;

namespace CTFServer.Models.Request.Info;

/// <summary>
/// Post details
/// </summary>
public class PostDetailModel
{
    /// <summary>
    /// Post id
    /// </summary>
    [Required]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Post title
    /// </summary>
    [Required]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Post summary
    /// </summary>
    [Required]
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Post content
    /// </summary>
    [Required]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Whether the post is pinned
    /// </summary>
    [Required]
    public bool IsPinned { get; set; } = false;

    /// <summary>
    /// Post tags
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// Author avatar
    /// </summary>
    public string? AuthorAvatar { get; set; }

    /// <summary>
    /// Author name
    /// </summary>
    public string? AuthorName { get; set; }

    /// <summary>
    /// Release time
    /// </summary>
    [Required]
    public DateTimeOffset Time { get; set; } = DateTimeOffset.UtcNow;

    internal static PostDetailModel FromPost(Post post)
        => new()
        {
            Id = post.Id,
            Title = post.Title,
            Tags = post.Tags,
            IsPinned = post.IsPinned,
            Summary = post.Summary,
            Content = post.Content,
            Time = post.UpdateTimeUTC,
            AuthorAvatar = post.Author?.AvatarUrl,
            AuthorName = post.Author?.UserName,
        };
}