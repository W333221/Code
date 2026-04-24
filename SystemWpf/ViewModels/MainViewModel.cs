using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace SystemWpf.ViewModels
{
    public class MainViewModel:BindableBase
    {
        private Random _random = new Random();

        public string Name => "张三";
        public string Level => "练气期";
        public int FatRate => 25;

        private double _glow;
        public double Glow
        {
            get => _glow;
            set => SetProperty(ref _glow, value);
        }

        public ObservableCollection<int> WaveValues { get; set; } = new();

        public DelegateCommand PlayCommand=> new(PlayVoice);

        public MainViewModel()
        {
            for (int i = 0; i < 30; i++)
                WaveValues.Add(10);
           
            StartBreathing();
            StartWave();
        }

        private void StartBreathing()
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
            double t = 0;

            timer.Tick += (s, e) =>
            {
                t += 0.08;
                Glow = (Math.Sin(t) + 1) / 2;
            };

            timer.Start();
        }

        private void StartWave()
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };

            timer.Tick += (s, e) =>
            {
                for (int i = 0; i < WaveValues.Count; i++)
                    WaveValues[i] = _random.Next(5, 50);
            };

            timer.Start();
        }

        private void PlayVoice()
        {
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "system.wav");
            var player = new SoundPlayer(fullPath);
            player.Play();
        }
    }
}
