using System.Threading;
using System.Threading.Tasks;

namespace YAERP.Application.Common.Interfaces;

public record GoogleUserInfoDto(
    string Id,
    string Email,
    string Name,
    string PictureUrl,
    bool IsConnected);

public interface IGoogleAuthService
{
    Task<GoogleUserInfoDto?> AuthenticateAsync(CancellationToken ct = default);
    Task DisconnectAsync(CancellationToken ct = default);
    Task<GoogleUserInfoDto?> GetCurrentUserInfoAsync(CancellationToken ct = default);
    bool IsConnected { get; }
}
