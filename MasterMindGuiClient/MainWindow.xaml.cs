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

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                // Connect to the WCF service endpoint called "ShoeService" 
                DuplexChannelFactory<ICodeMaker> channel = new DuplexChannelFactory<ICodeMaker>(this, "Mastermind");
                codeMaker = channel.CreateChannel();

                // Subscribe to the callbacks
                callbacksEnabled = codeMaker.ToggleCallbacks();

                selected = new List<MasterMindLibrary.Colors>();
                solidColors = new Dictionary<MasterMindLibrary.Colors, SolidColorBrush>();
                guesses = new List<List<MasterMindLibrary.Colors>>();

                solidColors.Add(MasterMindLibrary.Colors.Red, new SolidColorBrush(System.Windows.Media.Colors.Red));
                solidColors.Add(MasterMindLibrary.Colors.Green, new SolidColorBrush(System.Windows.Media.Colors.Green));
                solidColors.Add(MasterMindLibrary.Colors.Blue, new SolidColorBrush(System.Windows.Media.Colors.Blue));
                solidColors.Add(MasterMindLibrary.Colors.Yellow, new SolidColorBrush(System.Windows.Media.Colors.Yellow));
                solidColors.Add(MasterMindLibrary.Colors.Pink, new SolidColorBrush(System.Windows.Media.Colors.Pink));
                solidColors.Add(MasterMindLibrary.Colors.Purple, new SolidColorBrush(System.Windows.Media.Colors.Purple));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private delegate void ClientUpdateDelegate(CallbackInfo info);

        public void SomeoneWon(CallbackInfo info)
        {

            MessageBox.Show("A user won: " + info.name);
            GuessesWindow window = new GuessesWindow(this.guesses, this.solidColors);
            window.ShowDialog();
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
            if (selected.Count < 4)
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
            if (guesses.Count + 1 <= 8)
            {
                //"Kev" string should take the user's name instead of it being this hardcoded string.
                bool correct = codeMaker.IsCorrect(selected, "Kev");
                if (!correct)
                {
                    guesses.Add(selected);
                    Results.Text = String.Format("That sequence is incorrect!\nYou've made {0} guesses.", guesses.Count);
                    selected = new List<MasterMindLibrary.Colors>();
                    updateColours();
                    if (guesses.Count == 8)
                    {
                        GuessesWindow window = new GuessesWindow(this.guesses, this.solidColors);
                        window.ShowDialog();
                    }
                }
                else
                {
                    guesses.Add(selected);
                    GuessesWindow window = new GuessesWindow(this.guesses, this.solidColors);
                    window.ShowDialog();
                    Results.Text = String.Format("That sequence is correct!\nYou guessed {0} before finding the sequence.", guesses.Count);
                }
            }
        }
    }
}
