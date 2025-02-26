using System;
using System.IO;

class Calculate{
     double Angle { get; private set; } // Угол в градусах
   double Speed { get; private set; } // Скорость в м/с
   const double Gravity { get; private set; } = 9.81; // Ускорение свободного падения
  private string FileInput = "../input.txt";
  private string FileOutput = "../output.txt";

    
   double dx(double angle, double speed)
    {
        double t,vx, x;
        double angleRad = Angle * Math.PI / 180.0;

       vx = Speed * Math.Cos(angleRad);
       
       

        x = vx * t;
       

        return x;
    }

double dy(double angle, double speed){

       double vy,t,y;

       double angleRad = Angle * Math.PI / 180.0;

       double vy = Speed * Math.Sin(angleRad);
       t = (2 * vy) / Gravity;
       y = vy * t - 0.5 * Gravity * Math.Pow(t, 2);
        return y;
}|
}

class Program
{
    static void Main()
    {
       
        string[] lines = File.ReadAllLines(FileInput);
        double angle = double.Parse(lines[0]); 
        double speed = double.Parse(lines[1]); 

       
        Calculate calculate = new Calculate();


        // Вывод результатов
        Console.WriteLine($"Координаты полета снаряда:");
        Console.WriteLine($"Горизонтальная координата (дальность):"calculate.dx(angle,speed)" м");
        Console.WriteLine($"Вертикальная координата (высота):"calculate.dy(angle,speed)" м");
    }
}
