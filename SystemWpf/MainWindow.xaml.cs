using System.Configuration;
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
using SystemWpf.Views;

namespace SystemWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly IRegionManager _regionManager;
        public MainWindow(IRegionManager regionManager)
        {
            InitializeComponent();
            _regionManager=regionManager;
            this.MouseLeftButtonUp += (s, e) => AutoSnap();
        }

        private void Drag(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _regionManager.RequestNavigate("ControlMain", nameof(Main));
         
        }


        private void AutoSnap()
        {
            var screenWidth = SystemParameters.WorkArea.Width;

            if (Left <= 5)
            {
                // 吸左边
                AnimateTo(-Width + 20);
            }
            else if (Left + Width >= screenWidth - 5)
            {
                // 吸右边
                AnimateTo(screenWidth - 20);
            }
        }

        private void AnimateTo(double targetX)
        {
            var anim = new DoubleAnimation
            {
                To = targetX,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase()
            };

            BeginAnimation(Window.LeftProperty, anim);
        }
    }
}