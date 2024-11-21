using System.Text;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static WpfAppMed.TeacherAdminWindow;
using Microsoft.Win32;
using System.IO;

namespace WpfAppMed
{
    /// <summary>
    /// Interaction logic for StudentAdminWindow.xaml
    /// </summary>
    public partial class StudentAdminWindow : Window
    {
        private NpgsqlConnection connection;

        private string conectString = "Host=localhost;Port=5432;Database=postgres; User Id = postgres; Password = postgres;";
        public StudentAdminWindow()
        {
            InitializeComponent();

            SqlConnectionReader();
        }

        public class Student
        {
            public int student_id { get; set; }
            public string student_fullname { get; set; }
            public string student_phonenumber { get; set; }
            public DateTime student_datereg { get; set; }
            public int? image_id { get; set; } // Может быть null
            public ImageSource StudentImage { get; set; } // Для привязки изображения в DataGrid
        }

        //Метод для загрузк данных из бд в DataGrid
        private void SqlConnectionReader()
        {
            List<Student> students = new List<Student>();

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(conectString))
                {
                    connection.Open();

                    string sql = @"
                SELECT s.student_id, s.student_fullname, s.student_phonenumber, s.student_datereg, s.image_id, i.image
                FROM medcenter_schema.students s
                LEFT JOIN medcenter_schema.images i ON s.image_id = i.image_id";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Student student = new Student
                            {
                                student_id = reader.GetInt32(0),
                                student_fullname = reader.GetString(1),
                                student_phonenumber = reader.GetString(2),
                                student_datereg = reader.GetDateTime(3),
                                image_id = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4),
                                StudentImage = null
                            };

                            if (!reader.IsDBNull(5))
                            {
                                byte[] imageBytes = (byte[])reader[5];
                                student.StudentImage = ByteImage.GetImageFromByteArray(imageBytes);
                            }

