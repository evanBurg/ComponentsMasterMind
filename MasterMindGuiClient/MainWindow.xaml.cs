using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ServiceModel;
using MasterMindLibrary;

namespace MasterMindGUI
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public partial class MainWindow : Window, ICallback
    {
        // Member variables
        private ICodeMaker codeMaker = null;
        private bool callbacksEnabled = false;

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
    }
}
