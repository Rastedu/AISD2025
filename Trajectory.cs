using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ProjectileMotion{
    class Projectile{
        private readonly double _initialVelocity; // Начальная скорость (м/с)
        private readonly double _angle;           // Угол (в рад)
        private readonly double G = 9.81;         

        Projectile(double initialVelocity, double angleInDegrees, double G){
            _initialVelocity = initialVelocity;
            _angle = DegreeToRadian(angleInDegrees);
            G = gravity;
        }

        
        static double DegToRad(double angle){
            return angle * Math.PI / 180.0;
        }

        
        double dX(double time){
            return _initialVelocity * Math.Cos(_angle) * time;
        }

       
        double dY(double time){
            return _initialVelocity * Math.Sin(_angle) * time - 0.5 * G * Math.Pow(time, 2);
        }

        (double x, double y) GetCoordinates(double time){
            return (dX(time), dY(time));
        }

        
        List<(double x, double y)> CalculateTrajectory(double timeStep, double maxTime){
            var trajectory = new List<(double x, double y)>();
            for (double t = 0; t <= maxTime; t += timeStep)
            {
                trajectory.Add(GetCoordinates(t));
            }
            return trajectory;
        }
    }

    class Program{
    static void Main(string[] args)
    {
        string inputFilePath = "input.txt";
        string outputFilePath = "output.txt";

        if (!File.Exists(inputFilePath))
        {
            Console.WriteLine($"Er 404");
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

            Projectile projectile = new Projectile(initialVelocity, angle);

            double timeStep = 0.1;
            double maxTime = 10;
            var trajectory = projectile.CalculateTrajectory(timeStep, maxTime);

            using (var writer = new StreamWriter(outputFilePath))
            {
                writer.WriteLine("X, Y");
                foreach (var point in trajectory) // Используем явное обращение к элементам кортежа
                {
                    double x = point.x;
                    double y = point.y;
                    writer.WriteLine($"{x:F2}, {y:F2}");
                }
            }

            Console.WriteLine($"Encode success");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Er: {ex.Message}");
        }
    }
    }
}
