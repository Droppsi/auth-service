using AuthService.Infrastructure.Database;

namespace AuthService.Tests;

public class BaseIntegrationTest(CustomWebAppFactory factory) : IClassFixture<CustomWebAppFactory>
{
    protected readonly HttpClient Client = factory.CreateClient();
    protected AppDbContext DbContext = factory.CreateDbContext();

    protected void RefreshDbContext()
    {
        DbContext.Dispose();
        DbContext = factory.CreateDbContext();
    }

    protected void ClearDb()
    {
        DbContext.Database.EnsureDeleted();
        DbContext.Database.EnsureCreated();
        RefreshDbContext();
    }
}