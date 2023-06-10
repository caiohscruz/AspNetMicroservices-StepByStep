﻿using Npgsql;

namespace Discount.API.Extensions
{
    public static class HostExtensions
    {
        public static IHost MigrateDatabase<TContext>(this IHost host, int? retry = 0)
        {
            int retryForAvailability = retry.Value;

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var configuration = services.GetRequiredService<IConfiguration>();
                var logger = services.GetRequiredService<ILogger<TContext>>();

                try
                {
                    logger.LogInformation("Migrating postresql database.");

                    using var connection = new NpgsqlConnection
                        (configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
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

                    logger.LogInformation("Migrated postresql database.");
                }
                catch (NpgsqlException ex)
                {
                    logger.LogError(ex, "An error occurred while migrating the postresql database");

                    if (retryForAvailability < 50)
                    {
                        retryForAvailability++;
                        System.Threading.Thread.Sleep(2000);
                        MigrateDatabase<TContext>(host, retryForAvailability);
                    }
                }
            }

            return host;
        }
    }
}
