using HRestaurant.DTOS.Common;
using HRestaurant.Enum;
using HRestaurant.Exceptions;

namespace HRestaurant.Extentions;

public static class FileExtention
{
    private const long MaximumImageBytes = 5 * 1024 * 1024;
    private static readonly IReadOnlyDictionary<string, string> AllowedImages =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp"
        };

    public static bool CheckFileSize(this FileUploadDTO file, FileSize fileSize, long size) =>
        fileSize switch
        {
            FileSize.Kb => file.Length <= size * 1024,
            FileSize.Mb => file.Length <= size * 1024 * 1024,
            FileSize.Gb => file.Length <= size * 1024 * 1024 * 1024,
            _ => false
        };

    public static void DeleteFile(this string filename, params string[] roots)
    {
        if (string.IsNullOrWhiteSpace(filename) || roots.Length == 0) return;
        var root = Path.GetFullPath(Path.Combine(roots))
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var safeName = Path.GetFileName(filename);
        var path = Path.GetFullPath(Path.Combine(root, safeName));
        if (!path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            return;
        if (File.Exists(path)) File.Delete(path);
    }

    public static async Task<string> CreateFileAsync(
        this FileUploadDTO file,
        CancellationToken cancellationToken = default,
        params string[] roots)
    {
        ArgumentNullException.ThrowIfNull(file);
        if (roots.Length == 0)
            throw new InvalidOperationException("An upload directory is required.");

        var extension = Path.GetExtension(Path.GetFileName(file.FileName)).ToLowerInvariant();
        if (file.Length is <= 0 or > MaximumImageBytes
            || !AllowedImages.TryGetValue(extension, out var contentType)
            || !string.Equals(file.ContentType, contentType, StringComparison.OrdinalIgnoreCase))
        {
            throw InvalidImage();
        }

        if (file.Content.CanSeek) file.Content.Position = 0;
        var signature = new byte[12];
        var signatureLength = 0;
        while (signatureLength < signature.Length)
        {
            var read = await file.Content.ReadAsync(
                signature.AsMemory(signatureLength, signature.Length - signatureLength),
                cancellationToken);
            if (read == 0) break;
            signatureLength += read;
        }
        if (!HasValidSignature(extension, signature.AsSpan(0, signatureLength)))
            throw InvalidImage();

        var root = Path.GetFullPath(Path.Combine(roots))
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        Directory.CreateDirectory(root);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var path = Path.GetFullPath(Path.Combine(root, fileName));
        if (!path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw InvalidImage();

        try
        {
            await using var output = new FileStream(
                path,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            await output.WriteAsync(signature.AsMemory(0, signatureLength), cancellationToken);
            await file.Content.CopyToAsync(output, cancellationToken);
            return fileName;
        }
        catch
        {
            if (File.Exists(path)) File.Delete(path);
            throw;
        }
    }

    public static bool CheckFileType(this FileUploadDTO file, string type) =>
        file.ContentType.StartsWith(type + "/", StringComparison.OrdinalIgnoreCase);

    private static bool HasValidSignature(string extension, ReadOnlySpan<byte> bytes) =>
        extension switch
        {
            ".jpg" or ".jpeg" => bytes.Length >= 3
                && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF,
            ".png" => bytes.Length >= 8
                && bytes[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            ".webp" => bytes.Length >= 12
                && bytes[..4].SequenceEqual("RIFF"u8)
                && bytes.Slice(8, 4).SequenceEqual("WEBP"u8),
            _ => false
        };

    private static ValidationException InvalidImage() => new(
        new Dictionary<string, string[]>
        {
            ["Image"] = ["Only genuine JPEG, PNG or WebP images up to 5 MB are allowed."]
        });
}
