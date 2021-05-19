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
    /// Логика взаимодействия для LeaderboardWindow.xaml
    /// </summary>
    public partial class LeaderboardWindow : Window
    {
        List<(string, int)> playerPtsList;
        const int playersCount = 100;
        const string lorem = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
        public LeaderboardWindow()
        {
            InitializeComponent();
            playerPtsList = new List<(string, int)>();
        }

        private void LeaderboardWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Owner.Visibility = Visibility.Visible;
        }

        private void SetValues(int count)
        {
            for (int i = 0; i < count; i++)
            {
                playerPtsList.Add(($"Player {i} {lorem}", i));
            }
        }

        private void LeaderboardWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SetValues(playersCount);
            foreach (var el in playerPtsList)
            {
                var playerPtsCont = new StackPanel();
                playerPtsCont.Orientation = Orientation.Horizontal;
                playerPtsCont.HorizontalAlignment = HorizontalAlignment.Center;

                var nameLabel = new Label();
                nameLabel.Content = el.Item1;
                nameLabel.FontSize = 30;
                nameLabel.Margin = new Thickness(30, 0, 30, 0);
                playerPtsCont.Children.Add(nameLabel);

                var scoreLabel = new Label();
                scoreLabel.Content = el.Item2;
                scoreLabel.FontSize = 30;
                scoreLabel.Margin = new Thickness(30, 0, 30, 0);
                playerPtsCont.Children.Add(scoreLabel);

                leaderboardContainer.Children.Add(playerPtsCont);
            }
        }
    }
}
