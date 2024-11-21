using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppMed
{
    public static class CaptchaGenerator
    {
        private static Random _random = new Random();

        public static string GenerateCaptchaText()
        {
            int length = _random.Next(4, 8); // от 4 до 7 символов
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] captchaChars = new char[length];
            for (int i = 0; i < length; i++)
            {
                captchaChars[i] = chars[_random.Next(chars.Length)];
            }
            return new string(captchaChars);
        }

        public static Bitmap GenerateCaptchaImage(string captchaText)
        {
            int width = 200;
            int height = 100;
            Bitmap bitmap = new Bitmap(width, height);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Генерация контрастного фона с шумом
            HatchStyle[] hatchStyles = { HatchStyle.BackwardDiagonal, HatchStyle.Cross, HatchStyle.DottedGrid, HatchStyle.Horizontal, HatchStyle.Vertical };
            HatchBrush hatchBrush = new HatchBrush(hatchStyles[_random.Next(hatchStyles.Length)], System.Drawing.Color.LightGray, System.Drawing.Color.White);
            graphics.FillRectangle(hatchBrush, 0, 0, width, height);

            // Добавление линий за текстом
            System.Drawing.Pen linePen = new System.Drawing.Pen(System.Drawing.Color.Gray, 1);

            for (int i = 0; i < 5; i++)
            {
                int x1 = _random.Next(width);
                int y1 = _random.Next(height);
                int x2 = _random.Next(width);
                int y2 = _random.Next(height);
                graphics.DrawLine(linePen, x1, y1, x2, y2);
            }

            // Параметры для текста
            int charX = 10;
            int charY = _random.Next(10, 40);
            for (int i = 0; i < captchaText.Length; i++)
            {
                // Случайный цвет символа
                System.Drawing.Color[] colors = { System.Drawing.Color.Black, System.Drawing.Color.Red, System.Drawing.Color.DarkBlue, System.Drawing.Color.Green, System.Drawing.Color.Brown, System.Drawing.Color.DarkCyan, System.Drawing.Color.Purple };
                SolidBrush textBrush = new SolidBrush(colors[_random.Next(colors.Length)]);

                // Случайный размер шрифта
                int fontSize = _random.Next(20, 35);

                // Случайный угол поворота
                float angle = _random.Next(-30, 30);

                // Создаем графический контейнер для поворота символа
                GraphicsState state = graphics.Save();
                graphics.TranslateTransform(charX, charY);
                graphics.RotateTransform(angle);

                // Рисуем символ
                Font font = new Font("Arial", fontSize, System.Drawing.FontStyle.Bold);
                graphics.DrawString(captchaText[i].ToString(), font, textBrush, 0, 0);

                // Восстанавливаем состояние графики
                graphics.Restore(state);

                // Обновляем позицию для следующего символа
                charX += fontSize - 10; // символы слегка прикасаются

                // Случайное смещение по Y
                charY = _random.Next(10, 40);
            }

            // Добавление линий на текст
            for (int i = 0; i < 5; i++)
            {
                int x1 = _random.Next(width);
                int y1 = _random.Next(height);
                int x2 = _random.Next(width);
                int y2 = _random.Next(height);
                graphics.DrawLine(linePen, x1, y1, x2, y2);
            }

            graphics.Dispose();

            // Сохраняем изображение (при необходимости)
            // bitmap.Save("captcha.png", ImageFormat.Png);

            return bitmap;
        }
    }
}
