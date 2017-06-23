namespace CarTracker.Persistence.ReadModel.SqlServer
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
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

        public IEnumerable<CarItem> GetCars()
        {
            using (var connection = this.GetConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = $"{this.schema}.GetCars";

                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return new CarItem
                        {
                            Registration = reader.GetString(0),
                            TotalDistanceTravelled = reader.GetInt32(1),
                        };
                    }
                }
            }
        }

        private SqlConnection GetConnection() => new SqlConnection(this.connectionString).WithInitializedSchema(this.schema, DatabaseName);
    }
}
