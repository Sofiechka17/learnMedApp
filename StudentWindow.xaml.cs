using Npgsql;
using System;
using System.Data;
using System.Windows;

namespace WpfAppMed
{
    public partial class StudentWindow : Window
    {
        private int _studentId; // идентификатор студента

        // строка подключения к базе данных
        private string connectionString = "Host=localhost;Port=5432;Database=postgres; User Id = postgres; Password = postgres;";

        public StudentWindow(int studentId)
        {
            InitializeComponent();
            _studentId = studentId;
            LoadStudentData();
        }

        private void LoadStudentData()
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    // Получение информации о курсе студента
                    using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM get_student_course_info(@studentId)", connection))
                    {
                        cmd.Parameters.AddWithValue("@studentId", _studentId);
                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                CourseNameTextBlock.Text = reader.GetString(1);
                                TestNameTextBlock.Text = reader.GetString(1);
                            }
                        }
                    }

                    // Получение результатов тестирования студента
                    using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM medcenter_schema.testResults WHERE student_id = @studentId", connection))
                    {
                        cmd.Parameters.AddWithValue("@studentId", _studentId);
                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                TestResultsTextBlock.Text = $"Баллы: {reader["score"]}, Пройдено: {reader["passed"]}";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
            }
        }

        private void ViewCertificateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    // Проверка наличия сертификата и извлечение имени студента и названия курса
                    using (NpgsqlCommand cmd = new NpgsqlCommand(@"
                        SELECT s.student_fullname, c.course_name
                        FROM medcenter_schema.certificates cert
                        JOIN medcenter_schema.students s ON s.student_id = cert.student_id
                        JOIN medcenter_schema.courses c ON c.course_id = cert.course_id
                        WHERE s.student_id = @studentId", connection))
                    {
                        cmd.Parameters.AddWithValue("@studentId", _studentId);

                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Получаем имя студента и название курса
                                string studentName = reader.GetString(0); // student_fullname
                                string courseName = reader.GetString(1);  // course_name

                                // Генерируем сертификат с именем студента и названием курса
                                GenerateCertificatePdf(studentName, courseName);
                            }
                            else
                            {
                                MessageBox.Show("Сертификат не найден.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при просмотре сертификата: " + ex.Message);
            }
        }

        private void GenerateCertificatePdf(string studentName, string courseName)
        {
            // Генерация сертификата в формате PDF
            try
            {
                // Импортируем библиотеку iTextSharp (добавьте NuGet пакет iTextSharp в проект)
                using (var document = new iTextSharp.text.Document())
                {
                    iTextSharp.text.pdf.PdfWriter.GetInstance(document, new System.IO.FileStream($"Certificate_{studentName}_{courseName}.pdf", System.IO.FileMode.Create));
                    document.Open();

                    // Добавляем информацию в PDF
                    document.Add(new iTextSharp.text.Paragraph("СЕРТИФИКАТ"));
                    document.Add(new iTextSharp.text.Paragraph($"Студент: {studentName}"));
                    document.Add(new iTextSharp.text.Paragraph($"Курс: {courseName}"));
                    document.Add(new iTextSharp.text.Paragraph($"Дата выдачи: {DateTime.Now.ToShortDateString()}"));

                    document.Close();
                }

                MessageBox.Show("Сертификат успешно создан.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при создании сертификата: " + ex.Message);
            }
        }
    }
}