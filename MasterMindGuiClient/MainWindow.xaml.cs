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

                DuplexChannelFactory<ICodeMaker> channel = new DuplexChannelFactory<ICodeMaker>(this, new NetTcpBinding(), new EndpointAddress("net.tcp://" + ip + ":13200/MasterMindLibrary/MasterService"));
                codeMaker = channel.CreateChannel();
                this.submit.IsEnabled = false;

               

                try
                {
                    HasSomeoneWon();
                    // Subscribe to the callbacks
                    callbacksEnabled = codeMaker.ToggleCallbacks();
                    connected = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(String.Format("There was an issue connecting to '{0}'. Please check the entered address as well as your network status and try again", ip));
                    App.Current.joinWindow = new JoinWindow();
                    App.Current.joinWindow.Show();
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

                    this.Show();
                }
                else
                {
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
            string name = codeMaker.HasSomeoneWon();
            if (name != "")
            {
                finished = true;
                codeMaker.IsCorrect(selected, name);
            }
        }

        private delegate void ClientUpdateDelegate(CallbackInfo info);

        public void SomeoneWon(CallbackInfo info)
        {
            if (System.Threading.Thread.CurrentThread == this.Dispatcher.Thread)
            {
                if (info.someoneWon == false)
                {
                    // Update the GUI
                    MessageBox.Show("A user won: " + info.name);
                    GuessesWindow window = new GuessesWindow(this.guesses, this.solidColors);
                    window.ShowDialog();
                }
                else
                {
                    MessageBox.Show("A user has already won: " + info.name);
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

        private void testSequence(object sender, RoutedEventArgs e)
        {
            if (guesses.Count + 1 <= 8 && selected.Count == 4)
            {
                Tuple<bool?, string> results = codeMaker.IsCorrect(selected, name);
                if (results.Item1 == false)
                {
                    guesses.Add(selected);
                    Results.Text = String.Format("That sequence is incorrect!\nYou've made {0} guesses.", guesses.Count);
                    selected = new List<MasterMindLibrary.Colors>();
                    this.submit.IsEnabled = false;
                    MessageBox.Show(results.Item2);
                    updateColours();
                    if (guesses.Count == 8)
                    {
                        GuessesWindow window = new GuessesWindow(this.guesses, this.solidColors);
                        window.ShowDialog();
                    }
                }
                else if (results.Item1 == true)
                {
                    guesses.Add(selected);
                    Results.Text = String.Format("That sequence is correct!\nYou guessed {0} before finding the sequence.", guesses.Count);
                }
            }
        }
    }
}
