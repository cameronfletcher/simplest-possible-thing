namespace CarTracker.Persistence
{
    using System.Threading.Tasks;
    using CarTracker.Model;

    public interface ICarRepository
    {
        Task SaveAsync(Car car);

        Task<Car> LoadAsync(string registration);
    }
}
