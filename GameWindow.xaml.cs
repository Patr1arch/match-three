using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TestTaskGF
{
    /// <summary>
    /// Логика взаимодействия для GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Window
    {
        const int CELLSCOUNT = 8;
        const int MINMATCHCOUNT = 3;
        Random rand;
        Tuple<int, int> selectedPos;

        public GameWindow()
        {
            InitializeComponent();
            ConfigureGame();         
        }

        private void ConfigureGame()
        {
            rand = new Random();
            selectedPos = null;
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
                    // btn.Background = Brushes.White;
                    Grid.SetRow(btn, i);
                    Grid.SetColumn(btn, j);
                    btn.Click += Button_Click;
                    gameGrid.Children.Add(btn);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //var btn = (GameButton)(sender);
            //int i = Grid.GetRow(btn);
            //int j = Grid.GetColumn(btn);
            //var newBtn = gameGrid.Children.Cast<GameButton>()
            //    .First(p => Grid.GetRow(p) == i && Grid.GetColumn(p) == j);
            if (selectedPos == null)
            {
                selectedPos = new Tuple<int, int>(Grid.GetRow((GameButton)sender), Grid.GetColumn((GameButton)sender));
                
                GetGameButton(selectedPos).IsEnabled = false;
            }
            else {
                if (CheckAdjacent(selectedPos,
                new Tuple<int, int>(Grid.GetRow((GameButton)sender), Grid.GetColumn((GameButton)sender))))
                {
                    debBtn.Content = "Yes";
                }
                else
                {
                    debBtn.Content = "No";
                }
                GetGameButton(selectedPos).IsEnabled = true;
                selectedPos = null;
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < CELLSCOUNT; i++)
            {
                foreach (var pair in CheckLines(i, i))
                {
                    gameGrid.Children.Cast<GameButton>()
                        .First(p => Grid.GetRow(p) == pair.Item1 && Grid.GetColumn(p) == pair.Item2).Background = Brushes.Red;
                    
                }

            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            while (true)
            {
                var matchList = CheckAllLines();
                foreach (var pair in matchList)
                {
                    SiftDown(pair);
                }
                if (matchList.Count ==  0)
                    break;
            }


        }
    }
}
