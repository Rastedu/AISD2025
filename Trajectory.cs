using System;
using System.Collections.Generic;
using System.IO;

namespace ProjectileMotion
{
    class Projectile
    {
        readonly double _initialVelocity; // Начальная скорость (м/с)
        readonly double _angle;           // Угол (в рад)
        double G = 9.81;

        public delegate void CoordinatesCalculatedHandler(double x, double y);
        public event CoordinatesCalculatedHandler CoordinatesCalculated;

        public Projectile(double initialVelocity, double angleInDegrees, double G)
        {
            _initialVelocity = initialVelocity;
            _angle = DegToRad(angleInDegrees);
        }

        static double DegToRad(double angle)
        {
            return angle * Math.PI / 180.0;
        }

        double dX(double time)
        {
            return _initialVelocity * Math.Cos(_angle) * time;
        }

        double dY(double time)
        {
            return _initialVelocity * Math.Sin(_angle) * time - 0.5 * G * Math.Pow(time, 2);
        }

        (double x, double y) GetCoordinates(double time)
        {
            (double x, double y) coordinates = (dX(time), dY(time));
            OnCoordinatesCalculated(coordinates.x, coordinates.y); 
            return coordinates;
        }

         public virtual void OnCoordinatesCalculated(double x, double y)
        {
            CoordinatesCalculated?.Invoke(x, y); 
        }

        public List<(double x, double y)> CalculateTrajectory(double timeStep, double maxTime)
        {
            var trajectory = new List<(double x, double y)>();
            for (double t = 0; t <= maxTime; t += timeStep)
            {
                trajectory.Add(GetCoordinates(t));
            }
            return trajectory;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string inputFilePath = "input.txt";
            string outputFilePath = "output.txt";
            double G = 9.81;

            if (!File.Exists(inputFilePath))
            {
                Console.WriteLine($"Error 404: Input file not found.");
                return;
            }

            try
            {
                var lines = File.ReadAllLines(inputFilePath);
                if (lines.Length < 2)
                {
                    Console.WriteLine("Incorrect input");
                    return;
                }

                double angle = double.Parse(lines[0]);
                double initialVelocity = double.Parse(lines[1]);

                Projectile projectile = new Projectile(initialVelocity, angle, G);

                projectile.CoordinatesCalculated += (x, y) =>
                {
                    Console.WriteLine($"Coordinates calculated: X = {x:F2}, Y = {y:F2}");
                };

                double timeStep = 0.1;
                double maxTime = 10;
                var trajectory = projectile.CalculateTrajectory(timeStep, maxTime);

                using (var writer = new StreamWriter(outputFilePath))
                {
                    writer.WriteLine("X, Y");
                    foreach (var point in trajectory)
                    {
                        
                        double x = point.x;
                        double y = point.y;
                        writer.WriteLine($"{x:F2}, {y:F2}");
                    }
                }

                Console.WriteLine("Encoding success");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}