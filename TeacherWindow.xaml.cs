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
using static WpfAppMed.StudentAdminWindow;
using static WpfAppMed.TeacherWindow;

namespace WpfAppMed
{
    /// <summary>
    /// Логика взаимодействия для TeacherWindow.xaml
    /// </summary>
    public partial class TeacherWindow : Window
    {
        private NpgsqlConnection connection;

        private string conectString = "Host=localhost;Port=5432;Database=postgres; User Id = postgres; Password = postgres;";
        public TeacherWindow()
        {
            InitializeComponent();
            ConnectReader();
        }

        public class Direction
        {
            public int direction_id { get; set; }
            public string direction_name { get; set; }

            public override string ToString()
            {
                return direction_name; // для отображения названия в ComboBox
            }
        }

        public class Course
        {
            public int course_id { get; set; }
            public string course_name { get; set; }
        }

        //Метод для загрузк данных из бд в DataGrid
        private void ConnectReader()
        {
            List<Student> students = new List<Student>();
            List<Direction> directions = new List<Direction>();

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(conectString))
                {
                    connection.Open();

                    // Загружаем студентов
                    string studentSql = "SELECT student_id, student_fullname FROM medcenter_schema.students";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(studentSql, connection))
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            students.Add(new Student
                            {
                                student_id = reader.GetInt32(0),
                                student_fullname = reader.GetString(1)
                            });
                        }
                    }

                    // Загружаем направления
                    string directionSql = "SELECT direction_id, direction_name FROM medcenter_schema.directions";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(directionSql, connection))
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            directions.Add(new Direction
                            {
                                direction_id = reader.GetInt32(0),
                                direction_name = reader.GetString(1)
                            });
                        }
                    }

                    connection.Close();
                }

                // Привязываем данные к ComboBox
                Students.ItemsSource = students;
                Directions.ItemsSource = directions;
                Directions.DisplayMemberPath = "direction_name"; // Показываем только названия курсов
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при подключении к базе данных: " + ex.Message);
            }
        }

        private void Directions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Directions.SelectedItem is Direction selectedDirection)
            {
                List<Course> courses = new List<Course>();

                try
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(conectString))
                    {
                        connection.Open();

                        string sql = "SELECT course_id, course_name FROM medcenter_schema.courses WHERE direction_id = @directionId";

                        using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
                        {
                            cmd.Parameters.AddWithValue("@directionId", selectedDirection.direction_id);

                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    courses.Add(new Course
                                    {
                                        course_id = reader.GetInt32(0),
                                        course_name = reader.GetString(1)
                                    });
                                }
                            }

                            Courses.ItemsSource = courses; // Привязываем список курсов к ComboBox
                            Courses.DisplayMemberPath = "course_name"; // Показываем только названия курсов
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке курсов: " + ex.Message);
                }
            }
        }

        private void btn_testing(object sender, RoutedEventArgs e)
        {
            if (Students.SelectedItem is Student selectedStudent && Courses.SelectedItem is Course selectedCourse)
            {
                try
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(conectString))
                    {
                        connection.Open();

                        // Обновляем результат теста для студента
                        string sql = "UPDATE medcenter_schema.testResults SET passed = TRUE, score = 100 WHERE student_id = @studentId AND course_id = @courseId";

                        using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
                        {
                            cmd.Parameters.AddWithValue("@studentId", selectedStudent.student_id);
                            cmd.Parameters.AddWithValue("@courseId", selectedCourse.course_id); // Аналогично, идентификатор курса

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Тест успешно пройден. Сертификат выдан.");

                                // Добавляем запись в таблицу сертификатов
                                string certSql = "INSERT INTO medcenter_schema.certificates (student_id, course_id) VALUES (@studentId, @courseId)";
                                using (NpgsqlCommand certCmd = new NpgsqlCommand(certSql, connection))
                                {
                                    certCmd.Parameters.AddWithValue("@studentId", selectedStudent.student_id);
                                    certCmd.Parameters.AddWithValue("@courseId", selectedCourse.course_id); // Аналогично, идентификатор курса
                                    certCmd.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                MessageBox.Show("Не удалось обновить результат теста. Проверьте данные.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при обновлении теста: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите студента и курс");
            }
        }

        private void btn_note(object sender, RoutedEventArgs e)
        {
            if (Students.SelectedItem is Student selectedStudent && Courses.SelectedItem is Course selectedCourse)
            {
                try
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(conectString))
                    {
                        connection.Open();

                        // Записываем студента на курс
                        string sql = "INSERT INTO medcenter_schema.registrations (student_id, course_id, status) VALUES (@studentId, @courseId, 'активный')";

                        using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
                        {
                            cmd.Parameters.AddWithValue("@studentId", selectedStudent.student_id);
                            cmd.Parameters.AddWithValue("@courseId", selectedCourse.course_id); // Придется добавить идентификатор курса в класс Course
                            cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("Студент успешно записан на курс");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при записи студента на курс: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите студента и курс");
            }
        }

        private void btn_cancel(object sender, RoutedEventArgs e)
        {
            this.Close(); // Закрывает текущее окно
        }

        private void Courses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