                            students.Add(student);
                        }
                    }

                    connection.Close();
                }
                // Привязываем данные к DataGrid
                Registration_student.ItemsSource = students;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при подключении к базе данных: " + ex.Message);
            }
        }

        // Обработчик события кнопки "Добавить"
        private void Button_Click_add(object sender, RoutedEventArgs e)
        {
            string phoneNumber = number_input.Text;
            List<char> digits = new List<char> { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            int validPhoneNumberSymbolCount = 11;

            if (phoneNumber.Length != validPhoneNumberSymbolCount)
            {
                bool isPhoneNumberValid = true;
                int symbolIndex = 0;

                while (isPhoneNumberValid && symbolIndex < phoneNumber.Length)
                {
                    char symbol = digits[symbolIndex];

                    if (digits.Contains(symbol) == false)
                    {
                        isPhoneNumberValid = false;
                    }
                    else
                    {
                        symbolIndex++;
                    }
                }

                if (isPhoneNumberValid)
                {
                    try
                    {
                        using (NpgsqlConnection connection = new NpgsqlConnection(conectString))
                        {
                            connection.Open();

                            //sql запрос для добавление нового студента
                            var sqlrequest = "INSERT INTO medcenter_schema.students(\"student_fullname\", \"student_phonenumber\", \"student_datereg\") VALUES (@student_fullname, @student_phonenumber, @student_datereg)";
                            using (var cmd = new NpgsqlCommand(sqlrequest, connection))
                            {
                                //Передаем параметры из полей ввода в запрос
                                cmd.Parameters.AddWithValue("student_fullname", fullname_input.Text);
                                cmd.Parameters.AddWithValue("student_phonenumber", number_input.Text);
                                cmd.Parameters.AddWithValue("@student_datereg", DateTime.Parse(date_input.Text));

                                cmd.Prepare();
                                cmd.ExecuteNonQuery();// выполняем запрос
                            }

                            connection.Close();
                        }

                        //Сообщаем пользователю об успешном добавлении
                        MessageBox.Show("Student added successfully");

                        //Обновляем DataGrid, чтобы отобразить новые данные
                        SqlConnectionReader();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error adding student: " + ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("В телефоне присутсвуют символы не цифры");
                }
            }
            else
            {
                MessageBox.Show("Количество цифр не соответствует номеру телефона");
            }

        }

        private void Button_Click_del(object sender, RoutedEventArgs e)
        {
            if (Registration_student.SelectedItem is Student selectedStudent)
            {
                try
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(conectString))
                    {
                        connection.Open();

                        string sql = "DELETE FROM medcenter_schema.students WHERE \"student_id\" = @student_id";

                        using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
                        {
                            cmd.Parameters.AddWithValue("@student_id", selectedStudent.student_id);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Student deleted successfully");
                                SqlConnectionReader();
                            }
                            else
                            {
                                MessageBox.Show("No student was deleted");
                            }
                        }

                        connection.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting student: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Please select a student to delete");
            }
        }

        private void Button_Click_up(object sender, RoutedEventArgs e)
        {
            Student selectedStudent = (Student)Registration_student.SelectedItem;

            if (selectedStudent != null)
            {
                try
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(conectString))
                    {
                        connection.Open();

                        string sql = @"UPDATE medcenter_schema.students
                                            SET ""student_fullname"" = @student_fullname,
                                                ""student_phonenumber"" = @student_phonenumber,
                                                ""student_datereg"" = @student_datereg
                                                WHERE ""student_id"" = @student_id";

                        using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
                        {
                            cmd.Parameters.AddWithValue("@student_id", selectedStudent.student_id);
                            cmd.Parameters.AddWithValue("@student_fullname", fullname_input.Text);
                            cmd.Parameters.AddWithValue("@student_phonenumber", number_input.Text);
                            cmd.Parameters.AddWithValue("@student_datereg", DateTime.Parse(date_input.Text));


                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Student updated successfully");
                                SqlConnectionReader();
                            }
                            else
                            {
                                MessageBox.Show("No student was updated");
                            }
                        }

                        connection.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating student: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Please select a student to update");
            }
        }

        private void Button_Click_teacher(object sender, RoutedEventArgs e)
        {
            TeacherAdminWindow teacherAdminWindow = new TeacherAdminWindow();//Создание нового окна
            this.Close();// закрытие этого окна
            teacherAdminWindow.ShowDialog();//отображение
        }

        private void Registration_student_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var row = ItemsControl.ContainerFromElement(Registration_student, e.OriginalSource as DependencyObject) as DataGridRow;

            if (row != null)
            {
                var selectedStudent = row.Item as Student;

                if (selectedStudent != null)
                {
                    fullname_input.Text = selectedStudent.student_fullname;
                    number_input.Text = selectedStudent.student_phonenumber;
                    date_input.Text = selectedStudent.student_datereg.ToString("dd-MM-yyyy");
                }
            }
        }

        public static class ByteImage
        {
            // Преобразование массива байтов в ImageSource
            public static System.Windows.Media.ImageSource GetImageFromByteArray(byte[] byteArray)
            {
                using (MemoryStream stream = new MemoryStream(byteArray))
                {
                    var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                    return decoder.Frames[0];
                }
            }

            // Обёртка для конверсии
            public static System.Windows.Media.ImageSource Convert(System.Windows.Media.ImageSource source)
            {
                return source;
            }
        }


        private void SelectImage(object sender, RoutedEventArgs e)
        {
            if (Registration_student.SelectedItem is Student selectedStudent)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg;*.bmp)|*.png;*.jpeg;*.jpg;*.bmp";
                if (openFileDialog.ShowDialog() == true)
                {
                    byte[] imageBytes = File.ReadAllBytes(openFileDialog.FileName);

                    using (NpgsqlConnection connection = new NpgsqlConnection(conectString))
                    {
                        connection.Open();

                        // Начинаем транзакцию
                        using (var transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                // Вставляем изображение и получаем image_id
                                string insertImageQuery = "INSERT INTO medcenter_schema.images (image) VALUES (@image) RETURNING image_id";
                                int imageId;
                                using (var insertImageCommand = new NpgsqlCommand(insertImageQuery, connection))
                                {
                                    insertImageCommand.Parameters.AddWithValue("@image", NpgsqlTypes.NpgsqlDbType.Bytea, imageBytes);
                                    imageId = (int)insertImageCommand.ExecuteScalar();
                                }

                                // Обновляем запись студента, устанавливая image_id
                                string updateStudentQuery = "UPDATE medcenter_schema.students SET image_id = @image_id WHERE student_id = @student_id";
                                using (var updateStudentCommand = new NpgsqlCommand(updateStudentQuery, connection))
                                {
                                    updateStudentCommand.Parameters.AddWithValue("@image_id", imageId);
                                    updateStudentCommand.Parameters.AddWithValue("@student_id", selectedStudent.student_id);
                                    updateStudentCommand.ExecuteNonQuery();
                                }

                                // Фиксируем транзакцию
                                transaction.Commit();

                                MessageBox.Show("Изображение успешно сохранено в базе данных и привязано к студенту!");

                                // Обновляем изображение студента в DataGrid
                                selectedStudent.image_id = imageId;
                                selectedStudent.StudentImage = ByteImage.GetImageFromByteArray(imageBytes);

                                // Обновляем DataGrid
                                Registration_student.Items.Refresh();
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                MessageBox.Show("Ошибка при сохранении изображения: " + ex.Message);
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите студента, чтобы привязать к нему изображение.");
            }
        }

        private void LoadImage(object sender, RoutedEventArgs e)
        {
            SqlConnectionReader();
            MessageBox.Show("Изображения студентов обновлены.");

            try
            {
                string currentDirectory = Directory.GetCurrentDirectory();
                string binDirectory = Directory.GetParent(currentDirectory).Parent.FullName;
                string projectDirectory = Directory.GetParent(binDirectory).FullName;

                // Создаем папку Images в корневой папке проекта, если ее нет
                string imagesFolder = System.IO.Path.Combine(projectDirectory, "Images");
                
                if (Directory.Exists(imagesFolder) == false)
                {
                    Directory.CreateDirectory(imagesFolder);
                }

                // Создаем уникальную подпапку внутри папки Images
                string uniqueFolderName = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string uniqueFolderPath = System.IO.Path.Combine(imagesFolder, uniqueFolderName);
                Directory.CreateDirectory(uniqueFolderPath);

                // Подключаемся к базе данных и загружаем изображения студентов
                using (NpgsqlConnection connection = new NpgsqlConnection(conectString))
                {
                    connection.Open();

                    string sql = @"
                        SELECT s.student_id, s.student_fullname, i.image
                        FROM medcenter_schema.students s
                        LEFT JOIN medcenter_schema.images i ON s.image_id = i.image_id
                        WHERE i.image IS NOT NULL";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int studentId = reader.GetInt32(0);
                            string studentFullName = reader.GetString(1);
                            byte[] imageBytes = (byte[])reader[2];

                            // Формируем имя файла: studentId_studentFullName.png
                            string sanitizedFullName = string.Concat(studentFullName.Split(System.IO.Path.GetInvalidFileNameChars()));
                            string imageFileName = $"{studentId}_{sanitizedFullName}.png";
                            string imagePath = System.IO.Path.Combine(uniqueFolderPath, imageFileName);

                            // Сохраняем изображение в файл
                            File.WriteAllBytes(imagePath, imageBytes);
                        }
                    }

                    connection.Close();
                }

                MessageBox.Show($"Изображения успешно сохранены в папку: {uniqueFolderPath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении изображений: " + ex.Message);
            }
        }

        private void Button_Click_export(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем данные из DataGrid
                List<string> lines = new List<string>();

                // Заголовки столбцов
                string header = "ID,Full Name,Phone Number,Date Registered,Image Status";
                lines.Add(header);

                // Данные
                foreach (var student in Registration_student.Items)
                {
                    if (student is Student s)
                    {
                        string imageStatus = s.StudentImage != null ? "предоставлен" : "не предоставлен";

                        string line = $"{s.student_id},{s.student_fullname},{s.student_phonenumber},{s.student_datereg:dd-MM-yyyy},{imageStatus}";
                        lines.Add(line);
                    }
                }

                // Сохранение данных в файл CSV
                string path = "studentsReg.csv";
                File.WriteAllLines(path, lines);
                MessageBox.Show($"Данные экспортированы в файл: {path}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка экспорта данных: " + ex.Message);
            }
        }
    }
}
