using Npgsql;
using System;
using System.Collections;
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
    /// Логика взаимодействия для TeacherAdminWindow.xaml
    /// </summary>
    public partial class TeacherAdminWindow : Window
    {
        private NpgsqlConnection connection;
        private string conectString = "Host=localhost;Port=5432;Database=postgres; User Id = postgres; Password = postgres;";

        public TeacherAdminWindow()
        {
            InitializeComponent();
            LoadData(); // Загружаем преподавателей
            LoadDirections(); // Загружаем направления
        }

        public class Teacher
        {
            public int teacher_id { get; set; }
            public string teacher_fullname { get; set; }
            public int teacher_seniority { get; set; }
            public string phone_number { get; set; }
        }

        //Метод для загрузк данных из бд в DataGrid
        private void LoadData()
        {
            List<Teacher> teachers = new List<Teacher>();

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(conectString))
                {
                    connection.Open();

                    string sql = "SELECT * FROM medcenter_schema.teachers";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            teachers.Add(new Teacher
                            {
                                teacher_id = reader.GetInt32(0),
                                teacher_fullname = reader.GetString(1),
                                teacher_seniority = reader.GetInt32(2),
                                phone_number = reader.GetString(3)
                            });
                        }
                    }
                }

                // Привязываем данные к DataGrid
                Registration_teacher.ItemsSource = teachers;

                // Обновляем CollectionView, чтобы применить текущие фильтры после загрузки данных
                var collectionView = CollectionViewSource.GetDefaultView(Registration_teacher.ItemsSource);
                collectionView.Filter = FilterTeachers;
                collectionView.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при подключении к базе данных: " + ex.Message);
            }
        }

        // Обработчик события кнопки "Добавить"
        private void Click_add(object sender, RoutedEventArgs e)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(conectString))
                {
                    connection.Open();

                    //sql запрос для добавление нового преподавателя
                    var query = "INSERT INTO medcenter_schema.teachers(\"teacher_fullname\", \"teacher_seniority\", \"phone_number\") VALUES (@teacher_fullname, @teacher_seniority, @phone_number)";
                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        //Передаем параметры из полей ввода в запрос
                        cmd.Parameters.AddWithValue("@teacher_fullname", fullname_input2.Text);
                        cmd.Parameters.AddWithValue("@teacher_seniority", int.Parse(seniority_input.Text));
                        cmd.Parameters.AddWithValue("@phone_number", number_input2.Text);

                        cmd.Prepare();
                        cmd.ExecuteNonQuery();// выполняем запрос
                    }

                }

                //Сообщаем пользователю об успешном добавлении
                MessageBox.Show("Teacher added successfully");

                //Обновляем DataGrid, чтобы отобразить новые данные
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding teacher: " + ex.Message);
            }
        }

        private void Click_del(object sender, RoutedEventArgs e)
        {
            if (Registration_teacher.SelectedItem is Teacher selectedTeacher)
            {
                try
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(conectString))
                    {
                        connection.Open();

                        string sql = "DELETE FROM medcenter_schema.teachers WHERE \"teacher_id\" = @teacher_id";

                        using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
                        {
                            cmd.Parameters.AddWithValue("@teacher_id", selectedTeacher.teacher_id);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Teacher deleted successfully");
                                LoadData();
                            }
                            else
                            {
                                MessageBox.Show("No teacher was deleted");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting teacher: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Please select a teacher to delete");
            }
        }

        private void Click_up(object sender, RoutedEventArgs e)
        {
            Teacher selectedTeacher = (Teacher)Registration_teacher.SelectedItem;

            if (selectedTeacher != null)
            {
                try
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(conectString))
                    {
                        connection.Open();

                        string sql = @"UPDATE medcenter_schema.teachers
                                    SET ""teacher_fullname"" = @teacher_fullname,
                                        ""teacher_seniority"" = @teacher_seniority,
                                        ""phone_number"" = @phone_number
                                        WHERE ""teacher_id"" = @teacher_id";

                        using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
                        {
                            cmd.Parameters.AddWithValue("@teacher_id", selectedTeacher.teacher_id);
                            cmd.Parameters.AddWithValue("@teacher_fullname", fullname_input2.Text);
                            cmd.Parameters.AddWithValue("@teacher_seniority", int.Parse(seniority_input.Text));
                            cmd.Parameters.AddWithValue("@phone_number", number_input2.Text);


                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Teacher updated successfully");
                                LoadData();
                            }
                            else
                            {
                                MessageBox.Show("No teacher was updated");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating teacher: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Please select a teacher to update");
            }
        }

        private void Button_Click_student(object sender, RoutedEventArgs e)
        {
            StudentAdminWindow studentWindow = new StudentAdminWindow();//Создание нового окна
            this.Close();// закрытие этого окна
            studentWindow.ShowDialog();//отображение
        }

        private void Button_LK_teacher(object sender, RoutedEventArgs e)
        {
            TeacherWindow teacherWindow = new TeacherWindow();//Создание нового окна
            this.Close();// закрытие этого окна
            teacherWindow.ShowDialog();//отображение
        }
        private void Registration_teacher_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var row = ItemsControl.ContainerFromElement(Registration_teacher, e.OriginalSource as DependencyObject) as DataGridRow;

            if (row != null)
            {
                var selectedTeacher = row.Item as Teacher;

                if (selectedTeacher != null)
                {
                    fullname_input2.Text = selectedTeacher.teacher_fullname;
                    seniority_input.Text = selectedTeacher.teacher_seniority.ToString();
                    number_input2.Text = selectedTeacher.phone_number;
                }
            }
        }

        private void Seniority_Changed(object sender, SelectionChangedEventArgs e)
        {
            var collectionView = CollectionViewSource.GetDefaultView(Registration_teacher.ItemsSource);
            collectionView.Filter = FilterTeachers;
            collectionView.Refresh();
        }

        private bool FilterSeniority(object item)
        {
            var teacher = item as Teacher;
            if (teacher != null)
            {
                string selectedSeniority = Filter_seniority.SelectedItem as string;
                if (selectedSeniority == "Опытный")
                {
                    return teacher.teacher_seniority > 5;
                }
                else if (selectedSeniority == "Молодой")
                {
                    return teacher.teacher_seniority <= 5;
                }
                else
                {
                    return true;
                }
            }
            return true;
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            var collectionView = CollectionViewSource.GetDefaultView(Registration_teacher.ItemsSource);
            collectionView.Filter = FilterTeachers;
            collectionView.Refresh();
        }

        private bool FilterTeachers(object item)
        {
            var teacher = item as Teacher;
            if (teacher != null)
            {
                // Фильтрация по стажу
                if (Filter_seniority.SelectedItem is ComboBoxItem selectedSeniority)
                {
                    if (selectedSeniority.Content.ToString() == "Опытный" && teacher.teacher_seniority <= 5)
                        return false;
                    if (selectedSeniority.Content.ToString() == "Молодой" && teacher.teacher_seniority > 5)
                        return false;
                }

                // Поиск по фамилии
                string searchText = seach.Text.ToLower();
                if (!string.IsNullOrEmpty(searchText))
                {
                    if (!teacher.teacher_fullname.ToLower().Contains(searchText))
                        return false;
                }

                return true;
            }
            return false;
        }

        private void RegisterTeacherToCourse_Click(object sender, RoutedEventArgs e)
        {
            if (Registration_teacher.SelectedItem is Teacher selectedTeacher && Courses.SelectedItem is Course selectedCourse)
            {
                try
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(conectString))
                    {
                        connection.Open();

                        // Записываем преподавателя на курс
                        string sql = "INSERT INTO medcenter_schema.teacher_courses (teacher_id, course_id) VALUES (@teacherId, @courseId)";

                        using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
                        {
                            cmd.Parameters.AddWithValue("@teacherId", selectedTeacher.teacher_id);
                            cmd.Parameters.AddWithValue("@courseId", selectedCourse.course_id); // курс нужно выбирать из интерфейса
                            cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("Преподаватель успешно записан на курс");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при записи преподавателя на курс: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите преподавателя и курс");
            }
        }

        private void LoadDirections()
        {
            List<Direction> directions = new List<Direction>();

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(conectString))
                {
                    connection.Open();

                    string sql = "SELECT direction_id, direction_name FROM medcenter_schema.directions";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
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

                    Directions.ItemsSource = directions; // Привязываем список направлений к ComboBox
                    Directions.DisplayMemberPath = "direction_name"; // Показываем только названия направлений
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке направлений: " + ex.Message);
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
    }
}
