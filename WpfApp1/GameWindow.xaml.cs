using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public partial class GameWindow : Window
    {
        private ProjectileTrajectoryRenderer _trajectoryRenderer;

        public GameWindow()
        {
            InitializeComponent();
            Loaded += GameWindow_Loaded;
        }

        private void GameWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Убеждаемся, что Canvas проинициализирован
                GameCanvas.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                GameCanvas.Arrange(new Rect(0, 0, GameCanvas.DesiredSize.Width, GameCanvas.DesiredSize.Height));

                // Инициализируем рендерер траектории
                _trajectoryRenderer = new ProjectileTrajectoryRenderer(GameCanvas)
                {
                    LaunchPoint = new Point(50, GameCanvas.ActualHeight - 50),
                    InitialVelocity = 70,
                    Angle = 45,
                    ProjectileMass = 1.5
                };

                // Создаем мишени
                _trajectoryRenderer.CreateTargets();
                
                // Запускаем симуляцию
                _trajectoryRenderer.StartSimulation();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Game initialization error: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _trajectoryRenderer?.StopSimulation();
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            _trajectoryRenderer?.StopSimulation();
            base.OnClosed(e);
        }
    }
}