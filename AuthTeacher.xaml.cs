using Npgsql;
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
using static WpfAppMed.TeacherAdminWindow;

namespace WpfAppMed
{
    /// <summary>
    /// Логика взаимодействия для AuthTeacher.xaml
    /// </summary>
    public partial class AuthTeacher : Window
    {
        private string conectString = "Host=localhost;Port=5432;Database=postgres; User Id = postgres; Password = postgres;";
        private int _failedLoginAttempts = 0;

        public AuthTeacher()
        {
            InitializeComponent();
        }

        private void btn_ok(object sender, RoutedEventArgs e)
        {
            string login = Login.Text.Trim();
            string password = Password.Text.Trim();
            string selectedPersonType = PersonSelector.Text;
            System.Diagnostics.Debug.WriteLine(selectedPersonType);

            if (selectedPersonType == "Преподаватель" || selectedPersonType == "Ученик")
            {
                try
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(conectString))
                    {
                        connection.Open();

                        string sql = "";

                        if (selectedPersonType == "Преподаватель")
                        {
                            sql = "SELECT teacher_id, teacher_fullname, phone_number FROM medcenter_schema.teachers WHERE teacher_fullname = @login AND phone_number = @password";
                        }
                        else
                        {
                            sql = "SELECT student_id, student_fullname, student_phonenumber FROM medcenter_schema.students WHERE student_fullname = @login AND student_phonenumber = @password";
                        }

                        using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
                        {
                            cmd.Parameters.AddWithValue("@login", login);
                            cmd.Parameters.AddWithValue("@password", password);

                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read()) 
                                {
                                    Window nextWindow = null;

                                    if (selectedPersonType == "Преподаватель")
                                    {
                                        nextWindow = new TeacherWindow();
                                    }
                                    else
                                    {
                                        int studentId = reader.GetInt32(0);
                                        nextWindow = new StudentWindow(studentId);
                                    }

                                    this.Close();
                                    nextWindow.Show();
                                }
                                else
                                {
                                    _failedLoginAttempts++;
                                    MessageBox.Show("Неверный логин или пароль");

                                    if (_failedLoginAttempts >= 3)
                                    {
                                        ShowCaptchaWindow();
                                    }
                                }
                            }
                        }
                        

                        connection.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при подключении к базе данных: " + ex.Message);
                }
            }
            else if (selectedPersonType == "Админ")
            {
                System.Diagnostics.Debug.WriteLine(Login.Text);
                System.Diagnostics.Debug.WriteLine(Password.Text);

                if (Login.Text == "admin" && Password.Text == "root")
                {
                    StudentAdminWindow studentWindow = new StudentAdminWindow();
                    studentWindow.ShowDialog();//отображение
                    this.Close();// закрытие этого окна
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль для админа");
                }
            }
        }

        private void ShowCaptchaWindow()
        {
            CaptchaWindow captchaWindow = new CaptchaWindow
            {
                Owner = this // Устанавливаем владельца окна
            };

            captchaWindow.ShowDialog();

            if (captchaWindow.IsCaptchaVerified)
            {
                // Пользователь успешно прошел капчу, сбрасываем счетчик неудачных попыток
                _failedLoginAttempts = 0;
            }
        }

        private void btn_cancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
