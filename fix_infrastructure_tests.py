with open('tests/YAERP.Infrastructure.Tests/Security/IdentityServiceTests.cs', 'r') as f:
    content = f.read()

content = content.replace(
    'public void InvalidatePermissionCache_ShouldRemoveUserPermissions()',
    'public async Task InvalidatePermissionCache_ShouldRemoveUserPermissions()'
)
content = content.replace(
    'var hasPermission = identityService.EvaluatePermissionAsync(userId, permissionCode, CancellationToken.None).Result;',
    'var hasPermission = await identityService.EvaluatePermissionAsync(userId, permissionCode, CancellationToken.None);'
)

with open('tests/YAERP.Infrastructure.Tests/Security/IdentityServiceTests.cs', 'w') as f:
    f.write(content)
