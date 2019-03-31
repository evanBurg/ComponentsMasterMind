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
                    Grid.SetColumn(ellipse, i);
                    grid.Children.Add(ellipse);
                }

                this.container.Children.Add(grid);
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
