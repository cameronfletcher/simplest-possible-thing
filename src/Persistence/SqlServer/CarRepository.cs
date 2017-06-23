namespace CarTracker.Persistence.SqlServer
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;
    using CarTracker.Model;

    public sealed class CarRepository : ICarRepository
    {
        private const string DatabaseName = "Cars";

        private readonly string connectionString;
        private readonly string schema;

        public CarRepository(string connectionString, string schema = "dbo")
        {
            this.connectionString = connectionString;
            this.schema = schema;
        }

        public async Task SaveAsync(Car car)
        {
            Guard.Against.Null(() => car);

            var memento = car.GetState();

            using (var connection = this.GetConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = $"{this.schema}.SaveCar";
                command.Parameters.Add("@Registration", SqlDbType.VarChar, 50).Value = memento.Registration;
                command.Parameters.Add("@TotalDistanceTravelled", SqlDbType.Int).Value = memento.TotalDistanceTravelled;
                command.Parameters.Add("@IsDestroyed", SqlDbType.Bit).Value = memento.IsDestroyed;
                command.Parameters.Add("@__State", SqlDbType.VarChar, 8).Value = (object)memento.State ?? DBNull.Value;
                command.Parameters.Add("@__StateOut", SqlDbType.VarChar, 8).Direction = ParameterDirection.Output;

                await connection.OpenAsync().ConfigureAwait(false);

                try
                {
                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
                catch (SqlException ex) when (ex.Errors.Cast<SqlError>().Any(sqlError => sqlError.Number == 50409))
                {
                    if (memento.State == null)
                    {
                        throw new PersistenceException("Car already exists.", ex);
                    }
                    else
                    {
                        throw new PersistenceException("A concurrency error has occurred.", ex);
                    }
                }

                memento.State = Convert.ToString(command.Parameters["@__StateOut"].Value);
            }
        }

        public async Task<Car> LoadAsync(string registration)
        {
            Guard.Against.Null(() => registration);

            using (var connection = this.GetConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = $"{this.schema}.LoadCar";
                command.Parameters.Add("@Registration", SqlDbType.VarChar, 50).Value = registration;

                await connection.OpenAsync().ConfigureAwait(false);

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    if (!await reader.ReadAsync().ConfigureAwait(false))
                    {
                        throw new PersistenceException($"Cannot find any car with registration '{registration}'.");
                    }

                    var car = new Car();

                    car.SetState(
                        new Car.Memento
                        {
                            Registration = reader.GetString(0),
                            TotalDistanceTravelled = reader.GetInt32(1),
                            State = reader.GetString(2),
                        });

                    return car;
                }
            }
        }

        private SqlConnection GetConnection() => new SqlConnection(this.connectionString).WithInitializedSchema(this.schema, DatabaseName);
    }
}
