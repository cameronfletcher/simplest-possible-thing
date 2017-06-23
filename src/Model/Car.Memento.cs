namespace CarTracker.Model
{
    // NOTE (Cameron): This has to be visible to the repository layer. For full seperation of concerns introduce a public facory type that has visibility.
    public partial class Car
    {
        private string state;

        protected internal virtual Memento GetState()
        {
            return new Memento
            {
                Registration = this.Registration,
                TotalDistanceTravelled = this.totalDistanceTravelled,
                IsDestroyed = this.isDestroyed,
                State = this.state,
            };
        }

        protected internal virtual void SetState(Memento memento)
        {
            // NOTE (Cameron): No need to set 'IsDestroyed' as we can never reconstitue a car that no longer exists.
            this.Registration = memento.Registration;
            this.totalDistanceTravelled = memento.TotalDistanceTravelled;
            this.state = memento.State;
        }

        protected internal class Memento
        {
            public string Registration { get; set; }

            public int TotalDistanceTravelled { get; set; }

            public bool IsDestroyed { get; set; }

            public string State { get; set; }
        }
    }
}
