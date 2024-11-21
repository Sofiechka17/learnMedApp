using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfAppMed
{
    /// <summary>
    /// Логика взаимодействия для Titul.xaml
    /// </summary>
    public partial class Titul : Window
    {
        public Titul()
        {
            InitializeComponent();
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Создаем всплывающее окно
            Popup popup = new Popup();
            popup.PlacementTarget = (Image)sender;
            popup.Placement = PlacementMode.Bottom;
            popup.Width = 200;
            popup.Height = 100;

            // Создаем контент для всплывающего окна
            StackPanel content = new StackPanel();
            content.Orientation = Orientation.Vertical;
            content.Margin = new Thickness(10);

            Button btnAuth = new Button();
            btnAuth.Content = "Авторизация";
            btnAuth.Click += BtnAuth_Click;
            content.Children.Add(btnAuth);

            Button btnReg = new Button();
            btnReg.Content = "Регистрация";
            btnReg.Click += BtnReg_Click;
            content.Children.Add(btnReg);

            popup.Child = content;
            popup.IsOpen = true;
        }

        private void BtnAuth_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно AuthTeacher
            AuthTeacher authWindow = new AuthTeacher();
            authWindow.Show();
            this.Close();
        }

        private void BtnReg_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно StudentAdminWindow
            StudentAdminWindow regWindow = new StudentAdminWindow();
            regWindow.Show();
            this.Close();
        }

    }
}
