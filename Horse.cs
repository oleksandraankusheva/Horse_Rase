using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Color = System.Windows.Media.Color;
namespace _1
{
    public partial class Horse
    {
        // Зображення коня
        public Image HorseImage { get; set; }
        // Зображення жокея
        public Image JockeyImage { get; set; }
        // Ім'я коня
        public string Name { get; private set; }
        // Колір жокея
        public Color _color { get; private set; }
        // Прапорець, що позначає закриття ставки
        public bool IsBidClosed { get; private set; }
        // Швидкість коня
        private int Speed { get; set; }
        // Поточна позиція коня
        public int CurrentPosition { get; set; }
        // Позиція коня по осі X
        public int PositionX { get; private set; }
        // Позиція коня по осі Y
        public int PositionY { get; private set; }
        // Генератор випадкових чисел
        private readonly Random _random = new Random();
        // Делегат для зміни балансу
        private readonly Action<double> _changeBalance;
        // Час завершення гонки для коня
        private TimeSpan _time;
        // Властивість для часу завершення гонки
        public TimeSpan Time
        {
            get => _time;
            set
            {
                _time = value;
                Finished = true;
            }
        }
        // Кадр анімації
        private int _animationFrame;
        // Властивість для кадру анімації
        private int AnimationFrame
        {
            get => _animationFrame;
            set => _animationFrame = value % 12;
        }
        // Сума грошей на коні
        private double _money;
        // Властивість для суми грошей
        public double Money
        {
            get => _money;
            set
            {
                _money = Math.Round(value, 2);
                IsBidClosed = true;
            }
        }
        // Коефіцієнт для коня
        private double _coefficient;
        // Властивість для коефіцієнта
        public double Coefficient
        {
            get => _coefficient;
            set
            {
                _coefficient = value;
            }
        }
        // Прапорець, що позначає завершення гонки для коня
        private bool _finished;
        // Властивість для завершення гонки
        public bool Finished
        {
            get => _finished;
            private set
            {
                _finished = value;

                if (value)
                {
                    IsBidClosed = true;
                    if (CurrentPosition == 1) _changeBalance(Money * Coefficient);
                }
            }
        }
        // Конструктор класу
        public Horse(string name, Color color, int x, int y, Action<double> changeBalance)
        {
            Name = name;
            _color = color;
            IsBidClosed = false;
            _changeBalance += changeBalance;
            Speed = _random.Next(4, 11);
            Coefficient = 2 - Speed / 10.0;
            AnimationFrame = 0;
            HorseImage = InitializeHorseImage();
            JockeyImage = InitializeJockeyImage();
            PositionX = x;
            PositionY = y;
        }
        // Властивість для кольору жокея
        public Color Color
        {
            get => _color;
            private set => _color = value;
        }
        // Ініціалізація зображення жокея
        private Image InitializeJockeyImage()
        {
            return new Image
            {
                Source = new BitmapImage(new Uri($"Images/HorsesMask/mask_0000{_color.ToString().ToLower()}.png", UriKind.Relative)),
                Width = 100,
                Tag = "tmp",
                Margin = new Thickness(0, 30, 0, 0)
            };
        }
        // Ініціалізація зображення коня
        private Image InitializeHorseImage()
        {
            return new Image
            {
                Source = new BitmapImage(new Uri("Images/Horses/WithOutBorder_0000.png", UriKind.Relative)),
                Width = 100,
                Tag = "tmp"
            };
        }
        // Метод для переміщення коня асинхронно
        public async Task MoveAsync()
        {
            int distance = await Task.Run(() => (int)(Speed * (_random.Next(7, 11) / 10.0)));

            PositionX += distance;
            AnimationFrame++;
            string fileNumber = _animationFrame.ToString().Length > 1 ? _animationFrame.ToString() : "0" + _animationFrame;
            HorseImage.Source = new BitmapImage(new Uri($"Properties/WithOutBorder_00{fileNumber}.png", UriKind.Relative));
            string jockeyImagePathRed = $"Properties/mask_00{fileNumber}red.png";
            string jockeyImagePathOrange = $"Properties/mask_00{fileNumber}orange.png";
            string jockeyImagePathYellow = $"Properties/mask_00{fileNumber}yellow.png";
            string jockeyImagePathGreen = $"Properties/mask_00{fileNumber}green.png";
            string jockeyImagePathBlue = $"Properties/mask_00{fileNumber}blue.png";
            BitmapImage jockeyBitmap = null;

            if (_color == Colors.Red)
            {
                jockeyBitmap = new BitmapImage(new Uri(jockeyImagePathRed, UriKind.Relative));
            }
            else if (_color == Colors.Yellow)
            {
                jockeyBitmap = new BitmapImage(new Uri(jockeyImagePathYellow, UriKind.Relative));
            }
            else if (_color == Colors.Blue)
            {
                jockeyBitmap = new BitmapImage(new Uri(jockeyImagePathBlue, UriKind.Relative));
            }
            else if (_color == Colors.Green)
            {
                jockeyBitmap = new BitmapImage(new Uri(jockeyImagePathGreen, UriKind.Relative));
            }
            else
            {
                jockeyBitmap = new BitmapImage(new Uri(jockeyImagePathOrange, UriKind.Relative));
            }

            JockeyImage.Source = jockeyBitmap;

            if (PositionX >= 300)
            {
                IsBidClosed = true;
            }

            if (!IsBidClosed)
            {
                CalculateCoefficient();
            }
        }
        // Обчислення коефіцієнта для коня
        private void CalculateCoefficient()
        {
            Coefficient = Math.Round(2 - Speed / 10.0 + CurrentPosition / 10.0, 2);
        }
    }
}
