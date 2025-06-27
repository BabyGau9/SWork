using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SWork.Data.Models;

namespace SWork.Common.Helper
{
    public static class MigrationHelper
    {
        public static async Task EnsureDatabaseMigratedAsync(SWorkDbContext dbContext, ILogger logger)
        {
            try
            {
                // Check if database exists
                if (!await dbContext.Database.CanConnectAsync())
                {
                    logger.LogInformation("Database does not exist. Creating database...");
                    await dbContext.Database.EnsureCreatedAsync();
                    logger.LogInformation("Database created successfully!");
                    return;
                }

                // Check for pending migrations
                var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
                var pendingList = pendingMigrations.ToList();

                if (pendingList.Any())
                {
                    logger.LogInformation("Found {Count} pending migrations: {Migrations}", 
                        pendingList.Count, string.Join(", ", pendingList));

                    // Apply migrations
                    await dbContext.Database.MigrateAsync();
                    logger.LogInformation("All migrations applied successfully!");
                }
                else
                {
                    logger.LogInformation("Database is up to date. No pending migrations.");
                }

                // Log current migration status
                var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync();
                logger.LogInformation("Current database version: {LatestMigration}", 
                    appliedMigrations.LastOrDefault() ?? "None");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during database migration: {Message}", ex.Message);
                throw;
            }
        }

        public static async Task<bool> TestDatabaseConnectionAsync(SWorkDbContext dbContext, ILogger logger, int maxRetries = 5)
        {
            var retryCount = 0;
            var delayMs = 2000;

            while (retryCount < maxRetries)
            {
                try
                {
                    if (await dbContext.Database.CanConnectAsync())
                    {
                        logger.LogInformation("Database connection test successful!");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning("Database connection test failed (attempt {Attempt}/{MaxAttempts}): {Message}", 
                        retryCount + 1, maxRetries, ex.Message);
                }

                retryCount++;
                if (retryCount < maxRetries)
                {
                    logger.LogWarning("Retrying database connection in {Delay} seconds...", delayMs / 1000);
                    await Task.Delay(delayMs);
                    delayMs = Math.Min(delayMs * 2, 10000); // Exponential backoff
                }
            }

            logger.LogError("Database connection test failed after {MaxRetries} attempts", maxRetries);
            return false;
        }
    }
} 