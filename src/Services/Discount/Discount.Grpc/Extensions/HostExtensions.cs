using Npgsql;
using Polly;

namespace Discount.Grpc.Extensions
{
    public static class HostExtensions
    {
        public static IHost MigrateDatabase<TContext>(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var configuration = services.GetRequiredService<IConfiguration>();
                var logger = services.GetRequiredService<ILogger<TContext>>();

                try
                {
                    logger.LogInformation("Migrating postresql database.");

                    var retry = Policy.Handle<NpgsqlException>()
                            .WaitAndRetry(
                                retryCount: 5,
                                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // 2,4,8,16,32 sc
                                onRetry: (exception, retryCount, context) =>
                                {
                                    logger.LogError($"Retry {retryCount} of {context.PolicyKey} at {context.OperationKey}, due to: {exception}.");
                                });

                    //if the postgresql server container is not created on run docker compose this
                    //migration can't fail for network related exception. The retry options for database operations
                    //apply to transient exceptions                    
                    retry.Execute(() => ExecuteMigrations(configuration));

                    logger.LogInformation("Migrated postresql database.");
                }
                catch (NpgsqlException ex)
                {
                    logger.LogError(ex, "An error occurred while migrating the postresql database");                  
                }
            }

            return host;
        }
        private static void ExecuteMigrations(IConfiguration configuration)
        {
            using var connection = new NpgsqlConnection(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
            connection.Open();

            using var command = new NpgsqlCommand
            {
                Connection = connection
            };

            command.CommandText = @"
                        SELECT EXISTS (
                            SELECT 1
                            FROM pg_tables
                            WHERE schemaname = 'public'
                            AND tablename = 'coupon'
                        )";

            var tableExists = (bool)command.ExecuteScalar();

            if (!tableExists)
            {
                command.CommandText = @"
                            CREATE TABLE Coupon (
                                Id SERIAL PRIMARY KEY,
                                ProductName VARCHAR(24) NOT NULL,
                                Description TEXT,
                                Amount INT
                            );

                            INSERT INTO Coupon(ProductName, Description, Amount) 
                            VALUES 
                                ('IPhone X', 'IPhone Discount', 150),
                                ('Samsung 10', 'Samsung Discount', 100);";

                command.ExecuteNonQuery();
            }
        }
    }
}
