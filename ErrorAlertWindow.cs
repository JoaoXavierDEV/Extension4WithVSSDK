using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Extension4WithVSSDK
{
    public partial class ErrorAlertWindow : Window
    {
        private static readonly Random _random = new Random();

        private static readonly string[] ImageExtensions =
            { ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp" };

        private static readonly string[] SoundExtensions =
            { ".wav", ".mp3", ".wma", ".m4a" };

        private MediaPlayer _mediaPlayer;

        public ErrorAlertWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            LoadRandomImage();
            PlayRandomSound();
        }

        private static string GetAssetsBasePath()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        private static string PickRandomFile(string directory, string[] extensions)
        {
            if (!Directory.Exists(directory))
                return null;

            var files = Directory.GetFiles(directory)
                .Where(f => extensions.Any(ext =>
                    f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                .ToArray();

            return files.Length > 0 ? files[_random.Next(files.Length)] : null;
        }

        private void LoadRandomImage()
        {
            try
            {
                var dir = Path.Combine(GetAssetsBasePath(), "assets", "imagens", "erros");
                var file = PickRandomFile(dir, ImageExtensions);
                if (file is null) return;

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(file, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                ErrorImage.Source = bitmap;
            }
            catch
            {
                // Imagem inválida ou inacessível
            }
        }

        private void PlayRandomSound()
        {
            try
            {
                var dir = Path.Combine(GetAssetsBasePath(), "assets", "sons", "erros");
                var file = PickRandomFile(dir, SoundExtensions);
                if (file is null) return;

                // MediaPlayer roda dentro do processo da extensão
                _mediaPlayer = new MediaPlayer();
                _mediaPlayer.Open(new Uri(file, UriKind.Absolute));
                _mediaPlayer.Play();
            }
            catch
            {
                // Som inválido ou inacessível
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        protected override void OnClosed(EventArgs e)
        {
            _mediaPlayer?.Stop();
            _mediaPlayer?.Close();
            _mediaPlayer = null;
            base.OnClosed(e);
        }
    }
}