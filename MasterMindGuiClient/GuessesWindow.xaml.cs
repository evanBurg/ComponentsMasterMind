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
using System.Windows.Shapes;

namespace MasterMindGuiClient
{
    public partial class GuessesWindow : Window
    {
        private Dictionary<MasterMindLibrary.Colors, SolidColorBrush> solidColors;

        private void GenerateGuesses(List<List<MasterMindLibrary.Colors>> guesses)
        {
            foreach(List<MasterMindLibrary.Colors> guess in guesses)
            {
                MaterialDesignThemes.Wpf.Card card = new MaterialDesignThemes.Wpf.Card();
                card.Padding = new Thickness(5);
                card.Margin = new Thickness(5);
                Grid grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());

                for(int i = 0; i < guess.Count; i++)
                {
                    Ellipse ellipse = new Ellipse();
                    ellipse.Height = 50;
                    ellipse.Width = 50;
                    ellipse.Fill = solidColors[guess[i]];
                    ellipse.Stroke = new SolidColorBrush(Colors.Black);
                    ellipse.Margin = new Thickness(5);
                    Grid.SetColumn(ellipse, i);
                    grid.Children.Add(ellipse);
                }

                card.Content = grid;
                this.container.Children.Add(card);
            }
        }

        public GuessesWindow(List<List<MasterMindLibrary.Colors>> guesses, Dictionary<MasterMindLibrary.Colors, SolidColorBrush> solidColors)
        {
            InitializeComponent();
            this.solidColors = solidColors;
            GenerateGuesses(guesses);
        }
    }
}
