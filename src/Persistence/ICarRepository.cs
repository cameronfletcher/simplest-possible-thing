namespace CarTracker.Persistence
{
    using CarTracker.Model;

    public interface ICarRepository
    {
        void Save(Car car);

        Car Load(string registration);
    }
}
