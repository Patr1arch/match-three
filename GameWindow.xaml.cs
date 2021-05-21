using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using TestTaskGF.Models;

namespace TestTaskGF
{
    /// <summary>
    /// Логика взаимодействия для GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Window
    {
        const int CELLSCOUNT = 8;
        const int MINMATCHCOUNT = 3;
        const int STARTTIME = 60;
        const int MAXNICKNAMELENGTH = 16;
        Random rand;
        Tuple<int, int> selectedPos;
        int points;
        int timeRemain = STARTTIME;
        private static SemaphoreSlim slowStuffSemaphore = new SemaphoreSlim(1, 1);
        DispatcherTimer dispatcherTimer;


        public GameWindow()
        {
            InitializeComponent();
            ConfigureGame();         
        }

        private void ConfigureGame()
        {
            rand = new Random();
            selectedPos = null;
            points = 0;
            InitializeCells();
            while (true)
            {
                var matchList = CheckAllLines();
                foreach (var pair in matchList)
                {
                    SiftDown(pair);
                }
                if (matchList.Count == 0)
                    break;
            }
        }

        private void SiftDown(Tuple<int, int> pair)
        {
            if (pair.Item1 == 0)
            {
                GetGameButton(pair).CurrentFigure =
                        (Figure)Enum.GetValues(typeof(Figure)).GetValue(rand.Next(0, Enum.GetValues(typeof(Figure)).Length));
            }
            else
            {
                GetGameButton(pair).SwapFigures(
                    GetGameButton(new Tuple<int, int>(pair.Item1 - 1, pair.Item2)));
                SiftDown(new Tuple<int, int>(pair.Item1 - 1, pair.Item2));
            }
        }

        private GameButton GetGameButton(Tuple<int, int> pair)
        {
            return pair.Item1 < 0 || pair.Item2 < 0 || pair.Item1 >= CELLSCOUNT || pair.Item2 > CELLSCOUNT ? null :
                gameGrid.Children.Cast<GameButton>()
                        .First(p => Grid.GetRow(p) == pair.Item1 && Grid.GetColumn(p) == pair.Item2);
        }

        private void InitializeCells()
        {
            for (int i = 0; i < CELLSCOUNT; i++)
            {
                for (int j = 0; j < CELLSCOUNT; j++)
                {
                    var btn = new GameButton();
                    btn.CurrentFigure = (Figure)Enum.GetValues(typeof(Figure)).GetValue(rand.Next(0, Enum.GetValues(typeof(Figure)).Length));
                    // btn.Content = (char)btn.CurrentFigure;
                    btn.Width = 40;
                    btn.Height = 40;
                    btn.FontSize = 30;
                    btn.Background = Brushes.Bisque;
                    Grid.SetRow(btn, i);
                    Grid.SetColumn(btn, j);
                    btn.Click += Button_Click;
                    gameGrid.Children.Add(btn);
                }
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            //var btn = (GameButton)(sender);
            //int i = Grid.GetRow(btn);
            //int j = Grid.GetColumn(btn);
            //var newBtn = gameGrid.Children.Cast<GameButton>()
            //    .First(p => Grid.GetRow(p) == i && Grid.GetColumn(p) == j);
            if (slowStuffSemaphore.CurrentCount == 0) return;
            await slowStuffSemaphore.WaitAsync();
            if (selectedPos == null)
            {
                selectedPos = new Tuple<int, int>(Grid.GetRow((GameButton)sender), Grid.GetColumn((GameButton)sender));
                
                GetGameButton(selectedPos).IsEnabled = false;
            }
            else {
                var newSelectedPos = new Tuple<int, int>(Grid.GetRow((GameButton)sender), Grid.GetColumn((GameButton)sender));
                GetGameButton(selectedPos).IsEnabled = true;
                if (CheckAdjacent(selectedPos, newSelectedPos))
                {
                    await InteractButtons(newSelectedPos);
                }       
                selectedPos = null;
            }
            slowStuffSemaphore.Release();
        }

        private async Task InteractButtons(Tuple<int, int> newSelectedPos)
        {
            int newPts = 0;
            GetGameButton(selectedPos).SwapFigures(GetGameButton(newSelectedPos));
            var matchCoords = CheckAllLines();
            if (matchCoords.Count > 0)
            {
                while (matchCoords.Count > 0)
                {
                    foreach (var el in matchCoords)
                    {
                        GetGameButton(el).Background = Brushes.Red;
                    }
                    SystemSounds.Beep.Play();
                    await Task.Delay(500);
                    foreach (var el in matchCoords)
                    {
                        GetGameButton(el).Background = Brushes.Bisque;
                        SiftDown(el);
                        newPts++;
                    }
                    matchCoords = CheckAllLines();
                }
                points += newPts;
                ptsLabel.Content = "Points: " + points.ToString();
            }
            else
            {
                GetGameButton(selectedPos).SwapFigures(GetGameButton(newSelectedPos));
            }
        }

        private bool CheckAdjacent(Tuple<int, int> firstSelPos, Tuple<int, int> secondSelPos)
        {
            return (firstSelPos.Item1 + 1 == secondSelPos.Item1 && firstSelPos.Item2 == secondSelPos.Item2) ||
                (firstSelPos.Item1 - 1 == secondSelPos.Item1 && firstSelPos.Item2 == secondSelPos.Item2) ||
                (firstSelPos.Item1 == secondSelPos.Item1 && firstSelPos.Item2 + 1 == secondSelPos.Item2) ||
                (firstSelPos.Item1 == secondSelPos.Item1 && firstSelPos.Item2 - 1 == secondSelPos.Item2);
        }

        private HashSet<Tuple<int, int>> CheckLines(int i, int j)
        {
            HashSet<Tuple<int, int>> matchCoords = new HashSet<Tuple<int, int>>();
            HashSet<Tuple<int, int>> tempMatchCoords = new HashSet<Tuple<int, int>>();
            for (int colJ = 0; colJ < CELLSCOUNT - 1; colJ++)
            {
                if (gameGrid.Children.Cast<GameButton>().First(p => Grid.GetRow(p) == i && Grid.GetColumn(p) == colJ).CurrentFigure ==
                    gameGrid.Children.Cast<GameButton>().First(p => Grid.GetRow(p) == i && Grid.GetColumn(p) == colJ + 1).CurrentFigure)
                {
                    if (tempMatchCoords.Count == 0)
                    {
                        tempMatchCoords.Add(new Tuple<int, int>(i, colJ));
                    }
                    tempMatchCoords.Add(new Tuple<int, int>(i, colJ + 1));
                }
                else if (tempMatchCoords.Count >= MINMATCHCOUNT)
                {
                    foreach (var el in tempMatchCoords)
                        matchCoords.Add(el);
                    tempMatchCoords.Clear();
                }
                else tempMatchCoords.Clear();
            }
            if (tempMatchCoords.Count >= MINMATCHCOUNT)
            {
                foreach (var el in tempMatchCoords)
                    matchCoords.Add(el);
            }
            tempMatchCoords.Clear();
            for (int rowI = 0; rowI < CELLSCOUNT - 1; rowI++)
            {
                if (gameGrid.Children.Cast<GameButton>().First(p => Grid.GetRow(p) == rowI && Grid.GetColumn(p) == j).CurrentFigure ==
                    gameGrid.Children.Cast<GameButton>().First(p => Grid.GetRow(p) == rowI + 1 && Grid.GetColumn(p) == j).CurrentFigure)
                {
                    if (tempMatchCoords.Count == 0)
                    {
                        tempMatchCoords.Add(new Tuple<int, int>(rowI, j));
                    }
                    tempMatchCoords.Add(new Tuple<int, int>(rowI + 1, j));
                }
                else if (tempMatchCoords.Count >= MINMATCHCOUNT)
                {
                    foreach (var el in tempMatchCoords)
                        matchCoords.Add(el);
                    tempMatchCoords.Clear();
                }
                else tempMatchCoords.Clear();
            }
            if (tempMatchCoords.Count >= MINMATCHCOUNT)
            {
                foreach (var el in tempMatchCoords)
                    matchCoords.Add(el);
            }
            return matchCoords;
        }

        private HashSet<Tuple<int, int>> CheckAllLines()
        {
            HashSet<Tuple<int, int>> matchCoords = new HashSet<Tuple<int, int>>();
            for (int i = 0; i < CELLSCOUNT; i++)
            {
                foreach(var el in CheckLines(i, i))
                {
                    matchCoords.Add(el);
                }
            }
            return matchCoords;
        }

        private void GameWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Owner.Visibility = Visibility.Visible;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(DispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (timeRemain == 0)
            {
                dispatcherTimer.Stop();
                inputBox.Visibility = System.Windows.Visibility.Visible;
                //InputBox.Show("Game over!", "Game Over");
                //Close();
            }
            timeLabel.Content = "Time remaining: " + timeRemain--.ToString();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            inputBox.Visibility = System.Windows.Visibility.Collapsed;
            var path = Environment.CurrentDirectory.ToString() + "/leaderboard.json";

            if (!File.Exists(path))
            {
                File.WriteAllText(path, "[]");
            }
            string json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<List<Player>>(json);
            data.Add(new Player(inputTextBox.Text, points));
            json = JsonSerializer.Serialize(data);
            File.WriteAllText(path, json);

            Close();
        }

        private void inputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (inputTextBox.Text.Length >= MAXNICKNAMELENGTH)
            {
                inputTextBox.Text = inputTextBox.Text.Substring(0, MAXNICKNAMELENGTH);
                inputTextBox.SelectionStart = inputTextBox.Text.Length;
            }
        }
    }
}
