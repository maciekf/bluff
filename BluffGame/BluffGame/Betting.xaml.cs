﻿using System;
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
    /// Interaction logic for Bet.xaml
    /// </summary>
    public partial class Betting : Page
    {
        public ClientState Context { set; get; }
        public Betting(ClientState Context)
        {
            this.Context = Context;
            InitializeComponent();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
