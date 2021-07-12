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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TestTaskGF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Start_Game(object sender, RoutedEventArgs e)
        {
            GameWindow gameWindow = new GameWindow((string)((Button)sender).Content != "Play");
            this.Visibility = Visibility.Collapsed;
            gameWindow.Owner = this;
            gameWindow.Show();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LeaderboardWindow leaderboardWindow = new LeaderboardWindow();
            this.Visibility = Visibility.Collapsed;
            leaderboardWindow.Owner = this;
            leaderboardWindow.Show();
        }
    }
}
