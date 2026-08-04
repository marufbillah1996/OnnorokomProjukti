namespace AssignmentHub.Application.Common.Utilities;

/// <summary>
/// Plain fast SHA-256 hashing for refresh token values — deliberately NOT the slow password
/// hasher, since a refresh token is already 256 bits of random entropy, not a low-entropy
/// human password.
/// </summary>
public static class RefreshTokenHasher
{
    public static string Hash(string token) =>
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token)));
}
