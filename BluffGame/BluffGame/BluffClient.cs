using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace BluffGame
{

    public class BluffClient
    {
        public GamePage UIPage { set; get; }
        public Bet CurrentBet { set; get; }
        public String PlayerName { set; get; }
        public GameState Context { set; get; }

        private int tcpport;
        private bool running;
        private string address;
        private TcpClient tcpClient;
        private BinaryFormatter bFormatter;
        private Thread readThread;
        private PlayerMsg chatMsg;

        private void init()
        {
            tcpport = 29492;
            bFormatter = new BinaryFormatter();

            tcpClient = new TcpClient(address, tcpport);
            Thread nameThread = new Thread(new ThreadStart(sendName));
            nameThread.Start();
            this.running = false;
        }

        public BluffClient(String playerName, String address)
        {
            this.PlayerName = playerName;
            this.address = address;
            init();
        }

        public void SendBet()
        {
            Thread betThread = new Thread(new ThreadStart(send));
            betThread.Start();
        }

        public void SendChatMessage(string content)
        {
            chatMsg = new PlayerMsg("chat", content);
            Thread chatThread = new Thread(new ThreadStart(sendChatMsg));
            chatThread.Start();
        }

        private void send()
        {
            if (CurrentBet != null)
            {
                PlayerMsg message = new PlayerMsg("bet", CurrentBet.ToString());
                bFormatter.Serialize(tcpClient.GetStream(), message);
            }
        }

        private void sendChatMsg()
        {
            try
            {
                if (chatMsg != null)
                {
                    bFormatter.Serialize(tcpClient.GetStream(), chatMsg);
                }
            }
            catch (Exception e)
            {
            }
        }
        private void sendName()
        {
            PlayerMsg message = new PlayerMsg("playername", PlayerName);
            // System.IO.IOException pojawia się przy hostowaniu 
            bFormatter.Serialize(tcpClient.GetStream(), message);
        }

        public void StartListening()
        {
            running = true;
            readThread = new Thread
                (new ThreadStart(ListenForMessage));
            readThread.IsBackground = true;
            readThread.Start();
        }

        private void ListenForMessage()
        {
            try
            {
                while (running && (tcpClient != null))
                {
                    Object state =
                        (new BinaryFormatter()).Deserialize(tcpClient.GetStream());

                    if (state == null)
                    {
                        StopListening();
                    }
                    else
                    {
                        if (state is GameState)
                        {
                            lock (UIPage.Context.CurrentGameState)
                            {
                                UIPage.Context.CurrentGameState = (GameState)state;
                            }
                            UIPage.Dispatcher.Invoke(new GamePage.NoParam(UIPage.update));
                        }
                        if (state is PlayerMsg)
                        {
                            Console.WriteLine("Cos bedzie na czacie");
                            lock (UIPage)
                            {
                                UIPage.Message = (PlayerMsg)state;
                                Debug.Print(UIPage.Message.msgContent);
                            }
                            UIPage.Dispatcher.Invoke(new GamePage.NoParam(UIPage.chatUpdate));
                        }
                    }
                }
            }
            catch (Exception)
            {
                StopListening();
            }
        }

        public void StopListening()
        {
            if (running)
            {
                if (tcpClient != null)
                    tcpClient.Close();
                running = false;
            }
        }

    }
}
