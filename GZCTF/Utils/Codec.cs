using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace CTFServer.Utils;

public partial class Codec
{
    /// <summary>
    /// Base64 encoding
    /// </summary>
    public static class Base64
    {
        public static string Decode(string? str, string type = "utf-8")
        {
            if (str is null)
                return string.Empty;

            try
            {
                return Encoding.GetEncoding(type).GetString(Convert.FromBase64String(str));
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string Encode(string? str, string type = "utf-8")
        {
            if (str is null)
                return string.Empty;

            try
            {
                return Convert.ToBase64String(Encoding.GetEncoding(type).GetBytes(str));
            }
            catch
            {
                return string.Empty;
            }
        }

        public static byte[] EncodeToBytes(string? str, string type = "utf-8")
        {
            if (str is null)
                return Array.Empty<byte>();

            byte[] encoded;
            try
            {
                encoded = Encoding.GetEncoding(type).GetBytes(str);
            }
            catch
            {
                return Array.Empty<byte>();
            }

            Span<char> buffer = new char[encoded.Length * 4 / 3 + 8];
            if (Convert.TryToBase64Chars(encoded, buffer, out var charsWritten))
                return Encoding.GetEncoding(type).GetBytes(buffer.Slice(0, charsWritten).ToArray());
            else
                return Array.Empty<byte>();
        }

        public static byte[] DecodeToBytes(string? str)
        {
            if (str is null)
                return Array.Empty<byte>();

            Span<byte> buffer = new byte[str.Length * 3 / 4 + 8];

            if (Convert.TryFromBase64String(str, buffer, out int bytesWritten))
                return buffer.Slice(0, bytesWritten).ToArray();

            return Array.Empty<byte>();
        }
    }

    /// <summary>
    /// Leet
    /// </summary>
    public static class Leet
    {
        private readonly static Dictionary<char, string> CharMap = new()
        {
            ['A'] = "Aa4",
            ['B'] = "Bb68",
            ['C'] = "Cc",
            ['D'] = "Dd",
            ['E'] = "Ee3",
            ['F'] = "Ff1",
            ['G'] = "Gg69",
            ['H'] = "Hh",
            ['I'] = "Ii1l",
            ['J'] = "Jj",
            ['K'] = "Kk",
            ['L'] = "Ll1I",
            ['M'] = "Mm",
            ['N'] = "Nn",
            ['O'] = "Oo0",
            ['P'] = "Pp",
            ['Q'] = "Qq9",
            ['R'] = "Rr",
            ['S'] = "Ss5",
            ['T'] = "Tt7",
            ['U'] = "Uu",
            ['V'] = "Vv",
            ['W'] = "Ww",
            ['X'] = "Xx",
            ['Y'] = "Yy",
            ['Z'] = "Zz2",
            ['0'] = "0oO",
            ['1'] = "1lI",
            ['2'] = "2zZ",
            ['3'] = "3eE",
            ['4'] = "4aA",
            ['5'] = "5Ss",
            ['6'] = "6Gb",
            ['7'] = "7T",
            ['8'] = "8bB",
            ['9'] = "9g"
        };

        public static double LeetEntropy(string flag)
        {
            double entropy = 0;
            var doLeet = false;
            foreach (var c in flag)
            {
                if (c == '{' || c == ']')
                    doLeet = true;
                else if (doLeet && (c == '}' || c == '['))
                    doLeet = false;
                else if (doLeet && CharMap.TryGetValue(char.ToUpperInvariant(c), out var table) && table is not null)
                    entropy += Math.Log(table.Length, 2);
            }
            return entropy;
        }

        public static string LeetFlag(string original)
        {
            StringBuilder sb = new(original.Length);
            Random random = new();

            var doLeet = false;
            // note: only leet 'X' in flag{XXXX_XXX_[TEAM_HASH]_XXX}
            foreach (var c in original)
            {
                if (c == '{' || c == ']')
                    doLeet = true;
                else if (doLeet && (c == '}' || c == '['))
                    doLeet = false;
                else if (doLeet && CharMap.TryGetValue(char.ToUpperInvariant(c), out var table) && table is not null)
                {
                    var nc = table[random.Next(table.Length)];
                    sb.Append(nc);
                    continue;
                }

                sb.Append(c == ' ' ? '_' : c); // replace blank to underline
            }

            return sb.ToString();
        }
    }

    [GeneratedRegex("^(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&*()\\-_=+]).{8,}$")]
    private static partial Regex PasswordRegex();

    /// <summary>
    /// 生成随机密码
    /// </summary>
    /// <param name="length">密码长度</param>
    /// <returns></returns>
    public static string RandomPassword(int length)
    {
        var random = new Random();
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()-_=+";

        string pwd;
        do
        {
            pwd = new string(Enumerable.Repeat(chars, length < 8 ? 8 : length)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        while (!PasswordRegex().IsMatch(pwd));

        return pwd;
    }

    /// <summary>
    /// Convert a byte array to a hex string
    /// </summary>
    /// <param name="bytes">Original byte array</param>
    /// <param name="useLower">Whether to use lowercase</param>
    /// <returns></returns>
    public static string BytesToHex(byte[] bytes, bool useLower = true)
    {
        var output = BitConverter.ToString(bytes).Replace("-", "");
        return useLower ? output.ToLowerInvariant() : output.ToUpperInvariant();
    }

    /// <summary>
    /// Perform XOR operation on a byte array, with a given key
    /// </summary>
    /// <param name="data">Original data</param>
    /// <param name="xor">XOR key</param>
    /// <returns>XOR result</returns>
    public static byte[] Xor(byte[] data, byte[] xor)
    {
        var res = new byte[data.Length];
        for (var i = 0; i < data.Length; ++i)
        {
            res[i] = (byte)(data[i] ^ xor[i % xor.Length]);
        }
        return res;
    }

    /// <summary>
    /// Get the ASCII array of a string
    /// </summary>
    /// <param name="str">Original string</param>
    /// <returns></returns>
    public static List<int> ASCII(string str)
    {
        var buff = Encoding.ASCII.GetBytes(str);
        List<int> res = new();
        foreach (var item in buff)
            res.Add(item);
        return res;
    }

    /// <summary>
    /// Convert a string to a base
    /// </summary>
    /// <param name="source">Source data</param>
    /// <param name="tobase">Target base, supporting 2, 8, 10, 16</param>
    /// <returns></returns>
    public static List<string> ToBase(List<int> source, int tobase)
        => new(source.ConvertAll((a) => Convert.ToString(a, tobase)));

    /// <summary>
    /// Reverse a string
    /// </summary>
    /// <param name="s">Original string</param>
    /// <returns></returns>
    public static string Reverse(string s)
    {
        var charArray = s.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }

    /// <summary>
    /// Get the MD5 hash digest of a string
    /// </summary>
    /// <param name="str">Original String</param>
    /// <param name="useBase64">Whether to use Base64 encoding</param>
    /// <returns></returns>
    public static string StrMD5(string str, bool useBase64 = false)
    {
        var output = MD5.HashData(Encoding.Default.GetBytes(str));
        if (useBase64)
            return Convert.ToBase64String(output);
        return BitConverter.ToString(output).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// Get the SHA256 hash digest of a string
    /// </summary>
    /// <param name="str">Original string</param>
    /// <param name="useBase64">Whether to use Base64 encoding</param>
    /// <returns></returns>
    public static string StrSHA256(string str, bool useBase64 = false)
    {
        var output = SHA256.HashData(Encoding.Default.GetBytes(str));
        if (useBase64)
            return Convert.ToBase64String(output);
        return BitConverter.ToString(output).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// Get the MD5 hash digest of a string
    /// </summary>
    /// <param name="str">Original string</param>
    /// <returns></returns>
    public static byte[] BytesMD5(string str)
        => MD5.HashData(Encoding.Default.GetBytes(str));

    /// <summary>
    /// Get the SHA256 hash digest of a string
    /// </summary>
    /// <param name="str">Original string</param>
    /// <returns></returns>
    public static byte[] BytesSHA256(string str)
        => SHA256.HashData(Encoding.Default.GetBytes(str));

    /// <summary>
    /// Pack multiple files into a zip archive
    /// </summary>
    /// <param name="files">File list</param>
    /// <param name="basepath">Base path</param>
    /// <param name="zipName">Zip file name</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async static Task<Stream> ZipFilesAsync(IEnumerable<LocalFile> files, string basepath, string zipName, CancellationToken token = default)
    {
        var size = files.Select(f => f.FileSize).Sum();

        Stream tmp = size <= 64 * 1024 * 1024 ? new MemoryStream() :
            File.Create(Path.GetTempFileName(), 4096, FileOptions.DeleteOnClose);

        using var zip = new ZipArchive(tmp, ZipArchiveMode.Create, true);

        foreach (var file in files)
        {
            var entry = zip.CreateEntry(Path.Combine(zipName, file.Name), CompressionLevel.Fastest);
            await using var entryStream = entry.Open();
            await using var fileStream = File.OpenRead(Path.Combine(basepath, file.Location, file.Hash));
            await fileStream.CopyToAsync(entryStream, token);
        }

        await tmp.FlushAsync(token);
        return tmp;
    }
}
