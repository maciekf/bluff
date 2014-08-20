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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BluffGame
{
    /// <summary>
    /// Interaction logic for DirectJoin.xaml
    /// </summary>
    public partial class DirectJoin : Page
    {
        public ClientState Context { set; get; }
        public DirectJoin(ClientState Context)
        {
            this.Context = Context;
            InitializeComponent();
            this.Context.HostIP = "127.0.0.1";
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (Context.HostIP != "")
                ((NavigationWindow)this.Parent).Close();
            else
                titleLabel.Content = "Pierwsze podaj IP";
        }
        private void ipBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                okButton_Click(sender, (RoutedEventArgs)e);
            }
        }
    }
}
