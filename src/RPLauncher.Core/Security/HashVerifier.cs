using System.Security.Cryptography;

namespace RPLauncher.Core.Security;

public static class HashVerifier
{
    public static async Task<string> ComputeSha256Async(string filePath, CancellationToken ct = default)
    {
        await using var stream = File.OpenRead(filePath);
        using var sha256 = SHA256.Create();
        var hashBytes = await sha256.ComputeHashAsync(stream, ct);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public static async Task<bool> VerifyAsync(string filePath, string expectedSha256, CancellationToken ct = default)
    {
        if (!File.Exists(filePath)) return false;
        if (string.IsNullOrWhiteSpace(expectedSha256)) return false;

        var actual = await ComputeSha256Async(filePath, ct);
        return string.Equals(actual, expectedSha256.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
