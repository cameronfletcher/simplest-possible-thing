namespace CarTracker.Persistence.ReadModel
{
    using System.Collections.Generic;

    public interface ICarReadModelRepository
    {
        IEnumerable<CarItem> GetCars();
    }
}
