namespace CarTracker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using dddlib;

    public class Application
    {
        private readonly dddlib.Persistence.IEventStoreRepository eventStoreRepository;
        private readonly dddlib.Projections.IRepository<int, List<CarItem>> carListRepository;

        private Application(
            dddlib.Persistence.IEventStoreRepository eventStoreRepository,
            dddlib.Projections.IRepository<int, List<CarItem>> carListRepository)
        {
            this.eventStoreRepository = eventStoreRepository;
            this.carListRepository = carListRepository;
        }

        public static void Main()
        {
            var connectionString = @"Data Source=(localdb)\ProjectsV13;Initial Catalog=CarTracker;Integrated Security=True;";
            var eventStoreRepository = new dddlib.Persistence.SqlServer.SqlServerEventStoreRepository(connectionString);
            var carListRepository = new dddlib.Projections.Memory.MemoryRepository<int, List<CarItem>>();

            var view = new CarListView(carListRepository);
            var bus = new Microbus().AutoRegister(view);

            using (new dddlib.Persistence.EventDispatcher.SqlServer.SqlServerEventDispatcher(
                connectionString,
                (sequenceNumber, @event) => bus.Send(@event),
                Guid.NewGuid()))
            {
                new Application(eventStoreRepository, carListRepository).Run();
            }
        }

        public void Run()
        {
            Console.WriteLine("Car Tracker");

            var command = string.Empty;
            while (!string.Equals(command, "exit", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    this.Handle(command);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                Console.Write("> ");
                command = Console.ReadLine().Trim();
            }

            Console.WriteLine("Goodbye!");
        }

        private void Handle(string command)
        {
            var commandParts = command.Split(new[] { ' ' });
            switch (commandParts.First().ToLowerInvariant())
            {
                case "?":
                case "help":
                    Console.WriteLine(@"Commands:
    list               - lists all the registered cars
    register [reg]     - registers a car with the [reg]
    drive [reg] [dist] - drives the car with the [reg] the specified [dist]
    scrap [reg]        - scraps the car with the [reg]");
                    break;

                case "list":

                    // list cars
                    var carList = this.carListRepository.Get(0) ?? new List<CarItem>();
                    foreach (var car in carList)
                    {
                        Console.WriteLine("{0}: {1:G}km", car.Registration, car.TotalDistanceDriven);
                    }

                    break;

                case "register":
                    if (commandParts.Length < 2)
                    {
                        Console.WriteLine("Please specify the registration.");
                        break;
                    }

                    // register car: commandParts[1]
                    this.eventStoreRepository.Save(new Car(commandParts[1]));

                    Console.WriteLine("Registered {0}.", commandParts[1]);
                    break;

                case "drive":
                    if (commandParts.Length < 3)
                    {
                        Console.WriteLine("Please specify the registration and the distance.");
                        break;
                    }

                    var distance = 0;
                    if (!int.TryParse(commandParts[2], out distance))
                    {
                        Console.WriteLine("Distance must be a number.");
                        break;
                    }

                    // drive car: commandParts[1] distance
                    var carToDrive = this.eventStoreRepository.Load<Car>(commandParts[1]);
                    carToDrive.Drive(distance);
                    this.eventStoreRepository.Save(carToDrive);

                    Console.WriteLine("Drove {0} a distance of {1:G}km.", commandParts[1], distance);
                    break;

                case "scrap":
                    if (commandParts.Length < 2)
                    {
                        Console.WriteLine("Please specify the registration.");
                        break;
                    }

                    // scrap car: commandParts[1]
                    var carToScrap = this.eventStoreRepository.Load<Car>(commandParts[1]);
                    carToScrap.Scrap();

                    this.eventStoreRepository.Save(carToScrap);

                    Console.WriteLine("Scrapped {0}.", commandParts[1]);
                    break;

                case "":
                    break;

                default:
                    Console.WriteLine("Eh?");
                    break;
            }
        }
    }

    public class CarItem
    {
        public string Registration { get; set; }

        public int TotalDistanceDriven { get; set; }
    }

    public class CarListView
    {
        private readonly dddlib.Projections.IRepository<int, List<CarItem>> repository;

        public CarListView(dddlib.Projections.IRepository<int, List<CarItem>> repository)
        {
            Guard.Against.Null(() => repository);

            this.repository = repository;
        }

        public void Consume(CarRegistered @event)
        {
            var carList = this.repository.Get(0) ?? new List<CarItem>();
            carList.Add(new CarItem { Registration = @event.Registration });
            this.repository.AddOrUpdate(0, carList);
        }

        public void Consume(CarDriven @event)
        {
            var carList = this.repository.Get(0);
            var car = carList.Single(item => item.Registration == @event.Registration);
            car.TotalDistanceDriven += @event.Distance;
            this.repository.AddOrUpdate(0, carList);
        }

        public void Consume(CarScrapped @event)
        {
            var carList = this.repository.Get(0);
            carList.RemoveAll(item => item.Registration == @event.Registration);
            this.repository.AddOrUpdate(0, carList);
        }
    }

    public class Car : AggregateRoot
    {
        protected internal Car()
        {
        }

        public Car(string registration)
        {
            Guard.Against.Null(() => registration);

            this.Apply(new CarRegistered { Registration = registration });
        }

        [NaturalKey]
        public string Registration { get; private set; }

        public void Drive(int distance)
        {
            if (distance < 0)
            {
                throw new BusinessException("Cannot drive a negative distance.");
            }

            this.Apply(new CarDriven { Registration = this.Registration, Distance = distance });
        }

        public void Scrap()
        {
            this.Apply(new CarScrapped { Registration = this.Registration });
        }

        private void Handle(CarRegistered @event)
        {
            this.Registration = @event.Registration;
        }

        private void Handle(CarScrapped @event)
        {
            this.EndLifecycle();
        }
    }

    public class CarRegistered
    {
        public string Registration { get; set; }
    }

    public class CarDriven
    {
        public string Registration { get; set; }

        public int Distance { get; set; }
    }

    public class CarScrapped
    {
        public string Registration { get; set; }
    }
}