﻿using CTFServer.Models.Data;

namespace CTFServer.Models.Request.Edit;

/// <summary>
/// Flag information（Edit）
/// </summary>
public class FlagInfoModel
{
    /// <summary>
    /// Flag Id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Flag text
    /// </summary>
    public string Flag { get; set; } = string.Empty;

    /// <summary>
    /// Flag attachment
    /// </summary>
    public Attachment? Attachment { get; set; }

    internal static FlagInfoModel FromFlagContext(FlagContext context)
        => new()
        {
            Id = context.Id,
            Flag = context.Flag,
            Attachment = context.Attachment
        };
}