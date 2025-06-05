using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Drawing.Color;
using Point = System.Windows.Point;


namespace WpfApp1;

public partial class Task : Window
{
    private readonly Canvas _canvas;
    private readonly List<Ellipse> _projectilePoints = new List<Ellipse>();
    private readonly List<Rectangle> _targets = new List<Rectangle>();
    private readonly DispatcherTimer _animationTimer;
    private readonly Random _random = new Random();
    
    // Границы с текстурами
    private Image _topBorder;
    private Image _bottomBorder;
    private Image _background;
    
    // Звуки
    private SoundPlayer _launchSound;
    private SoundPlayer _hitSound;
    private SoundPlayer _targetsound;

    private double _time;
    private const double Gravity = 9.8;
    private Point _currentPosition;
    private bool _isProjectileActive;
    
    // Физические параметры
    public double InitialVelocity { get; set; } = 50;
    public double Angle { get; set; } = 45;
    public double ProjectileMass { get; set; } = 1.0;
    public Point LaunchPoint { get; set; }
    
    // Визуальные настройки
    public Brush ProjectileBrush { get; set; } = Brushes.Red;
    public double ProjectileSize { get; set; } = 8;
    public bool ShowFullTrajectory { get; set; } = true;
    public Brush TrajectoryBrush { get; set; } = Brushes.White;
    public double TrajectoryLineThickness { get; set; } = 1;
    
    // Настройки мишеней
    public int TargetCount { get; set; } = 5;
    public Size TargetSize { get; set; } = new Size(30, 30);
    public Brush TargetBrush { get; set; } = Brushes.Blue;
    
    // Пути к ресурсам
    public string BackgroundImagePath { get; set; } = "background.jpg";
    public string TopBorderImagePath { get; set; } = "top_border.png";
    public string BottomBorderImagePath { get; set; } = "bottom_border.png";
    public string LaunchSoundPath { get; set; } = "launch.wav";
    public string HitSoundPath { get; set; } = "hit.wav";
    
    public string TargetSoundPath { get; set; } = "target.mp3";
    // События
    public event Action<Point> ProjectileLaunched;
    public event Action<Point> TargetHit;
    public event Action SimulationFinished;
    private ProjectileTrajectoryRenderer _trajectoryRenderer;
    
    public string WinSoundPath { get; set; } = "end.mp3";
    public string WinMessage { get; set; } = "Победа! Все мишени поражены!";
    public Brush WinMessageBrush { get; set; } = Brushes.Gold;
    public double WinMessageFontSize { get; set; } = 36;
    
    private SoundPlayer _winSound;
    private int _activeTargetsCount;
    
     public Task(Canvas canvas)
     {
         _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
         _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _animationTimer.Tick += OnAnimationFrame;
        
        
        try
        {
            _launchSound = new SoundPlayer(LaunchSoundPath);
            _hitSound = new SoundPlayer(HitSoundPath);
            _targetsound = new SoundPlayer(TargetSoundPath);
            _winSound = new SoundPlayer(WinSoundPath);
        }
        catch
        {
            
            _launchSound = null;
            _hitSound = null;
            _targetsound = null;
            _winSound = null;
        }
        
        
        InitializeGraphics();
        CreateTargets();
        _targetsound?.Play();
    }

    
     private void InitializeGraphics()
    {
        
        _canvas.Children.Clear();
        
        
        _background = new Image
        {
            Stretch = Stretch.UniformToFill,
            Opacity = 0.8
        };
        
        try
        {
            _background.Source = new BitmapImage(new Uri(BackgroundImagePath, UriKind.RelativeOrAbsolute));
        }
        catch
        {
            _background.Source = CreateFallbackBackground();
        }
        
        _canvas.Children.Add(_background);
        
        
        _topBorder = new Image
        {
            Stretch = Stretch.Uniform,
            VerticalAlignment = VerticalAlignment.Top,
            Height = 60
        };
        
        try
        {
            _topBorder.Source = new BitmapImage(new Uri(TopBorderImagePath, UriKind.RelativeOrAbsolute));
        }
        catch
        {
            _topBorder.Source = CreateFallbackBorder(true);
        }
        
        _canvas.Children.Add(_topBorder);
        
       
        _bottomBorder = new Image
        {
            Stretch = Stretch.Uniform,
            VerticalAlignment = VerticalAlignment.Bottom,
            Height = 80
        };
        
        try
        {
            _bottomBorder.Source = new BitmapImage(new Uri(BottomBorderImagePath, UriKind.RelativeOrAbsolute));
        }
        catch
        {
            _bottomBorder.Source = CreateFallbackBorder(false);
        }
        
        _canvas.Children.Add(_bottomBorder);
        
        
        _canvas.SizeChanged += (s, e) => UpdateGraphicsLayout();
        UpdateGraphicsLayout();
    }

