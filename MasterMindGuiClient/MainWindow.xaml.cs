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

namespace MasterMindGUI
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public partial class MainWindow : Window, ICallback
    {
        // Member variables
        private ICodeMaker codeMaker = null;
        private List<MasterMindLibrary.Colors> selected;
        private List<SolidColorBrush> solidColors;
        private bool callbacksEnabled = false;
        private int guesses = 0;

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
                solidColors = new List<SolidColorBrush>();
                for (int i = 0; i <= (int)MasterMindLibrary.Colors.Purple; i++)
                {
                    solidColors.Add(new SolidColorBrush());
                }
                solidColors[0].Color = System.Windows.Media.Colors.Red;
                solidColors[1].Color = System.Windows.Media.Colors.Green;
                solidColors[2].Color = System.Windows.Media.Colors.Blue;
                solidColors[3].Color = System.Windows.Media.Colors.Yellow;
                solidColors[4].Color = System.Windows.Media.Colors.Pink;
                solidColors[5].Color = System.Windows.Media.Colors.Purple;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private delegate void ClientUpdateDelegate(CallbackInfo info);

        public void UpdateGui(CallbackInfo info)
        {
            if (System.Threading.Thread.CurrentThread == this.Dispatcher.Thread)
            {
                // Update the GUI

            }
            else
            {
                // Only the main (dispatcher) thread can change the GUI
                this.Dispatcher.BeginInvoke(new ClientUpdateDelegate(UpdateGui), info);
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
        }

        private void updateColours()
        {
            SolidColorBrush almond = new SolidColorBrush();
            almond.Color = System.Windows.Media.Colors.BlanchedAlmond;
            for (int i = 0; i < 4; i++)
            {
                if (selected.Count > i)
                {
                    changeColour(solidColors[(int)selected[i]], i);
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
                if (circle.Fill.ToString() == solidColors[(int)MasterMindLibrary.Colors.Red].ToString())
                {
                    selected.Add(MasterMindLibrary.Colors.Red);
                }
                else if (circle.Fill.ToString() == solidColors[(int)MasterMindLibrary.Colors.Green].ToString())
                {
                    selected.Add(MasterMindLibrary.Colors.Green);
                }
                else if (circle.Fill.ToString() == solidColors[(int)MasterMindLibrary.Colors.Blue].ToString())
                {
                    selected.Add(MasterMindLibrary.Colors.Blue);
                }
                else if (circle.Fill.ToString() == solidColors[(int)MasterMindLibrary.Colors.Yellow].ToString())
                {
                    selected.Add(MasterMindLibrary.Colors.Yellow);
                }
                else if (circle.Fill.ToString() == solidColors[(int)MasterMindLibrary.Colors.Pink].ToString())
                {
                    selected.Add(MasterMindLibrary.Colors.Pink);
                }
                else if (circle.Fill.ToString() == solidColors[(int)MasterMindLibrary.Colors.Purple].ToString())
                {
                    selected.Add(MasterMindLibrary.Colors.Purple);
                }
                updateColours();
            }
        }

        private void testSequence(object sender, RoutedEventArgs e)
        {
            bool correct = codeMaker.IsCorrect(this.selected);
            if (!correct)
            {
                guesses += 1;
                Results.Text = String.Format("That sequence is incorrect!\nYou've made {0} guesses.", guesses);
                selected = new List<MasterMindLibrary.Colors>();
                updateColours();
            }
            else
            {
                Results.Text = String.Format("That sequence is correct!\nYou guessed {0} before finding the sequence.", guesses);
            }
        }
    }
}
