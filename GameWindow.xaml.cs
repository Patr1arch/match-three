using System;
using System.Collections.Generic;
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
        Random rand;

        enum Figure : int { Triangle = '\u25B2', Square = '\u25A0', Cicle = '\u25CF', Diamond = '\u25C6', Star = '\u2605'}
        public GameWindow()
        {
            InitializeComponent();
            rand = new Random();
            InitializeCells();          
        }

        private void InitializeCells()
        {
            for (int i = 0; i < CELLSCOUNT; i++)
            {
                for (int j = 0; j < CELLSCOUNT; j++)
                {
                    var btn = new GameButton();
                    var picture = (Figure)Enum.GetValues(typeof(Figure)).GetValue(rand.Next(0, Enum.GetValues(typeof(Figure)).Length));
                    btn.Content = (char)picture;
                    btn.Width = 40;
                    btn.Height = 40;
                    btn.FontSize = 30;
                    Grid.SetRow(btn, i);
                    Grid.SetColumn(btn, j);
                    gameGrid.Children.Add(btn);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
          
        }

        private void GameWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Owner.Visibility = Visibility.Visible;
        }
    }
}
