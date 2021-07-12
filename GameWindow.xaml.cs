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
        private const int MATCHCOUNTFORLINE = 4;
        private const int MATCHCOUNTFORBOMB = 5;
        const int STARTTIME = 600;
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
                foreach (var triple in matchList)
                {
                    SiftDown(new Tuple<int, int>(triple.Item1, triple.Item2));
                }
                if (matchList.Count == 0)
                    break;
            }
        }

        private void SiftDown(Tuple<int, int> pair)
        {
            if (pair.Item1 == 0)
            {
                var index = rand.Next(0, Enum.GetValues(typeof(FigureColor)).Length);
                GetGameButton(pair).CurrentFigure = CreateFigure(index, index);
                //(Figure)Enum.GetValues(typeof(Figure)).GetValue(rand.Next(0, Enum.GetValues(typeof(Figure)).Length));
            }
            else
            {
                GetGameButton(pair).SwapFigures(
                    GetGameButton(new Tuple<int, int>(pair.Item1 - 1, pair.Item2)));
                SiftDown(new Tuple<int, int>(pair.Item1 - 1, pair.Item2));
            }
        }

        private (Figure, FigureColor) CreateFigure(int figureIndex, int colorIndex)
        {
            return (Enum.GetValues(typeof(Figure)).Cast<Figure>().ToArray().OrderBy(l => l.ToString()).ToArray()[figureIndex],
                (FigureColor)Enum.GetValues(typeof(FigureColor)).GetValue(colorIndex));
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
                    var index = rand.Next(0, Enum.GetValues(typeof(FigureColor)).Length);
                    btn.CurrentFigure = CreateFigure(index, index);
                    // btn.Content = (char)btn.CurrentFigure;
                    btn.Width = 40;
                    btn.Height = 40;
                    btn.FontSize = 30;
                    btn.Background = Brushes.Bisque;
                    Grid.SetRow(btn, i);
                    Grid.SetColumn(btn, j);
                    btn.Click += Button_Click;
                    btn.PreviewMouseDoubleClick += Button_PreviewMouseDown;
                    gameGrid.Children.Add(btn);
                }
            }
        }
        
        private void Button_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                var btn = GetGameButton(new Tuple<int, int>(Grid.GetRow((GameButton)sender),
                        Grid.GetColumn((GameButton)sender)));
                int index = Array.IndexOf(Enum.GetValues(btn.CurrentFigure.Item1.GetType()), btn.CurrentFigure.Item1);
                if (index < Enum.GetValues(typeof(FigureColor)).Length - 1)
                    btn.CurrentFigure = CreateFigure(index + 1, index + 1);
                else
                {
                    if (index == Enum.GetValues(typeof(Figure)).Length - 1)
                        btn.CurrentFigure = CreateFigure(0, 0);
                    else
                        btn.CurrentFigure = CreateFigure(index + 1, 
                            Array.IndexOf(Enum.GetValues(btn.CurrentFigure.Item2.GetType()),
                                    btn.CurrentFigure.Item2));
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
            var matchCoords = CheckAllLines(newSelectedPos);
            if (matchCoords.Count > 0)
            {
                while (matchCoords.Count > 0)
                {
                    foreach (var el in matchCoords)
                    {
                        GetGameButton(new Tuple<int, int>(el.Item1, el.Item2)).Background = Brushes.Red;
                    }
                    SystemSounds.Beep.Play();
                    await Task.Delay(2000);
                    foreach (var el in matchCoords)
                    {
                        GetGameButton(new Tuple<int, int>(el.Item1, el.Item2)).Background = Brushes.Bisque;
                        if (el.Item3 != 0)
                        {
                            GetGameButton(new Tuple<int, int>(el.Item1, el.Item2)).CurrentFigure =
                                ((Figure) el.Item3,
                                    GetGameButton(new Tuple<int, int>(el.Item1, el.Item2)).CurrentFigure.Item2);
                        }
                        else
                            SiftDown(new Tuple<int, int>(el.Item1, el.Item2));
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

        private HashSet<Tuple<int, int, int>> CheckLines(int i, int j, Tuple<int, int> newSelectedPos = null)
        {
            HashSet<Tuple<int, int, int>> matchCoords = new HashSet<Tuple<int, int, int>>();
            HashSet<Tuple<int, int>> tempMatchCoords = new HashSet<Tuple<int, int>>();
            for (int colJ = 0; colJ < CELLSCOUNT; colJ++)
            {
                if (colJ != CELLSCOUNT - 1 &&
                    gameGrid.Children.Cast<GameButton>().First(p => Grid.GetRow(p) == i && Grid.GetColumn(p) == colJ).CurrentFigure.Item2 ==
                    gameGrid.Children.Cast<GameButton>().First(p => Grid.GetRow(p) == i && Grid.GetColumn(p) == colJ + 1).CurrentFigure.Item2)
                {
                    if (tempMatchCoords.Count == 0)
                    {
                        tempMatchCoords.Add(new Tuple<int, int>(i, colJ));
                    }
                    tempMatchCoords.Add(new Tuple<int, int>(i, colJ + 1));
                }
                else if (tempMatchCoords.Count >= MINMATCHCOUNT)
                {
                    if (tempMatchCoords.Any(p => GetGameButton(p).CurrentFigure.Item1 == Figure.F6GorLine))
                    {
                        var lineEls = tempMatchCoords.ToList().FindAll
                            (p => GetGameButton(p).CurrentFigure.Item1 == Figure.F6GorLine);
                        foreach (var lineEl in lineEls)
                            matchCoords.UnionWith(GetReaction(lineEl, Figure.F6GorLine));
                    }
                    if (tempMatchCoords.Any(p => GetGameButton(p).CurrentFigure.Item1 == Figure.F7VerLine))
                    {
                        var colEls = tempMatchCoords.ToList().FindAll
                            (p => GetGameButton(p).CurrentFigure.Item1 == Figure.F7VerLine);
                        foreach (var colEl in colEls)
                            matchCoords.UnionWith(GetReaction(colEl, Figure.F7VerLine));
                    }
                    if (tempMatchCoords.Count == MATCHCOUNTFORLINE)
                    {
                        if (tempMatchCoords.Contains(newSelectedPos))
                        {
                            if (matchCoords.Contains(new Tuple<int, int, int>(newSelectedPos.Item1,
                                newSelectedPos.Item2, 0)))
                                matchCoords.Remove(new Tuple<int, int, int>(newSelectedPos.Item1, newSelectedPos.Item2,
                                    0));
                            matchCoords.Add(new Tuple<int, int, int>(newSelectedPos.Item1, newSelectedPos.Item2,
                                (int) Figure.F6GorLine));
                            tempMatchCoords.Remove(newSelectedPos);
                        }
                        else
                        {
                            var el = tempMatchCoords.ToArray()[tempMatchCoords.Count - 1];
                            if (matchCoords.Contains(new Tuple<int, int, int>(el.Item1, el.Item2, 0)))
                                matchCoords.Remove(new Tuple<int, int, int>(el.Item1,
                                    el.Item2, 0));
                            matchCoords.Add(new Tuple<int, int, int>(el.Item1, el.Item2,
                                (int) Figure.F6GorLine));
                            tempMatchCoords.Remove(el);
                        }
                    }
                    else if (tempMatchCoords.Count >= MATCHCOUNTFORBOMB)
                    {
                        if (tempMatchCoords.Contains(newSelectedPos))
                        {
                            if (matchCoords.Contains(new Tuple<int, int, int>(newSelectedPos.Item1,
                                newSelectedPos.Item2, 0)))
                                matchCoords.Remove(new Tuple<int, int, int>(newSelectedPos.Item1, newSelectedPos.Item2,
                                    0));
                            matchCoords.Add(new Tuple<int, int, int>(newSelectedPos.Item1, newSelectedPos.Item2,
                                (int) Figure.F8Bomb));
                            tempMatchCoords.Remove(newSelectedPos);
                        }
                        else
                        {
                            var el = tempMatchCoords.ToArray()[tempMatchCoords.Count - 1];
                            if (matchCoords.Contains(new Tuple<int, int, int>(el.Item1, el.Item2, 0)))
                                matchCoords.Remove(new Tuple<int, int, int>(el.Item1,
                                    el.Item2, 0));
                            matchCoords.Add(new Tuple<int, int, int>(el.Item1, el.Item2,
                                (int) Figure.F8Bomb));
                            tempMatchCoords.Remove(el);
                        }
                    }
                    foreach (var el in tempMatchCoords)
                    {
                        matchCoords.Add(new Tuple<int, int, int>(el.Item1, el.Item2, 0));   
                    }
                    tempMatchCoords.Clear();
                }
                else tempMatchCoords.Clear();
            }
            tempMatchCoords.Clear();
            for (int rowI = 0; rowI < CELLSCOUNT; rowI++)
            {
                if (rowI != CELLSCOUNT - 1 &&
                    gameGrid.Children.Cast<GameButton>().First(p => Grid.GetRow(p) == rowI && Grid.GetColumn(p) == j).CurrentFigure.Item2 ==
                    gameGrid.Children.Cast<GameButton>().First(p => Grid.GetRow(p) == rowI + 1 && Grid.GetColumn(p) == j).CurrentFigure.Item2)
                {
                    if (tempMatchCoords.Count == 0)
                    {
                        tempMatchCoords.Add(new Tuple<int, int>(rowI, j));
                    }
                    tempMatchCoords.Add(new Tuple<int, int>(rowI + 1, j));
                }
                else if (tempMatchCoords.Count >= MINMATCHCOUNT)
                {
                    if (tempMatchCoords.Any(p => GetGameButton(p).CurrentFigure.Item1 == Figure.F6GorLine))
                    {
                        var lineEls = tempMatchCoords.ToList().FindAll
                            (p => GetGameButton(p).CurrentFigure.Item1 == Figure.F6GorLine);
                        foreach (var lineEl in lineEls)
                            matchCoords.UnionWith(GetReaction(lineEl, Figure.F6GorLine));
                    }
                    if (tempMatchCoords.Any(p => GetGameButton(p).CurrentFigure.Item1 == Figure.F7VerLine))
                    {
                        var colEls = tempMatchCoords.ToList().FindAll
                            (p => GetGameButton(p).CurrentFigure.Item1 == Figure.F7VerLine);
                        foreach (var colEl in colEls)
                            matchCoords.UnionWith(GetReaction(colEl, Figure.F7VerLine));
                    }
                    if (tempMatchCoords.Count == MATCHCOUNTFORLINE)
                    {
                        if (tempMatchCoords.Contains(newSelectedPos))
                        {
                            if (matchCoords.Contains(new Tuple<int, int, int>(newSelectedPos.Item1,
                                newSelectedPos.Item2, 0)))
                                matchCoords.Remove(new Tuple<int, int, int>(newSelectedPos.Item1, newSelectedPos.Item2,
                                    0));
                            matchCoords.Add(new Tuple<int, int, int>(newSelectedPos.Item1, newSelectedPos.Item2,
                                (int) Figure.F7VerLine));
                            tempMatchCoords.Remove(newSelectedPos);
                        }
                        else
                        {
                            var el = tempMatchCoords.ToArray()[tempMatchCoords.Count - 1];
                            if (matchCoords.Contains(new Tuple<int, int, int>(el.Item1,
                                el.Item2, 0)))
                                matchCoords.Remove(new Tuple<int, int, int>(el.Item1, el.Item2,
                                    0));
                            matchCoords.Add(new Tuple<int, int, int>(el.Item1, el.Item2,
                                (int) Figure.F7VerLine));
                            tempMatchCoords.Remove(el);
                        }
                    }
                    else if (tempMatchCoords.Count >= MATCHCOUNTFORBOMB)
                    {
                        if (tempMatchCoords.Contains(newSelectedPos))
                        {
                            if (matchCoords.Contains(new Tuple<int, int, int>(newSelectedPos.Item1,
                                newSelectedPos.Item2, 0)))
                                matchCoords.Remove(new Tuple<int, int, int>(newSelectedPos.Item1, newSelectedPos.Item2,
                                    0));
                            matchCoords.Add(new Tuple<int, int, int>(newSelectedPos.Item1, newSelectedPos.Item2,
                                (int) Figure.F8Bomb));
                            tempMatchCoords.Remove(newSelectedPos);
                        }
                        else
                        {
                            var el = tempMatchCoords.ToArray()[tempMatchCoords.Count - 1];
                            if (matchCoords.Contains(new Tuple<int, int, int>(el.Item1,
                                el.Item2, 0)))
                                matchCoords.Remove(new Tuple<int, int, int>(el.Item1, el.Item2,
                                    0));
                            matchCoords.Add(new Tuple<int, int, int>(el.Item1, el.Item2,
                                (int) Figure.F8Bomb));
                            tempMatchCoords.Remove(el);
                        }
                    }
                    foreach (var el in tempMatchCoords)
                    {
                        matchCoords.Add(new Tuple<int, int, int>(el.Item1, el.Item2, 0)); 
                    }
                    tempMatchCoords.Clear();
                }
                else tempMatchCoords.Clear();
            }
            return matchCoords;
        }

        private IEnumerable<Tuple<int, int, int>> GetReaction(Tuple<int, int> lineEl, Figure figure)
        {
            HashSet<Tuple<int, int, int>> cont = new HashSet<Tuple<int, int, int>>();
            switch (figure)
            {
                case Figure.F6GorLine:
                    for (int k = 0; k < CELLSCOUNT; k++)
                    {
                        var fig = GetGameButton(new Tuple<int, int>(lineEl.Item1, k));
                        if ((fig.CurrentFigure.Item1 == Figure.F8Bomb
                            || fig.CurrentFigure.Item1 == Figure.F7VerLine) && k != lineEl.Item2)
                            cont.UnionWith(GetReaction(new Tuple<int, int>(lineEl.Item1, k), 
                                fig.CurrentFigure.Item1));
                        else
                            cont.Add(new Tuple<int, int, int>(lineEl.Item1, k, 0));
                    }
                    break;
                case Figure.F7VerLine:
                    for (int k = 0; k < CELLSCOUNT; k++)
                    {
                        var fig = GetGameButton(new Tuple<int, int>(k, lineEl.Item2));
                        if ((fig.CurrentFigure.Item1 == Figure.F8Bomb
                            || fig.CurrentFigure.Item1 == Figure.F6GorLine) && k != lineEl.Item1)
                            cont.UnionWith(GetReaction(new Tuple<int, int>(k, lineEl.Item2), 
                                fig.CurrentFigure.Item1));
                        else
                            cont.Add(new Tuple<int, int, int>(k, lineEl.Item2, 0));
                    }
                    break;
            }

            return cont;
        }

        private HashSet<Tuple<int, int, int>> CheckAllLines(Tuple<int, int> newSelectedPos = null)
        {
            HashSet<Tuple<int, int>> matchCoords = new HashSet<Tuple<int, int>>();
            HashSet<Tuple<int, int, int>> matchFigures = new HashSet<Tuple<int, int, int>>();
            for (int i = 0; i < CELLSCOUNT; i++)
            {
                foreach(var el in CheckLines(i, i, newSelectedPos))
                {
                    var pair = new Tuple<int, int>(el.Item1, el.Item2);
                    if (matchCoords.Contains(pair))
                    {
                        matchFigures.Remove(matchFigures.First(p => p.Item1 == pair.Item1 &&
                                                                    p.Item2 == pair.Item2));
                        matchFigures.Add(new Tuple<int, int, int>(pair.Item1, pair.Item2, (int) Figure.F8Bomb));
                        matchCoords.Remove(pair);
                    }
                    else
                    {
                        matchFigures.Add(el);
                        matchCoords.Add(pair);
                    }
                        
                }
            }
            return matchFigures;
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
