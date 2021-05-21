using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TestTaskGF.Models;

namespace TestTaskGF
{
    /// <summary>
    /// Логика взаимодействия для LeaderboardWindow.xaml
    /// </summary>
    public partial class LeaderboardWindow : Window
    {
        List<(string, int)> playerPtsList;
        const int MAXTOPPLAYERS = 10;
        public LeaderboardWindow()
        {
            InitializeComponent();
            playerPtsList = new List<(string, int)>();
        }

        private void LeaderboardWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Owner.Visibility = Visibility.Visible;
        }

        private void SetValuesForLeaderboard()
        {
            var path = Environment.CurrentDirectory.ToString() + "/leaderboard.json";
            if (!File.Exists(path))
                File.WriteAllText(path, "[]");
            string json = File.ReadAllText(path);
            var players = JsonSerializer.Deserialize<List<Player>>(json);

            players.Sort((x, y) => y.Points.CompareTo(x.Points));
            for (int i = 0; i < (players.Count > MAXTOPPLAYERS ? MAXTOPPLAYERS : players.Count); i++)
            {
                playerPtsList.Add((players[i].Name, players[i].Points));
            } 
        }

        private void LeaderboardWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SetValuesForLeaderboard();
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
