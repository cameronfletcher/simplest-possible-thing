namespace CarTracker.Model
{
    /// <summary>
    /// Represents a car.
    /// </summary>
    public partial class Car
    {
        private int totalDistanceTravelled;
        private bool isDestroyed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Car"/> class.
        /// </summary>
        //// NOTE (Cameron): Designed for use by the repository to instantiate an uninitialized instance of a car -or- by any future implementation that inherits from a type
        //// of Car in order for that implementation to provide the same functionality to the repository.
        protected internal Car()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Car"/> class.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <exception cref="BusinessException">A car registration cannot exceed more than 50 characters in length.</exception>
        public Car(string registration)
        {
            Guard.Against.Null(() => registration);

            if (registration.Length > 50)
            {
                throw new BusinessException("A car registration cannot exceed more than 50 characters in length.");
            }

            this.Registration = registration;
        }

        /// <summary>
        /// Gets the car registration.
        /// </summary>
        /// <value>The car registration.</value>
        public string Registration { get; private set; }

        /// <summary>
        /// Registers the car as having been driven the specified distance.
        /// </summary>
        /// <param name="distance">The distance.</param>
        /// <exception cref="BusinessException">A car cannot be driven a negative distance.</exception>
        public void Drive(int distance)
        {
            if (this.isDestroyed)
            {
                throw new BusinessException($"Cannot apply changes to '{this.Registration}' because that '{this.GetType().Name}' no longer exists in the system.");
            }

            if (distance < 0)
            {
                throw new BusinessException("A car cannot be driven a negative distance.");
            }

            this.totalDistanceTravelled += distance;
        }

        /// <summary>
        /// Marks the car as having been scrapped.
        /// </summary>
        public void Scrap() => this.isDestroyed = true;
    }
}
