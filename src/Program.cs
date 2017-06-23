﻿namespace CarTracker
{
    using System;
    using System.Linq;
    using CarTracker.Model;
    using CarTracker.Persistence;
    using CarTracker.Persistence.ReadModel;
    using CarTracker.Persistence.ReadModel.SqlServer;
    using CarTracker.Persistence.SqlServer;

    public class Application
    {
        private readonly ICarRepository repository;
        private readonly ICarReadModelRepository readModelRepository;

        private Application(
            ICarRepository carRepository,
            ICarReadModelRepository carReadModelRepository)
        {
            this.repository = carRepository;
            this.readModelRepository = carReadModelRepository;
        }

        public static void Main()
        {
            var connectionString = @"Data Source=(localdb)\ProjectsV13;Initial Catalog=CarTracker;Integrated Security=True;";
            new Application(new CarRepository(connectionString), new CarReadModelRepository(connectionString)).Run();
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
                    var carList = this.readModelRepository.GetCars();
                    foreach (var car in carList)
                    {
                        Console.WriteLine("{0}: {1:G}km", car.Registration, car.TotalDistanceTravelled);
                    }

                    break;

                case "register":
                    if (commandParts.Length < 2)
                    {
                        Console.WriteLine("Please specify the registration.");
                        break;
                    }

                    // register car: commandParts[1]
                    this.repository.Save(new Car(commandParts[1]));

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
                    var carToDrive = this.repository.Load(commandParts[1]);
                    carToDrive.Drive(distance);
                    this.repository.Save(carToDrive);

                    Console.WriteLine("Drove {0} a distance of {1:G}km.", commandParts[1], distance);
                    break;

                case "scrap":
                    if (commandParts.Length < 2)
                    {
                        Console.WriteLine("Please specify the registration.");
                        break;
                    }

                    // scrap car: commandParts[1]
                    var carToScrap = this.repository.Load(commandParts[1]);
                    carToScrap.Scrap();

                    this.repository.Save(carToScrap);

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
}