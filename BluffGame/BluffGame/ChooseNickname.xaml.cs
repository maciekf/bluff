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
    /// Interaction logic for ChooseNickame.xaml
    /// </summary>
    public partial class ChooseNickname : Page
    {
        public ClientState Context { set; get; }
        public ChooseNickname(ClientState Context)
        {
            this.Context = Context;
            InitializeComponent();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            Servers nextPage = new Servers(Context);
            if (Context.PlayerName.Equals(""))
                Context.PlayerName = "Anonim";
            this.NavigationService.Navigate(nextPage);
        }

        private void inputNickBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                okButton_Click(sender, (RoutedEventArgs) e);
            }
        }
    }
}
