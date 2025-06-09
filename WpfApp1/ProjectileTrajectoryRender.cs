using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

public class ProjectileTrajectoryRenderer
{
    private readonly Canvas _canvas;
    private readonly List<Ellipse> _projectilePoints = new List<Ellipse>();
    private readonly List<Rectangle> _targets = new List<Rectangle>();
    private readonly DispatcherTimer _animationTimer;
    private readonly Random _random = new Random();
    
    private double _time;
    private const double Gravity = 9.8; 
    private Point _currentPosition;
    private bool _isProjectileActive;
    
   
    public double InitialVelocity { get; set; } = 50;
    public double Angle { get; set; } = 45;
    public double ProjectileMass { get; set; } = 1.0; 
    public Point LaunchPoint { get; set; }
    
    public Brush ProjectileBrush { get; set; } = Brushes.Red;
    public double ProjectileSize { get; set; } = 8;
    public bool ShowFullTrajectory { get; set; } = true;
    public Brush TrajectoryBrush { get; set; } = Brushes.White;
    public double TrajectoryLineThickness { get; set; } = 1;
    
    // Настройки мишеней
    public int TargetCount { get; set; } = 5;
    public Size TargetSize { get; set; } = new Size(30, 30);
    public Brush TargetBrush { get; set; } = Brushes.Blue;
    
    // События
    public event Action<Point>? ProjectileLaunched;
    public event Action<Point>? TargetHit;
    public event Action? SimulationFinished;

    public ProjectileTrajectoryRenderer(Canvas canvas)
    {
        _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
        
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) 
        };
        _animationTimer.Tick += OnAnimationFrame;
        
        CreateTargets();
    }

    public void StartSimulation()
    {
        ClearTrajectory();
        _time = 0;
        _isProjectileActive = true;
        _animationTimer.Start();
        ProjectileLaunched?.Invoke(LaunchPoint);
    }

    public void StopSimulation()
    {
        _animationTimer.Stop();
        _isProjectileActive = false;
        SimulationFinished?.Invoke();
    }

    public void ClearTrajectory()
    {
        foreach (var point in _projectilePoints)
        {
            _canvas.Children.Remove(point);
        }
        _projectilePoints.Clear();
    }

    public void CreateTargets()
    {
        foreach (var target in _targets)
        {
            _canvas.Children.Remove(target);
        }
        _targets.Clear();
        
        for (int i = 0; i < TargetCount; i++)
        {
            var target = new Rectangle
            {
                Width = TargetSize.Width,
                Height = TargetSize.Height,
                Fill = TargetBrush,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                Tag = true 
            };
            
            double x = _random.Next((int)(_canvas.ActualWidth / 2), (int)(_canvas.ActualWidth - TargetSize.Width));
            double y = _random.Next(0, (int)(_canvas.ActualHeight - TargetSize.Height));
            
            Canvas.SetLeft(target, x);
            Canvas.SetTop(target, y);
            
            _canvas.Children.Add(target);
            _targets.Add(target);
        }
    }

    private void OnAnimationFrame(object sender, EventArgs e)
    {
        if (!_isProjectileActive) return;
        
        _time += 0.05;
        _currentPosition = CalculateProjectilePosition(_time);
        
        var canvasX = _currentPosition.X;
        var canvasY = _canvas.ActualHeight - _currentPosition.Y;
        
        if (canvasY > _canvas.ActualHeight || canvasX > _canvas.ActualWidth || canvasX < 0)
        {
            StopSimulation();
            return;
        }
        
        var projectilePoint = new Ellipse
        {
            Width = ProjectileSize,
            Height = ProjectileSize,
            Fill = ProjectileBrush,
            Stroke = Brushes.Black,
            StrokeThickness = 0.5
        };
        
        Canvas.SetLeft(projectilePoint, canvasX - ProjectileSize / 2);
        Canvas.SetTop(projectilePoint, canvasY - ProjectileSize / 2);
        
        _canvas.Children.Add(projectilePoint);
        _projectilePoints.Add(projectilePoint);
        
        if (ShowFullTrajectory && _projectilePoints.Count > 1)
        {
            var line = new Line
            {
                X1 = Canvas.GetLeft(_projectilePoints[_projectilePoints.Count - 2]) + ProjectileSize / 2,
                Y1 = Canvas.GetTop(_projectilePoints[_projectilePoints.Count - 2]) + ProjectileSize / 2,
                X2 = canvasX,
                Y2 = canvasY,
                Stroke = TrajectoryBrush,
                StrokeThickness = TrajectoryLineThickness
            };
            _canvas.Children.Add(line);
        }
        
        CheckCollisions(canvasX, canvasY);
    }

    private void CheckCollisions(double projectileX, double projectileY)
    {
        var projectileRect = new Rect(
            projectileX - ProjectileSize / 2,
            projectileY - ProjectileSize / 2,
            ProjectileSize,
            ProjectileSize);
        
        foreach (var target in _targets)
        {
            if (target.Tag as bool? != true) continue; 
            
            var targetRect = new Rect(
                Canvas.GetLeft(target),
                Canvas.GetTop(target),
                target.Width,
                target.Height);
            
            if (projectileRect.IntersectsWith(targetRect))
            {
                target.Tag = false;
                TargetHit?.Invoke(new Point(Canvas.GetLeft(target) + target.Width / 2, 
                                         Canvas.GetTop(target) + target.Height / 2));
                
                var fadeAnimation = new DoubleAnimation
                {
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.3),
                    FillBehavior = FillBehavior.Stop
                };
                
                fadeAnimation.Completed += (s, _) => _canvas.Children.Remove(target);
                target.BeginAnimation(UIElement.OpacityProperty, fadeAnimation);
                
            }
        }
    }

    private Point CalculateProjectilePosition(double time)
    {
        double angleRad = Angle * Math.PI / 180;
        double airResistanceCoefficient = 0.01 / ProjectileMass;
        
        double vx = InitialVelocity * Math.Cos(angleRad) * Math.Exp(-airResistanceCoefficient * time);
        double vy = InitialVelocity * Math.Sin(angleRad) * Math.Exp(-airResistanceCoefficient * time) - Gravity * time;
        
        double x = LaunchPoint.X + InitialVelocity * Math.Cos(angleRad) * 
                  (1 - Math.Exp(-airResistanceCoefficient * time)) / airResistanceCoefficient;
        
        double y = LaunchPoint.Y + (InitialVelocity * Math.Sin(angleRad) + Gravity / airResistanceCoefficient) *
                  (1 - Math.Exp(-airResistanceCoefficient * time)) / airResistanceCoefficient - 
                  Gravity * time / airResistanceCoefficient;
        
        return new Point(x, y);
    }
}