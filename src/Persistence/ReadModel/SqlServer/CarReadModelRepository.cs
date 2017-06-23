namespace CarTracker.Persistence.ReadModel.SqlServer
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using CarTracker.Persistence.ReadModel;

    // TODO (Cameron): Async.
    public sealed class CarReadModelRepository : ICarReadModelRepository
    {
        private const string DatabaseName = "Cars";

        private readonly string connectionString;
        private readonly string schema;

        public CarReadModelRepository(string connectionString, string schema = "dbo")
        {
            this.connectionString = connectionString;
            this.schema = schema;
        }

        public async Task<List<CarItem>> GetCarsAsync()
        {
            using (var connection = this.GetConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = $"{this.schema}.GetCars";

                await connection.OpenAsync().ConfigureAwait(false);

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    var cars = new List<CarItem>();

                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        cars.Add(
                            new CarItem
                            {
                                Registration = reader.GetString(0),
                                TotalDistanceTravelled = reader.GetInt32(1),
                            });
                    }

                    return cars;
                }
            }
        }

        private SqlConnection GetConnection() => new SqlConnection(this.connectionString).WithInitializedSchema(this.schema, DatabaseName);
    }
}