    private void UpdateGraphicsLayout()
    {
        
        _background.Width = _canvas.ActualWidth;
        _background.Height = _canvas.ActualHeight;
        
        
        Canvas.SetLeft(_topBorder, 0);
        Canvas.SetTop(_topBorder, 0);
        _topBorder.Width = _canvas.ActualWidth;
        
        
        Canvas.SetLeft(_bottomBorder, 0);
        Canvas.SetTop(_bottomBorder, _canvas.ActualHeight - _bottomBorder.Height);
        _bottomBorder.Width = _canvas.ActualWidth;
    }

    private BitmapSource CreateFallbackBackground()
    {
        
        var drawingVisual = new DrawingVisual();
        using (var drawingContext = drawingVisual.RenderOpen())
        {
            var gradient = new LinearGradientBrush(
                Colors.DarkBlue, Colors.LightSkyBlue, 
                new Point(0, 0), new Point(0, 1));
            
            drawingContext.DrawRectangle(
                gradient, null, 
                new Rect(0, 0, 800, 600));
        }
        
        var renderTarget = new RenderTargetBitmap(
            800, 600, 96, 96, PixelFormats.Pbgra32);
        renderTarget.Render(drawingVisual);
        
        return renderTarget;
    }

    private BitmapSource CreateFallbackBorder(bool isTop)
    {
        
        var color = isTop ? Colors.DarkGreen : Colors.SaddleBrown;
        var drawingVisual = new DrawingVisual();
        
        using (var drawingContext = drawingVisual.RenderOpen())
        {
            drawingContext.DrawRectangle(
                new SolidColorBrush(color), null, 
                new Rect(0, 0, 800, isTop ? 60 : 80));
            
            
            var pen = new Pen(new SolidColorBrush(Colors.Black), 1);
            for (int i = 0; i < 800; i += 20)
            {
                drawingContext.DrawLine(pen, 
                    new Point(i, isTop ? 60 : 0), 
                    new Point(i + 10, isTop ? 0 : 80));
            }
        }
        
        var renderTarget = new RenderTargetBitmap(
            800, isTop ? 60 : 80, 96, 96, PixelFormats.Pbgra32);
        renderTarget.Render(drawingVisual);
        
        return renderTarget;
    }

