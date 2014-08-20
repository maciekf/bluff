using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for GamePage.xaml
    /// </summary>
    public partial class GamePage : Page
    {
        public ClientState Context { set; get; }
        public delegate void NoParam();
        public PlayerMsg Message { set; get; }

        public List<String> chatMessages { set; get; } 

        public GamePage(ClientState Context)
        {
            this.Context = Context;
            chatMessages = new List<string>();
            InitializeComponent();
            update();
            backButton.Visibility = System.Windows.Visibility.Hidden;
            Context.Client.UIPage = this;
            Context.Client.StartListening();
            
        }

        public void chatUpdate()
        {
            lock (this)
            {
                if (Message != null)
                {
                    Debug.Print("Wrzucam " + Message.msgContent);
                    if (chatBox.Items.Count > 10)
                        chatBox.Items.RemoveAt(0);
                    chatBox.Items.Add(Message.msgContent);
                }
            }
        }

        private void chatInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Debug.Print("poszla wiadomosc na czat");
                Context.Client.SendChatMessage(Context.PlayerName + ": " + chatInput.Text);
                chatInput.Text = "";
            }
        }

        public void update()
        {
            lock (Context.CurrentGameState)
            {
                fillCanvas();
                betHistory.ItemsSource = Context.CurrentGameState.BetHistory;
                Context.RemainingBets = new List<Bet>();
                int lowerBound = 0;
                if (Context.CurrentGameState.BetHistory.Count > 0)
                {
                    lowerBound = BetTranslator.getBet(
                        Context.CurrentGameState.BetHistory[Context.CurrentGameState.BetHistory.Count - 1].ToString()) + 1;
                    Debug.Print(lowerBound.ToString());
                }
                for (; lowerBound < 83; ++lowerBound)
                {
                    Context.RemainingBets.Add(new Bet(lowerBound));
                }
                betBox.ItemsSource = Context.RemainingBets;
                if ((Context.CurrentGameState.AddressedTo == Context.CurrentGameState.NextToMove) && (Context.CurrentGameState.Active)
                    && (!Context.CurrentGameState.EndOfRound))
                {
                    moveLabel.Visibility = System.Windows.Visibility.Visible;
                    raiseButton.Visibility = System.Windows.Visibility.Visible;
                    callButton.Visibility = System.Windows.Visibility.Visible;
                    betBox.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    moveLabel.Visibility = System.Windows.Visibility.Hidden;
                    raiseButton.Visibility = System.Windows.Visibility.Hidden;
                    callButton.Visibility = System.Windows.Visibility.Hidden;
                    betBox.Visibility = System.Windows.Visibility.Hidden;
                }
                if (Context.CurrentGameState.BetHistory.Count == 0)
                    callButton.Visibility = System.Windows.Visibility.Hidden;
                if (Context.RemainingBets.Count == 0)
                    raiseButton.Visibility = System.Windows.Visibility.Hidden;
                if (Context.Host && !Context.CurrentGameState.Active)
                {
                    startButton.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    startButton.Visibility = System.Windows.Visibility.Hidden;
                }
                if (Context.CurrentGameState.Active && (!Context.CurrentGameState.Player[Context.CurrentGameState.AddressedTo].Playing ||
                    Context.CurrentGameState.Player[Context.CurrentGameState.AddressedTo].Winner))
                    backButton.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void hideToWait()
        {/*
            moveLabel.Visibility = System.Windows.Visibility.Hidden;
            raiseButton.Visibility = System.Windows.Visibility.Hidden;
            callButton.Visibility = System.Windows.Visibility.Hidden;
            betBox.Visibility = System.Windows.Visibility.Hidden;
            startButton.Visibility = System.Windows.Visibility.Hidden;
        */}

        private void fillCanvas()
        {
            GameCanvas.Children.Clear();

            Label alertLabel = new Label();
            alertLabel.Height = 40;
            alertLabel.Width = 360;
            if (!Context.CurrentGameState.Active)
                if (Context.Host)
                    alertLabel.Content = "Oczekiwanie na graczy";
                else
                    alertLabel.Content = "Oczekiwanie na start";
            else if (Context.CurrentGameState.EndOfRound)
            {
                int winner = -1;
                for (int i = 0; i < 6; i++)
                    if (Context.CurrentGameState.Player[i].Winner) winner = i;
                if (Context.CurrentGameState.AddressedTo == winner)
                    alertLabel.Content = "Gratulacje, wygrałeś!";
                else if (winner != -1)
                    alertLabel.Content = "Wygrał " + Context.CurrentGameState.Player[winner].Name + "!";
                else
                    alertLabel.Content = "Sprawdzenie";
            }
            else
            {
                alertLabel.Content = "Nowa runda";
                if (Context.CurrentGameState.BetHistory.Count > 0)
                    alertLabel.Content = Context.CurrentGameState.BetHistory[Context.CurrentGameState.BetHistory.Count - 1].ToString();
            }
            alertLabel.VerticalContentAlignment = VerticalAlignment.Center;
            alertLabel.HorizontalContentAlignment = HorizontalAlignment.Center;
            alertLabel.FontSize = 24;
            alertLabel.FontWeight = FontWeights.SemiBold;
            Canvas.SetTop(alertLabel, 200);
            Canvas.SetLeft(alertLabel, 150);
            GameCanvas.Children.Add(alertLabel);

            foreach (var player in Context.CurrentGameState.Player)
            {
                if (player.Seated)
                {
                    drawName(player.Position, player.Name);
                }
                if (player.Playing)
                {
                    bool visible = false;
                    if (player.Position == Context.CurrentGameState.AddressedTo || Context.CurrentGameState.EndOfRound)
                        visible = true;
                    int offset = 0;
                    if (player.CardScore <= 3)
                        offset = 1;
                    if (player.CardScore <= 1)
                        offset = 2;
                    for (int i = 0; i < player.CardScore; ++i)
                        drawCard(player.Hand[i], player.Position, i+offset, visible);
                }
            }
        }

        private void drawCard(Cards card, int position, int cardNumber, bool visible)
        {
            Label cardLabel = new Label();
            cardLabel.Height = Positions.Height;
            cardLabel.Width = Positions.Width;
            ImageBrush myBrush = new ImageBrush();
            Image image = new Image();
            if (visible)
                image.Source = new BitmapImage(
                    new Uri("pack://application:,,,/Resources/" + card.ToString("G") + ".png"));
            else
                image.Source = new BitmapImage(
                    new Uri("pack://application:,,,/Resources/Blue_Back.png"));
            myBrush.ImageSource = image.Source;

            cardLabel.Background = myBrush;
            Canvas.SetTop(cardLabel, Positions.CardPosition(position, cardNumber).Item2);
            Canvas.SetLeft(cardLabel, Positions.CardPosition(position, cardNumber).Item1);
            GameCanvas.Children.Add(cardLabel);
        }

        private void drawName(int position, String name)
        {
            Label nameLabel = new Label();
            nameLabel.Height = 40;
            nameLabel.Width = 100;
            nameLabel.Content = name;
            nameLabel.VerticalContentAlignment = VerticalAlignment.Center;
            nameLabel.HorizontalContentAlignment = HorizontalAlignment.Center;
            nameLabel.FontSize = 24;
            nameLabel.FontWeight = FontWeights.SemiBold;
            nameLabel.Foreground = new SolidColorBrush(Colors.Green);
            if ((Context.CurrentGameState.NextToMove == position) && (Context.CurrentGameState.Active))
                nameLabel.Foreground = new SolidColorBrush(Colors.Yellow);
            Tuple<int, int> pos = Positions.NameLabelPosition(position);
            Canvas.SetTop(nameLabel, pos.Item2);
            Canvas.SetLeft(nameLabel, pos.Item1);
            GameCanvas.Children.Add(nameLabel);
        }

        private void raiseButton_Click(object sender, RoutedEventArgs e)
        {
            Context.Client.CurrentBet = (Bet)betBox.SelectedItem;
            Context.Client.SendBet();
            hideToWait();
        }

        private void callButton_Click(object sender, RoutedEventArgs e)
        {
            Context.Client.CurrentBet = new Bet("Sprawdzam");
            Context.Client.SendBet();
            hideToWait();
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            Context.Server.StartGame();
            lock (Context.CurrentGameState)
            {
                Context.CurrentGameState.Active = true;
            }
            hideToWait();
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            Context.Client.StopListening();
            Servers backPage = new Servers(this.Context);
            ((NavigationWindow)this.Parent).NavigationService.Navigate(backPage);
        }
    }
}
