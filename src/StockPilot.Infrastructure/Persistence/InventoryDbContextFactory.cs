
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace StockPilot.Infrastructure.Persistence;

public class InventoryDbContextFactory
    : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        var currentDirectory = new DirectoryInfo(
            Directory.GetCurrentDirectory()
        );

        string? apiDirectory = null;

        // Search the current directory and its parent directories.
        // This works whether EF starts from the solution root,
        // API project, or Infrastructure project.
        for (var directory = currentDirectory;
             directory != null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(
                directory.FullName,
                "StockPilot.Api.csproj")))
            {
                apiDirectory = directory.FullName;
                break;
            }

            var possibleApiDirectory = Path.Combine(
                directory.FullName,
                "src",
                "StockPilot.Api"
            );

            if (File.Exists(Path.Combine(
                possibleApiDirectory,
                "StockPilot.Api.csproj")))
            {
                apiDirectory = possibleApiDirectory;
                break;
            }
        }

        if (apiDirectory is null)
        {
            throw new DirectoryNotFoundException(
                "Could not locate StockPilot.Api.csproj."
            );
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            configuration.GetConnectionString("Inventory");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:Inventory was not configured."
            );
        }

        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new InventoryDbContext(options);
    }
}
