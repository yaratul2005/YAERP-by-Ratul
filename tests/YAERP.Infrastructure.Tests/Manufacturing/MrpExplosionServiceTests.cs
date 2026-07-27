using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using YAERP.Application.Common.Interfaces;
using YAERP.Infrastructure.Manufacturing;

namespace YAERP.Infrastructure.Tests.Manufacturing;

public class MrpExplosionServiceTests
{
    [Fact]
    public void RunMrpAsync_InstantiatesService()
    {
        var mockContext = new Mock<IApplicationDbContext>();
        var mockBomService = new Mock<IBomExplosionService>();
        var sut = new MrpExplosionService(mockContext.Object, mockBomService.Object);
        Assert.NotNull(sut);
    }
}
