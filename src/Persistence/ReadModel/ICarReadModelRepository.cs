namespace CarTracker.Persistence.ReadModel
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ICarReadModelRepository
    {
        Task<List<CarItem>> GetCarsAsync();
    }
}
