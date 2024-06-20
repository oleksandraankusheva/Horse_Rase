using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace _1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            BetIndexChanging();
            InitializeAnimationTimer();

            MinWidth = MaxWidth = 1000;
            MinHeight = MaxHeight = 650;
        }
        // Список коней
        private List<Horse> _horses = new List<Horse>();
        // Секундомір для відстеження часу гонки
        private readonly Stopwatch _raceStopwatch = new Stopwatch();
        // Таймер для анімації
        private readonly DispatcherTimer _animationTimer = new DispatcherTimer();
        // Список активних імен коней
        private List<string> _activeHorsesNames = new List<string> { "Patron" };
        // Список всіх імен коней
        public readonly List<string> _horsesNames = new List<string> { "Patron", "Tucker", "Willow", "Lucky", "Ranger" };
        // Список кольорів жокеїв
        public readonly List<Color> _jockeyColors = new List<Color> { Colors.Red, Colors.Orange, Colors.Yellow, Colors.Green, Colors.Blue };
        // Лічильник фінішованих коней
        private int _finishedCount;
        // Список ставок
        public List<int> Bets { get; } = new List<int> { 10, 20, 50, 100, 200, 300, 500, 1000 };


        // Індекс активного коня
        private int _currentActiveHorseIndex;

        private int CurrentActiveHorseIndex
        {
            get => _currentActiveHorseIndex;
            set
            {
                if (value < 0)
                {
                    _currentActiveHorseIndex = _activeHorsesNames.Count - 1;
                }
                else if (value > _activeHorsesNames.Count - 1)
                {
                    _currentActiveHorseIndex = 0;
                }
                else
                {
                    _currentActiveHorseIndex = value;
                }

                HorseIndexChanging();
            }
        }
        // Активний кінь
        public string CurrentActiveHorse => _activeHorsesNames[_currentActiveHorseIndex];
        // Індекс ставки
        private int _currentBetIndex;
        private int CurrentBetIndex
        {
            get => _currentBetIndex;
            set
            {
                if (value < 0)
                {
                    _currentBetIndex = Bets.Count - 1;
                }
                else if (value > Bets.Count - 1)
                {
                    _currentBetIndex = 0;
                }
                else
                {
                    _currentBetIndex = value;
                }

                BetIndexChanging();
            }
        }

        // Баланс
        private double _balance = 1000;
        public double Balance
        {
            get => _balance;
            private set
            {
                if (value <= 0)
                {
                    _balance = 0;
                    BetButton.IsEnabled = false;
                }
                else _balance = value;

                BalanceChanging();
            }
        }

        // Зміна відображення ставки
        private void BetIndexChanging() => BetDisplay.Text = $"{Bets[CurrentBetIndex]}$";
        // Зміна відображення балансу
        private void BalanceChanging() => DisplayBalance.Text = $"Balance: {Math.Round(Balance, 2)}$";
        // Зміна відображення активного коня
        private void HorseIndexChanging() => ActiveHorseNameDisplay.Text = _activeHorsesNames[CurrentActiveHorseIndex];

        // Ініціалізація коней
        private void InitializeHorses()
        {
            int offsetY = 180;
            int heightRaceTrack = 170;
            int numberHorses = int.Parse((NumberHorses.SelectedItem as ComboBoxItem)?.Content.ToString() ?? string.Empty);
            int space = heightRaceTrack / (numberHorses - 1);

            for (int i = 0; i < numberHorses; i++)
            {
                Horse horse = new Horse(_horsesNames[i], _jockeyColors[i], 20, offsetY, val => Balance += val);
                _horses.Add(horse);
                offsetY += space;

                RaceTrack.Children.Add(horse.HorseImage);
                RaceTrack.Children.Add(horse.JockeyImage);

                Canvas.SetLeft(horse.HorseImage, horse.PositionX);
                Canvas.SetTop(horse.HorseImage, horse.PositionY);
                Canvas.SetLeft(horse.JockeyImage, horse.PositionX);
                Canvas.SetTop(horse.JockeyImage, horse.PositionY - 30);
            }
        }


        // Ініціалізація таймера анімації
        private async void AnimationTimerTick(object sender, EventArgs e)
        {
            await UpdatePositionsHorse();
        }

        private void InitializeAnimationTimer()
        {
            _animationTimer.Interval = TimeSpan.FromMilliseconds(150);
            _animationTimer.Tick += AnimationTimerTick;
        }
        // Оновлення позицій коней
        private async Task UpdatePositionsHorse()
        {
            List<Task> distanceTasks = _horses.Select(horse => horse.MoveAsync()).ToList();
            await Task.WhenAll(distanceTasks);
            foreach (var horse in _horses)
            {
                Canvas.SetLeft(horse.HorseImage, horse.PositionX);
                Canvas.SetTop(horse.HorseImage, horse.PositionY);
                Canvas.SetLeft(horse.JockeyImage, horse.PositionX);
                Canvas.SetTop(horse.JockeyImage, horse.PositionY - 30);

                if (horse.PositionX >= 570 && !horse.Finished)

                {
                    horse.Time = _raceStopwatch.Elapsed;
                    _finishedCount++;

                    if (_finishedCount == _horses.Count)
                    {
                        StopRace();
                        break;
                    }
                }
            }
            _horses = _horses.OrderByDescending(horse => horse.PositionX).ToList();

            HorsesDataGrid.ItemsSource = null;
            HorsesDataGrid.ItemsSource = _horses;

            for (var i = 0; i < _horses.Count; i++)
            {
                _horses[i].CurrentPosition = i + 1;
            }
        }
        // Зупинка гонки
        private void StopRace()
        {
            ChooseButtons.IsEnabled = false;
            BetButton.IsEnabled = false;
            _animationTimer.Stop();
            _raceStopwatch.Stop();

            StartPanel.Visibility = Visibility.Visible;
            _horses.Clear();

            for (int i = RaceTrack.Children.Count - 1; i >= 0; i--)
            {
                UIElement child = RaceTrack.Children[i];

                if (child is FrameworkElement element && element.Tag as string == "tmp")
                {
                    RaceTrack.Children.RemoveAt(i);
                }
            }
        }

        // Обробка події натискання на кнопку старту
        private void Start_Button_Click(object sender, RoutedEventArgs e)
        {
            ChooseButtons.IsEnabled = true;
            _horses.Clear();
            InitializeHorses();

            _activeHorsesNames = _horses.Select(h => h.Name).ToList();
            BetButton.IsEnabled = Balance > 0;

            _animationTimer.Start();
            _raceStopwatch.Restart();

            StartPanel.Visibility = Visibility.Collapsed;
            _finishedCount = 0;
        }

        // Обробка події натискання на кнопку попередньої ставки
        private void Previous_Bet_Button_Click(object sender, RoutedEventArgs e) => CurrentBetIndex--;
        // Обробка події натискання на кнопку наступної ставки
        private void Next_Bet_Button_Click(object sender, RoutedEventArgs e) => CurrentBetIndex++;
        // Обробка події натискання на кнопку попереднього коня
        private void Previous_Horse_Button_Click(object sender, RoutedEventArgs e)
        {
            CurrentActiveHorseIndex--;
            if (_horses.Count != 0) BetButton.IsEnabled = !_horses.First(h => h.Name == _activeHorsesNames[CurrentActiveHorseIndex]).IsBidClosed && Balance > 0;
        }

        // Обробка події натискання на кнопку наступного коня
        private void Next_Horse_Button_Click(object sender, RoutedEventArgs e)
        {
            CurrentActiveHorseIndex++;
            if (_horses.Count != 0) BetButton.IsEnabled = !_horses.First(h => h.Name == _activeHorsesNames[CurrentActiveHorseIndex]).IsBidClosed && Balance > 0;
        }
        // Обробка події натискання на кнопку ставки
        private void Bet_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Bets[CurrentBetIndex] <= Balance)
            {
                var activeHorse = _horses.FirstOrDefault(h => h.Name == _activeHorsesNames[CurrentActiveHorseIndex]);

                if (activeHorse != null && !activeHorse.IsBidClosed)
                {
                    Balance -= Bets[CurrentBetIndex];

                    if (_horses.Count != 0)
                    {
                        activeHorse.Money += Bets[CurrentBetIndex];
                        BetButton.IsEnabled = false;
                    }
                }
                BetButton.IsEnabled = false;
            }
        }

        // Обробка події натискання на кнопку зупинки
        private void Stop_Button_Click(object sender, RoutedEventArgs e)
        {
            StopRace();
        }
    }
}