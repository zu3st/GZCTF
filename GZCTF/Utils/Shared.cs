using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CTFServer.Utils;

/// <summary>
/// Task Result
/// </summary>
/// <typeparam name="TResult">Return value type</typeparam>
/// <param name="Status">Status</param>
/// <param name="Result">Result</param>
public record TaskResult<TResult>(TaskStatus Status, TResult? Result = default);

/// <summary>
/// Request Response
/// </summary>
/// <param name="Title">Response message</param>
/// <param name="Status">Status code</param>
public record RequestResponse(string Title, int Status = 400);

/// <summary>
/// Request Response
/// </summary>
/// <param name="Title">Response message</param>
/// <param name="Data">Data</param>
/// <param name="Status">Status code</param>
public record RequestResponse<T>(string Title, T Data, int Status = 400);

/// <summary>
/// Array Response
/// </summary>
/// <typeparam name="T"></typeparam>
public class ArrayResponse<T> where T : class
{
    public ArrayResponse(T[] array, int? tot = null)
    {
        Data = array;
        Total = tot ?? array.Length;
    }

    /// <summary>
    /// Data
    /// </summary>
    [Required]
    public T[] Data { get; set; }

    /// <summary>
    /// Data Length
    /// </summary>
    [Required]
    public int Length => Data.Length;

    /// <summary>
    /// Total
    /// </summary>
    public int Total { get; set; }
}

/// <summary>
/// Three first blood bonuses
/// </summary>
public struct BloodBonus
{
    public const long DefaultValue = (50 << 20) + (30 << 10) + 10;
    public const int Mask = 0x3ff;
    public const int Base = 1000;

    public BloodBonus(long init = DefaultValue) => Val = init;

    public static BloodBonus Default => new();

    public long Val { get; private set; } = DefaultValue;

    public static BloodBonus FromValue(long value)
    {
        if ((value & Mask) > Base || ((value >> 10) & Mask) > Base || ((value >> 20) & Mask) > Base)
            return new();
        return new(value);
    }

    public long FirstBlood => (Val >> 20) & 0x3ff;

    public float FirstBloodFactor => FirstBlood / 1000f + 1.0f;

    public long SecondBlood => (Val >> 10) & 0x3ff;

    public float SecondBloodFactor => SecondBlood / 1000f + 1.0f;

    public long ThirdBlood => Val & 0x3ff;

    public float ThirdBloodFactor => ThirdBlood / 1000f + 1.0f;

    public bool NoBonus => Val == 0;

    public static ValueConverter<BloodBonus, long> Converter => new(v => v.Val, v => new(v));

    public static ValueComparer<BloodBonus> Comparer => new((a, b) => a.Val == b.Val, c => c.Val.GetHashCode());
}