    public void StartSimulation()
    {
        ClearTrajectory();
        _time = 0;
        _isProjectileActive = true;
        
        
        _launchSound?.Play();
        
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
        
        _activeTargetsCount = TargetCount;
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
            if (target.Tag as bool? != true) continue; // Пропускаем неактивные мишени
            
            var targetRect = new Rect(
                Canvas.GetLeft(target),
                Canvas.GetTop(target),
                target.Width,
                target.Height);
            
            if (projectileRect.IntersectsWith(targetRect))
            {
                // Попадание в мишень
                target.Tag = false;
                TargetHit?.Invoke(new Point(Canvas.GetLeft(target) + target.Width / 2, 
                                         Canvas.GetTop(target) + target.Height / 2));
                
                // Анимация исчезновения мишени
                var fadeAnimation = new DoubleAnimation
                {
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.3),
                    FillBehavior = FillBehavior.Stop
                };
                
                fadeAnimation.Completed += (s, _) => _canvas.Children.Remove(target);
                target.BeginAnimation(UIElement.OpacityProperty, fadeAnimation);
                
                // Можно добавить эффект взрыва или частиц здесь
            }
        }
    }

    private Point CalculateProjectilePosition(double time)
    {
        double angleRad = Angle * Math.PI / 180;
        
        // Учет массы снаряда (чем больше масса, тем меньше влияние сопротивления воздуха)
        // Простая модель сопротивления воздуха, пропорциональная массе
        double airResistanceCoefficient = 0.01 / ProjectileMass;
        
        // Компоненты скорости с учетом сопротивления воздуха
        double vx = InitialVelocity * Math.Cos(angleRad) * Math.Exp(-airResistanceCoefficient * time);
        double vy = InitialVelocity * Math.Sin(angleRad) * Math.Exp(-airResistanceCoefficient * time) - Gravity * time;
        
        // Позиция с учетом сопротивления воздуха (интегрирование скорости)
        double x = LaunchPoint.X + InitialVelocity * Math.Cos(angleRad) * 
                  (1 - Math.Exp(-airResistanceCoefficient * time)) / airResistanceCoefficient;
        
        double y = LaunchPoint.Y + (InitialVelocity * Math.Sin(angleRad) + Gravity / airResistanceCoefficient) *
                  (1 - Math.Exp(-airResistanceCoefficient * time)) / airResistanceCoefficient - 
                  Gravity * time / airResistanceCoefficient;
        
        return new Point(x, y);
    }
   
    
    private void OnTargetHit(Point hitPosition)
    {
        _activeTargetsCount--;
        
        
        if (_activeTargetsCount <= 0)
        {
            ShowWinMessage();
        }
        
        var explosion = new Ellipse
        {
            Width = 60,
            Height = 60,
            Fill = new RadialGradientBrush(Colors.Yellow, Colors.Red),
            Opacity = 0.7
        };
        
        Canvas.SetLeft(explosion, hitPosition.X - 30);
        Canvas.SetTop(explosion, hitPosition.Y - 30);
        MainCanvas.Children.Add(explosion);
        
        // Анимация взрыва
        var anim = new DoubleAnimation
        {
            To = 0,
            Duration = TimeSpan.FromSeconds(0.5),
            FillBehavior = FillBehavior.Stop
        };
        
        anim.Completed += (s, _) => MainCanvas.Children.Remove(explosion);
        explosion.BeginAnimation(UIElement.OpacityProperty, anim);
    }
    
    private void OnSimulationFinished()
    {
        // Можно добавить логику по завершению симуляции
    }
    
    private void VelocitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_trajectoryRenderer != null)
        {
            _trajectoryRenderer.InitialVelocity = e.NewValue;
        }
    }

    private void AngleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_trajectoryRenderer != null)
        {
            _trajectoryRenderer.Angle = e.NewValue;
        }
    }
    
    private void ShowWinMessage()
    {
        // Останавливаем симуляцию
        StopSimulation();
        
        // Воспроизводим звук победы
        _winSound?.Play();
        
        // Создаем сообщение о победе
        var winText = new TextBlock
        {
            Text = WinMessage,
            Foreground = WinMessageBrush,
            FontSize = WinMessageFontSize,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            
        };
        
        // Создаем полупрозрачный фон для сообщения
        var winBackground = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0)),
            CornerRadius = new CornerRadius(10),
            Width = _canvas.ActualWidth * 0.7,
            Height = _canvas.ActualHeight * 0.3,
            Child = winText
        };
        
        // Создаем контейнер для центрирования
        var winContainer = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        winContainer.Children.Add(winBackground);
        
        // Добавляем на Canvas
        _canvas.Children.Add(winContainer);
        Canvas.SetLeft(winContainer, (_canvas.ActualWidth - winBackground.Width) / 2);
        Canvas.SetTop(winContainer, (_canvas.ActualHeight - winBackground.Height) / 2);
        
        // Анимация появления
        var anim = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromSeconds(0.5),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        
        winContainer.BeginAnimation(UIElement.OpacityProperty, anim);
        
        // Через 5 секунд убираем сообщение
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        timer.Tick += (s, e) => 
        {
            timer.Stop();
            var fadeAnim = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromSeconds(0.5),
                FillBehavior = FillBehavior.Stop
            };
            
            fadeAnim.Completed += (ss, ee) => _canvas.Children.Remove(winContainer);
            winContainer.BeginAnimation(UIElement.OpacityProperty, fadeAnim);
        };
        timer.Start();
    }
}