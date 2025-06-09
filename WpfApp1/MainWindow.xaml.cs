using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AnimateMenuIn();
        }

        private void AnimateMenuIn()
        {
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(1),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            MainGrid.BeginAnimation(OpacityProperty, fadeIn);
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            var fadeOut = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            fadeOut.Completed += (s, _) => 
            {
                // Создаем новое игровое окно
                var gameWindow = new GameWindow();
                gameWindow.Closed += (s, e) => this.Close(); // Закрываем меню после закрытия игры
                gameWindow.Show();
                this.Hide(); // Скрываем меню вместо закрытия
            };

            MainGrid.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            var fadeOut = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            fadeOut.Completed += (s, _) => Application.Current.Shutdown();
            MainGrid.BeginAnimation(OpacityProperty, fadeOut);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}