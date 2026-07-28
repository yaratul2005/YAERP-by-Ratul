using System;
using Xunit;
using YAERP.Infrastructure.Services;

namespace YAERP.Infrastructure.Tests.Services;

public class AutoUpdaterServiceTests
{
    [Theory]
    [InlineData("1.0.0", "1.0.1", true)]
    [InlineData("v1.0.0", "v1.0.1", true)]
    [InlineData("1.0.1", "1.0.0", false)]
    [InlineData("1.0.0", "1.0.0", false)]
    [InlineData("v1.1.0", "v2.0.0", true)]
    public void IsVersionNewer_ShouldCorrectlyCompareVersions(string current, string remote, bool expected)
    {
        var service = new AutoUpdaterService();
        var result = service.IsVersionNewer(current, remote);

        Assert.Equal(expected, result);
    }
}
