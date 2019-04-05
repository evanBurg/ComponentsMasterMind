using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ServiceModel;
using MasterMindLibrary;
using System.Windows.Shapes;
using System.Windows.Media;
using MasterMindGuiClient;
using System.Windows.Controls;

namespace MasterMindGUI
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public partial class MainWindow : Window, ICallback
    {
        // Member variables
        private ICodeMaker codeMaker = null;
        private List<MasterMindLibrary.Colors> selected;
        private Dictionary<MasterMindLibrary.Colors, SolidColorBrush> solidColors;
        private bool callbacksEnabled = false;
        private List<List<MasterMindLibrary.Colors>> guesses;
        const int MAX_GUESSES = 8;
        string name;
        private bool finished = false;
        private bool connected = false;

        public MainWindow(string ip, string name)
        {
            InitializeComponent();

            try
            {
                this.window.Title += " | " + name;
                this.name = name;
                // Connect to the WCF service endpoint called "ShoeService" 
                this.backspace.IsEnabled = false;
                var netTcp = new NetTcpBinding();
                netTcp.Security = new NetTcpSecurity();
                netTcp.Security.Mode = SecurityMode.None;

                DuplexChannelFactory<ICodeMaker> channel = new DuplexChannelFactory<ICodeMaker>(this, netTcp, new EndpointAddress("net.tcp://" + ip + ":13200/MasterMindLibrary/MasterService"));
                codeMaker = channel.CreateChannel();
                this.submit.IsEnabled = false;

                try
                {
                    connected = true;
                    callbacksEnabled = codeMaker.ToggleCallbacks();
                    HasSomeoneWon();
                }
                catch (Exception)
                {
                    MessageBox.Show(String.Format("There was an issue connecting to '{0}'. Please check the entered address as well as your network status and try again", ip));
                    connected = false;
                }

                if (connected)
                {
                    selected = new List<MasterMindLibrary.Colors>();
                    solidColors = new Dictionary<MasterMindLibrary.Colors, SolidColorBrush>();
                    guesses = new List<List<MasterMindLibrary.Colors>>();

                    solidColors.Add(MasterMindLibrary.Colors.Red, new SolidColorBrush(System.Windows.Media.Colors.Red));
                    solidColors.Add(MasterMindLibrary.Colors.Green, new SolidColorBrush(System.Windows.Media.Colors.Green));
                    solidColors.Add(MasterMindLibrary.Colors.Blue, new SolidColorBrush(System.Windows.Media.Colors.Blue));
                    solidColors.Add(MasterMindLibrary.Colors.Yellow, new SolidColorBrush(System.Windows.Media.Colors.Yellow));
                    solidColors.Add(MasterMindLibrary.Colors.Pink, new SolidColorBrush(System.Windows.Media.Colors.Pink));
                    solidColors.Add(MasterMindLibrary.Colors.Purple, new SolidColorBrush(System.Windows.Media.Colors.Purple));
                    App.Current.joinWindow.Close();
                    this.Show();
                }
                else
                {
                    App.Current.joinWindow = new JoinWindow();
                    App.Current.joinWindow.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void HasSomeoneWon()
        {
            string name = codeMaker.HasSomeoneWon(this.name);
            if (name == "exists")
            {
                connected = false;
                MessageBox.Show(String.Format("Someone has already joined the server with the name '{0}'", this.name));
            }
            else if (name != "")
            {
                codeMaker.IsCorrect(selected, name);
            }
        }

        private delegate void ClientUpdateDelegate(CallbackInfo info);
        private delegate void ClientUserDelegate(Dictionary<string, int> info);

        public void SomeoneWon(CallbackInfo info)
        {
            if (System.Threading.Thread.CurrentThread == this.Dispatcher.Thread)
            {
                if (info.someoneWon == false)
                {
                    // Update the GUI
                    MessageBox.Show("A user won: " + info.name);
                    this.window.Title += " | FINISHED";
                    if (info.name == this.name)
                    {
                        winnerCard.Visibility = Visibility.Visible;
                        SolidColorBrush green = new SolidColorBrush(System.Windows.Media.Colors.Green);
                        winnerText.Foreground = green;
                        winnerText.Text = "WINNER!";
                        this.backspace.IsEnabled = false;
                        this.submit.IsEnabled = false;
                        this.finished = true;
                    }
                    else
                    {
                        winnerCard.Visibility = Visibility.Visible;
                        SolidColorBrush red = new SolidColorBrush(System.Windows.Media.Colors.Red);
                        winnerText.Foreground = red;
                        winnerText.Text = "LOSER!";
                        this.backspace.IsEnabled = false;
                        this.submit.IsEnabled = false;
                        this.finished = true;
                    }
                }
                else
                {
                    if (!finished)
                    {
                        this.window.Title += " | FINISHED";
                        MessageBox.Show("A user has already won: " + info.name);
                        finished = true;
                        this.backspace.IsEnabled = false;
                        this.submit.IsEnabled = false;
                        winnerCard.Visibility = Visibility.Visible;
                        winnerText.Text = "FINISHED!";
                    }
                }
                this.submit.IsEnabled = false;
            }
            else
            {
                // Only the main (dispatcher) thread can change the GUI
                this.Dispatcher.BeginInvoke(new ClientUpdateDelegate(SomeoneWon), info);
            }
        }

        private void changeColour(SolidColorBrush colour, int index)
        {
            switch (index)
            {
                case 0:
                    first.Fill = colour;
                    break;
                case 1:
                    second.Fill = colour;
                    break;
                case 2:
                    third.Fill = colour;
                    break;
                case 3:
                    fourth.Fill = colour;
                    break;
            }

            if (selected.Count == 4)
                this.submit.IsEnabled = true;

            if (selected.Count >= 1)
                this.backspace.IsEnabled = true;
            else
                this.backspace.IsEnabled = false;
        }

        private void updateColours()
        {
            SolidColorBrush almond = new SolidColorBrush();
            almond.Color = System.Windows.Media.Colors.BlanchedAlmond;
            for (int i = 0; i < 4; i++)
            {
                if (selected.Count > i)
                {
                    changeColour(solidColors[selected[i]], i);
                }
                else
                {
                    changeColour(almond, i);
                }
            }
        }

        private void selectColour(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //{ Red = 0, Green, Blue, Yellow, Pink, Purple }
            Ellipse circle = sender as Ellipse;
            if (selected.Count < 4 && !finished)
            {
                if (circle.Fill.ToString() == solidColors[MasterMindLibrary.Colors.Red].ToString())
                {
                    selected.Add(MasterMindLibrary.Colors.Red);
                }
                else if (circle.Fill.ToString() == solidColors[MasterMindLibrary.Colors.Green].ToString())
                {
                    selected.Add(MasterMindLibrary.Colors.Green);
                }
                else if (circle.Fill.ToString() == solidColors[MasterMindLibrary.Colors.Blue].ToString())
                {
                    selected.Add(MasterMindLibrary.Colors.Blue);
                }
                else if (circle.Fill.ToString() == solidColors[MasterMindLibrary.Colors.Yellow].ToString())
                {
                    selected.Add(MasterMindLibrary.Colors.Yellow);
                }
                else if (circle.Fill.ToString() == solidColors[MasterMindLibrary.Colors.Pink].ToString())
                {
                    selected.Add(MasterMindLibrary.Colors.Pink);
                }
                else if (circle.Fill.ToString() == solidColors[MasterMindLibrary.Colors.Purple].ToString())
                {
                    selected.Add(MasterMindLibrary.Colors.Purple);
                }
                updateColours();
            }
        }

        private void showLastGuess(List<MasterMindLibrary.Colors> guess, List<bool?> hints)
        {
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());

            for (int i = 0; i < guess.Count; i++)
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Height = 15;
                ellipse.Width = 15;
                ellipse.Fill = solidColors[guess[i]];
                ellipse.Stroke = new SolidColorBrush(System.Windows.Media.Colors.Black);
                ellipse.Margin = new Thickness(2);
                Grid.SetColumn(ellipse, i);
                Grid.SetRowSpan(ellipse, 4);
                grid.Children.Add(ellipse);
            }

            for (int i = 0; i < hints.Count; i++)
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Height = 8;
                ellipse.Width = 8;
                ellipse.Stroke = new SolidColorBrush(System.Windows.Media.Colors.Black);
                ellipse.StrokeThickness = 1;
                ellipse.Margin = new Thickness(2);

                if (hints[i] == true)
                {
                    ellipse.Fill = new SolidColorBrush(System.Windows.Media.Colors.Red);
                }
                else if (hints[i] == false)
                {
                    ellipse.Fill = new SolidColorBrush(Color.FromArgb(255, 244, 122, 143));
                }
                else if (hints[i] == null)
                {
                    continue;
                }

                switch (i)
                {
                    case 0:
                        Grid.SetColumn(ellipse, 4);
                        Grid.SetRow(ellipse, 0);
                        break;
                    case 1:
                        Grid.SetColumn(ellipse, 5);
                        Grid.SetRow(ellipse, 0);
                        break;
                    case 2:
                        Grid.SetColumn(ellipse, 4);
                        Grid.SetRow(ellipse, 1);
                        break;
                    case 3:
                        Grid.SetColumn(ellipse, 5);
                        Grid.SetRow(ellipse, 1);
                        break;
                }
                grid.Children.Add(ellipse);
            }


            this.guessContainer.Children.Clear();
            this.guessContainer.Children.Add(grid);
        }

        private void testSequence(object sender, RoutedEventArgs e)
        {
            try
            {
                if (guesses.Count + 1 <= 8 && selected.Count == 4)
                {
                    Tuple<bool?, List<bool?>> results = codeMaker.IsCorrect(selected, name);
                    if (results.Item1 == false)
                    {
                        guesses.Add(selected);
                        Results.Text = String.Format("That sequence is incorrect!\nYou've made {0}/8 guesses.", guesses.Count);
                        showLastGuess(selected, results.Item2);
                        this.submit.IsEnabled = false;
                        if (guesses.Count == 8)
                        {
                            winnerCard.Visibility = Visibility.Visible;
                            SolidColorBrush red = new SolidColorBrush(System.Windows.Media.Colors.Red);
                            winnerText.Foreground = red;
                            winnerText.Text = "LOSER!";
                            this.finished = true;
                        }
                        else
                        {
                            selected = new List<MasterMindLibrary.Colors>();
                            updateColours();
                        }
                    }
                    else if (results.Item1 == true)
                    {
                        guesses.Add(selected);
                        showLastGuess(selected, results.Item2);
                        Results.Text = String.Format("That sequence is correct!\nYou guessed {0} time(s) before finding the sequence.", guesses.Count);
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("There was an issue talking to the server, you have been disconnected. Please try connecting again.");
                App.Current.joinWindow = new JoinWindow();
                App.Current.joinWindow.Show();
                App.Current.mainWindow.Close();
            }
        }

        private void viewGuesses(object sender, RoutedEventArgs e)
        {
            GuessesWindow window = new GuessesWindow(this.guesses, this.solidColors);
            window.ShowDialog();
        }

        public void SomeoneJoined(Dictionary<string, int> players)
        {
            if (System.Threading.Thread.CurrentThread == this.Dispatcher.Thread)
            {
                this.playerListStack.Children.Clear();
                foreach (KeyValuePair<string, int> entry in players)
                {
                    string name = entry.Key;
                    int guesses = entry.Value;

                    TextBlock user = new TextBlock();
                    user.FontSize = 14;
                    user.Text = name + " [" + guesses + "]";
                    this.playerListStack.Children.Add(user);
                }
            }
            else
            {
                // Only the main (dispatcher) thread can change the GUI
                this.Dispatcher.BeginInvoke(new ClientUserDelegate(SomeoneJoined), players);
            }
        }

        private void deselect(object sender, RoutedEventArgs e)
        {
            if (selected.Count > 0 && !this.finished)
            {
                this.selected.RemoveAt(selected.Count - 1);
                updateColours();
            }
        }
    }
}
