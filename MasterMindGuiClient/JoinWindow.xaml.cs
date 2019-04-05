using MasterMindGUI;
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
    /// <summary>
    /// Interaction logic for JoinWindow.xaml
    /// </summary>
    public partial class JoinWindow : Window
    {
        public JoinWindow()
        {
            InitializeComponent();
        }

        private void join(object sender, RoutedEventArgs e)
        {
            App.Current.mainWindow = new MainWindow(this.ipTxt.Text, this.nameTxt.Text);
            App.Current.joinWindow.Close();
        }
    }
}
