using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace SystemWpf.Views
{
    /// <summary>
    /// Main.xaml 的交互逻辑
    /// </summary>
    public partial class Main : UserControl
    {
        private List<Particle> _particles = new();
        private Random _random = new();
        public Main()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                var sb = (Storyboard)FindResource("FlowAnimation");
                sb.Begin(GlowBorder);

                StartParticles();
            };
            this.MouseMove += OnMouseMove;
            this.MouseDown += (s, e) => BurstEffect(e.GetPosition(ParticleCanvas));
         
        }

        private void StartParticles()
        {
            CompositionTarget.Rendering += OnRender;
        }
        private void OnRender(object sender, EventArgs e)
        {
            // 1️ 生成粒子
            if (_particles.Count < 40) // 控制数量
            {
                var p = CreateParticle();
                _particles.Add(p);
                ParticleCanvas.Children.Add(p.UI);
            }

            // 2️ 更新粒子
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var p = _particles[i];

                p.X += p.SpeedX;
                p.Y += p.SpeedY;
                p.Life -= 0.005;

                // 透明度渐变
                p.UI.Opacity = p.Life;

                Canvas.SetLeft(p.UI, p.X);
                Canvas.SetTop(p.UI, p.Y);

                // 3 回收粒子
                if (p.Life <= 0)
                {
                    ParticleCanvas.Children.Remove(p.UI);
                    _particles.RemoveAt(i);
                }
            }
        }
        private Particle CreateParticle()
        {
            double size = _random.Next(2, 6);

            var ellipse = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = new SolidColorBrush(Color.FromArgb(200, 0, 255, 255)), // 青色光点
                Opacity = 0
            };

            var p = new Particle
            {
                UI = ellipse,
                X = _random.NextDouble() * ActualWidth,
                Y = ActualHeight + 10, // 从底部往上
                SpeedX = (_random.NextDouble() - 0.5) * 0.3,
                SpeedY = -(_random.NextDouble() * 0.5 + 0.2), // 向上漂
                Life = 1
            };

            return p;
        }



        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(this);

            double centerX = ActualWidth / 2;
            double centerY = ActualHeight / 2;

            double dist = Math.Sqrt(Math.Pow(pos.X - centerX, 2) + Math.Pow(pos.Y - centerY, 2));
            double maxDist = Math.Sqrt(centerX * centerX + centerY * centerY);

            double factor = 1 - (dist / maxDist);

            GlowEffect.BlurRadius = 10 + factor * 20;
            GlowEffect.Opacity = 0.3 + factor * 0.7;
        }


        private void BurstEffect(Point center)
        {
            for (int i = 0; i < 30; i++)
            {
                var p = CreateParticle();

                double angle = _random.NextDouble() * Math.PI * 2;
                double speed = _random.NextDouble() * 3 + 1;

                p.X = center.X;
                p.Y = center.Y;

                p.SpeedX = Math.Cos(angle) * speed;
                p.SpeedY = Math.Sin(angle) * speed;

                _particles.Add(p);
                ParticleCanvas.Children.Add(p.UI);
            }
        }

    }

    class Particle
    {
        public Ellipse UI { get; set; }
        public double X, Y;
        public double SpeedX, SpeedY;
        public double Life; // 0~1
    }
}
