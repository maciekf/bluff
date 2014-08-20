using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
    public static class ErrorHadlers
    {
        public static void ShowError(string error)
        {
            NavigationWindow errWindow = new NavigationWindow();
            errWindow.ShowsNavigationUI = false;
            errWindow.Height = 500;
            errWindow.Width = 400;
            errWindow.MinHeight = 500;
            errWindow.MinWidth = 400;
            ErrorPage showPage = new ErrorPage(error);
            errWindow.NavigationService.Navigate(showPage);
            errWindow.ShowDialog();
        } 
    }
     
    public class NewGame 
    {
        private IPAddress hostAddress;
        private String gameName;
        public NewGame(IPEndPoint ipep, String name)
        {
            gameName = "Nowa gra: " + name;
            this.hostAddress = ipep.Address;
        }

        public IPAddress GetAddress()
        {
            return hostAddress;
        }

        public override string ToString()
        {
 	        return gameName;
        }
    }


    /// <summary>
    /// Interaction logic for Servers.xaml
    /// </summary>
    public partial class Servers : Page
    {
        private bool listening = false;
        private int udpport = 29593;
        private List<NewGame> gameList = new List<NewGame>();
        private Thread listeningThread;

        Socket udpsock;

        public ClientState Context { set; get; }
        public Servers(ClientState Context)
        {
            this.Context = Context;
            InitializeComponent();
            initGameListen();
            serversBox.MouseDoubleClick += new MouseButtonEventHandler(serversBox_DoubleClick);
        }

        private void setGameList()
        {
            lock (serversBox)
            {
                this.serversBox.ItemsSource = gameList;
            }
        }

        private void listenForGames()
        {
            IPEndPoint localudpep = new IPEndPoint(IPAddress.Any, udpport);
            udpsock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpsock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            udpsock.Bind(localudpep);
            IPEndPoint broadcastep = new IPEndPoint(IPAddress.Broadcast, 29493);
            byte[] msg = new PlayerMsg("broadcast","game").ToBytes();
            udpsock.SendTo(msg, broadcastep);
            PlayerMsg state;
            byte[] buffer = new byte[1518];
            EndPoint hostep = new IPEndPoint(IPAddress.Any, 0);
            try
            {
                while (listening)
                {
                    udpsock.ReceiveFrom(buffer, ref hostep);
                    state = PlayerMsg.BytesToMsg(buffer);
                    Console.WriteLine(state.msgType + " " + state.msgContent);
                    if (state != null)
                    {
                        IPAddress addr = ((IPEndPoint)hostep).Address;
                        NewGame ng = null;
                        foreach (NewGame it in gameList)
                            if (it.GetAddress().Equals(addr))
                                ng = it;

                        if (ng == null && state.msgType == "newgame")
                            gameList.Add(new NewGame((IPEndPoint)hostep, state.msgContent));
                        if (ng != null && state.msgType == "oldgame")
                            gameList.Remove(ng);
                    }
                    foreach (NewGame it in gameList) Console.WriteLine(it);

                    lock (this)
                    {
                        //i w jaki sposób ma wiedzieć do kogo się połączyć?
                        //tutaj liste elementów wklejasz w ekran
                        Console.WriteLine("Probuje aktualizowac gry");
                        this.Dispatcher.Invoke(setGameList);
                        Console.WriteLine("Gry zaktualizowane");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Skonczylem sluchac" + e.Message);
                stopGameListen();
            }
        }

        private void initGameListen()
        {
            listening = true;
            listeningThread = new Thread(listenForGames);
            listeningThread.IsBackground = true;
            listeningThread.Start();          
        }

        private void stopGameListen()
        {
            listening = false;
            try
            {
                udpsock.Close();
            }
            catch(Exception)
            {}
        }

        private void broadcastGameSearch()
        {
            byte[] buf = new PlayerMsg("broadcast", "games").ToBytes();
            udpsock.SendTo(buf, new IPEndPoint(IPAddress.Broadcast, 29493));
        }

        private void joinButton_Click(object sender, RoutedEventArgs e)
        {
            lock (serversBox)
            {
                if (serversBox.SelectedItem != null)
                {
                    try
                    {
                        Context.Client = new BluffClient(Context.PlayerName, ((NewGame)serversBox.SelectedItem).GetAddress().ToString());
                        GamePage nextPage = new GamePage(Context);
                        stopGameListen();
                        this.NavigationService.Navigate(nextPage);
                    }
                    catch (Exception ex)
                    {
                        Debug.Print("Bad connection " + ex.ToString());
                        ErrorHadlers.ShowError("Nie można dołączyć do sewera");
                    }
                }
            }
        }

        private void serversBox_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (serversBox.SelectedItem != null)
            {
                joinButton_Click(sender, (RoutedEventArgs)e);
            }
        }

        private void hostButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Context.Host = true;
                Context.Server = new BluffServer(Context.PlayerName + " gospodarzem", 6);
                Context.Server.Run();
                Context.Client = new BluffClient(Context.PlayerName, "localhost");
                GamePage nextPage = new GamePage(Context);
                stopGameListen();
                this.NavigationService.Navigate(nextPage);
            }
            catch (Exception ex)
            {
                Context.Host = false;
                Debug.Print(ex.ToString());
                ErrorHadlers.ShowError("Nie można stworzyć sewera");
            }
        }

        private void directJoinButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationWindow joinWindow = new NavigationWindow();
            joinWindow.ShowsNavigationUI = false;
            joinWindow.DataContext = this;
            joinWindow.Height = 500;
            joinWindow.Width = 400;
            joinWindow.MinHeight = 500;
            joinWindow.MinWidth = 400;
            DirectJoin showPage = new DirectJoin(Context);
            joinWindow.NavigationService.Navigate(showPage);
            joinWindow.ShowDialog();
                  
            try
            {
                Context.Client = new BluffClient(Context.PlayerName, Context.HostIP);
                GamePage nextPage = new GamePage(Context);
                stopGameListen();
                this.NavigationService.Navigate(nextPage);
            } catch (Exception ex) 
            {
                Debug.Print("Bad connection " + ex.ToString());
                ErrorHadlers.ShowError("Nie można dołączyć do sewera");
            }
        }
    }
}
