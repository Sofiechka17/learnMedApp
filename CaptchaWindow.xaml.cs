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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;

namespace WpfAppMed
{
    /// <summary>
    /// Логика взаимодействия для CaptchaWindow.xaml
    /// </summary>
    public partial class CaptchaWindow : Window
    {
        private string _captchaText;

        public bool IsCaptchaVerified { get; private set; } = false;

        public CaptchaWindow()
        {
            InitializeComponent();
            GenerateAndDisplayCaptcha();
        }

        private void GenerateAndDisplayCaptcha()
        {
            _captchaText = CaptchaGenerator.GenerateCaptchaText();
            Bitmap captchaImage = CaptchaGenerator.GenerateCaptchaImage(_captchaText);

            // Преобразуем Bitmap в BitmapImage для отображения в WPF
            using (MemoryStream memory = new MemoryStream())
            {
                captchaImage.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                CaptchaImage.Source = bitmapImage;
            }
        }

        private void VerifyCaptchaButton_Click(object sender, RoutedEventArgs e)
        {
            if (CaptchaTextBox.Text == _captchaText)
            {
                IsCaptchaVerified = true;
                MessageBox.Show("Капча введена верно.");
                this.Close();
            }
            else
            {
                MessageBox.Show("Капча введена неверно. Попробуйте снова.");
                GenerateAndDisplayCaptcha();
                CaptchaTextBox.Clear();
                CaptchaTextBox.Focus();
            }
        }

        private void RefreshCaptchaButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateAndDisplayCaptcha();
            CaptchaTextBox.Clear();
            CaptchaTextBox.Focus();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!IsCaptchaVerified)
            {
                e.Cancel = true; // Отменяем закрытие окна
                MessageBox.Show("Вы должны пройти капчу перед продолжением.");
            }
            else
            {
                base.OnClosing(e);
            }
        }
    }
}
