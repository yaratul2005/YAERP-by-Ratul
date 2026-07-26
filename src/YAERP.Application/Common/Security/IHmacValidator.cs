namespace YAERP.Application.Common.Security;

public interface IHmacValidator
{
    bool ValidateSignature(string payloadJson, string receivedHeaderSignature, string secretKey);
}
