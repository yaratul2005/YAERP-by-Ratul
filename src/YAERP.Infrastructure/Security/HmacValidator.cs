using System;
using System.Security.Cryptography;
using System.Text;
using YAERP.Application.Common.Security;

namespace YAERP.Infrastructure.Security;

public class HmacValidator : IHmacValidator
{
    public bool ValidateSignature(string payloadJson, string receivedHeaderSignature, string secretKey)
    {
        if (string.IsNullOrWhiteSpace(payloadJson) ||
            string.IsNullOrWhiteSpace(receivedHeaderSignature) ||
            string.IsNullOrWhiteSpace(secretKey))
        {
            return false;
        }

        byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payloadJson);

        using var hmac = new HMACSHA256(keyBytes);
        byte[] computedHash = hmac.ComputeHash(payloadBytes);

        // 1. Base64 signature comparison (Shopify style)
        string computedBase64 = Convert.ToBase64String(computedHash);
        byte[] base64Bytes = Encoding.UTF8.GetBytes(computedBase64);
        byte[] receivedBase64Bytes = Encoding.UTF8.GetBytes(receivedHeaderSignature.Trim());

        if (base64Bytes.Length == receivedBase64Bytes.Length &&
            CryptographicOperations.FixedTimeEquals(base64Bytes, receivedBase64Bytes))
        {
            return true;
        }

        // 2. Hex signature comparison (WooCommerce style)
        string computedHex = Convert.ToHexString(computedHash).ToLowerInvariant();
        string cleanReceivedHex = receivedHeaderSignature.Trim().ToLowerInvariant();

        byte[] hexBytes = Encoding.UTF8.GetBytes(computedHex);
        byte[] receivedHexBytes = Encoding.UTF8.GetBytes(cleanReceivedHex);

        return hexBytes.Length == receivedHexBytes.Length &&
               CryptographicOperations.FixedTimeEquals(hexBytes, receivedHexBytes);
    }
}
