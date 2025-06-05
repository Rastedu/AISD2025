using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private ProjectileTrajectoryRenderer _trajectoryRenderer;
    private Canvas _gameCanvas;
    
    public MainWindow()
    {
        InitializeComponent();
        Loaded += (s, e) => AnimateMenuIn();
        _gameCanvas = new Canvas
        {
            Visibility = Visibility.Collapsed
        };
        MainGrid.Children.Add(_gameCanvas);
        
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
            
            var translate = new ThicknessAnimation
            {
                From = new Thickness(0, 50, 0, 0),
                To = new Thickness(0),
                Duration = TimeSpan.FromSeconds(1),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            
            MainGrid.BeginAnimation(OpacityProperty, fadeIn);
            //MenuStackPanel.BeginAnimation(MarginProperty, translate);
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
                
                MessageBox.Show("Starting the game...");
                new Task(_gameCanvas).Show();
                Close();
            };
            
            MainGrid.BeginAnimation(OpacityProperty, fadeOut);
        }
        
        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
           
            var optionsWindow = new OptionsWindow();
            optionsWindow.Owner = this;
            optionsWindow.ShowDialog();
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
            Close();
            MainGrid.BeginAnimation(OpacityProperty, fadeOut);
        }
        
        
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
    }

