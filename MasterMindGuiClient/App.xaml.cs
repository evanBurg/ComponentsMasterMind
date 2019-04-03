using MasterMindGuiClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MasterMindGUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public JoinWindow joinWindow;
        public MainWindow mainWindow;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            joinWindow = new JoinWindow();
        }

        public static new App Current
        {
            get { return Application.Current as App; }
        }
    }
}
