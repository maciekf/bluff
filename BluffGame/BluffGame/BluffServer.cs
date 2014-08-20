using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace BluffGame
{

    [Serializable]
    public class PlayerMsg
    {
        public string msgType;
        public string msgContent;
        
        public PlayerMsg(string type, string content)
        {
            this.msgType = type;
            this.msgContent = content;
        }
        public static PlayerMsg BytesToMsg(byte[] src)
        {
            MemoryStream stream = new MemoryStream(src);
            BinaryFormatter formatter = new BinaryFormatter();
            return (PlayerMsg)formatter.Deserialize(stream);
        }
        public byte[] ToBytes()
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);
            return stream.ToArray();
        }
    }

    public class BluffServer
    {
        private Random _random;
        private const int maxSits = 6;
        private int tcpport = 29492;
        private int udpport = 29493;
        private string gameName;
        public string GameName { set { this.gameName = value; } get { return this.gameName; } }
        private int sits;
        
        private GameState gameState;
        private Socket tcpsock, udpsock;
        private Thread serverThread, sendingThread;
        private Socket[] playerSocket = new Socket[maxSits];
        private Thread[] playerThread = new Thread[maxSits];
        
        private String[] playerBet = new String[maxSits];

        private BluffServer() {}

        public BluffServer(string name, int sits)
        {
            this.gameName = name;
            this.sits = sits;
            gameState = new GameState();
            gameState.GameName = this.gameName;
        }

        public void BroadcastNewGame()
        {
            Console.WriteLine("Rozglaszam gre");
            if(!gameState.Active)
                udpsock.SendTo(new PlayerMsg("newgame",gameState.GameName).ToBytes(), (EndPoint)new IPEndPoint(IPAddress.Broadcast, 29593));
            else
                udpsock.SendTo(new PlayerMsg("oldgame", gameState.GameName).ToBytes(), (EndPoint)new IPEndPoint(IPAddress.Broadcast, 29593));
        }

        private void SendingThread(object o)
        {

            GameState[] gs = new GameState[sits];
            for(int i=0; i < sits; i++) gs[i] = gameState.Copy();
            System.Threading.Tasks.Parallel.For(0, sits, delegate(int i)
            { 
                if (gs[i].Player[i].Seated == true)
                {
                    gs[i].AddressedTo = i;
                    try
                    {
                        (new BinaryFormatter()).Serialize(new NetworkStream(playerSocket[i]), gs[i]);
                    }
                    catch (Exception)
                    {
                    }
                    Console.WriteLine("Wyslalem stan gry do " + i.ToString());
                }
            });
        }

        private void ChatSendingThread(object o)
        {
            PlayerMsg msg = (PlayerMsg)o;
            System.Threading.Tasks.Parallel.For(0, sits, delegate(int i)
            {
                try
                {
                    if (gameState.Player[i].Seated)
                    {
                        (new BinaryFormatter()).Serialize(new NetworkStream(playerSocket[i]), msg);
                        Console.WriteLine();
                    }
                }
                catch (Exception)
                {
                }
            });
        }

        private void SendGameState()
        {
            sendingThread = new Thread(new ParameterizedThreadStart(SendingThread));
            sendingThread.IsBackground = true;
            sendingThread.Start(null);
        }

        private void PlayerThread(object o) 
        {
            int nr = (int)o;
            NetworkStream stream = new NetworkStream(playerSocket[nr]);
            BinaryFormatter formatter = new BinaryFormatter();
            Console.WriteLine("Nowy proces gracza " + nr.ToString());

            while (true)
            {
                PlayerMsg msg = new PlayerMsg("", "");
                try
                {
                    if(playerSocket[nr].Poll(10000, SelectMode.SelectRead))
                        msg = (PlayerMsg)formatter.Deserialize(new NetworkStream(playerSocket[nr]));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Deserialize: " + e.Message);
                    if (!gameState.Active)
                    {
                        gameState.Player[nr] = new PlayerState(nr);
                        SendGameState();
                    }
                    playerBet[nr] = "Left";
                    playerSocket[nr].Close();
                    return;
                }
                

                switch (msg.msgType)
                {
                    case "playername":
                        Console.WriteLine("Gracz zmienia imie");
                        if (msg.msgContent != "")
                        {
                            if (gameState.Player[nr].Name == "")
                            {
                                gameState.Player[nr].Seated = true;
                                gameState.Player[nr].Position = nr;
                               // SendGameState();
                            }
                            gameState.Player[nr].Name = msg.msgContent;
                        }
                        break;
                    case "bet":
                        if(playerBet[nr] == null)
                                playerBet[nr] = msg.msgContent;
                        Console.WriteLine("Bet usera: " + nr.ToString() + " " + msg.msgContent);
                        break;
                    case "chat":
                        Console.WriteLine("odebralem wiadomosc na czat");
                        Thread chatSend = new Thread(ChatSendingThread);
                        chatSend.Start(msg);
                        break;
                }

            }
        }

        public void StartGame()
        /* starts game */
        {
            if (gameState.Active) return;
            int count = 0;
            for (int it = 0; it < sits; it++)
            {
                if (gameState.Player[it].Seated) count++;
            }
            if (count >= 2)
                gameState.Active = true;
        }

        private T[] Shuffle<T>(T[] array)
        {
            var random = _random;
            for (int i = array.Length; i > 1; i--)
            {
                int j = random.Next(i);
                T tmp = array[j];
                array[j] = array[i - 1];
                array[i - 1] = tmp;
            }
            return array;
        }

        private bool CheckHand(int[] cards, int howmany, int bet)
        {
            int[] nums = Enumerable.Repeat(0, 6).ToArray();
            bool[] card = Enumerable.Repeat(false, 24).ToArray();
            int[] color = Enumerable.Repeat(0, 4).ToArray();
            bool[] hand = Enumerable.Repeat(true, 90).ToArray();
            int it = 0;
            for (int i = 0; i < howmany; i++)
            {
                nums[cards[i] % 6]++;
                color[cards[i] / 6]++;
                card[cards[i]] = true;
            }

            for (int i = 0; i < 6; i++) hand[it++] = nums[i] > 0;
            for (int i = 0; i < 6; i++) hand[it++] = nums[i] >= 2;
            for (int i = 0; i < 6; i++)
                for (int j = 0; j < i; j++) hand[it++] = nums[i] >= 2 && nums[j] >= 2;
            hand[it++] = nums[0] > 0 && nums[1] > 0 && nums[2] > 0 && nums[3] > 0 && nums[4] > 0;
            hand[it++] = nums[1] > 0 && nums[2] > 0 && nums[3] > 0 && nums[4] > 0 && nums[5] > 0;
            for (int i = 0; i < 6; i++) hand[it++] = nums[i] >= 3;

            for (int i = 0; i < 6; i++)
                for (int j = 0; j < 6; j++)
                    if (i != j) hand[it++] = nums[i] >= 3 && nums[j] >= 2;
 
            for (int i = 0; i < 4; i++) hand[it++] = color[i] >= 5;
            for (int i = 0; i < 6; i++) hand[it++] = nums[i] >= 4;
            for (int i = 0; i < 4; i++) hand[it++] = color[i] == 6 || (color[i] == 5 && !card[6 * i + 5]);
            for (int i = 0; i < 4; i++) hand[it++] = color[i] == 6 || (color[i] == 5 && !card[6 * i    ]);
          
            return hand[bet];
        }

        private void PlayGame()
        {
            _random = new Random();
            Console.WriteLine("Rozpoczynam gre:");
            int playersLeft = 0;
            int[] allCards = new int[24];
            int[] cards = new int[sits];
            int cardsLeft = 0;
            int nextToMove = 0;
            int lastMoved = -1;
            int lastBet;
            int dealer = _random.Next(sits);
            int looser = -1;
            int ultiBet = BetTranslator.getBet("Sprawdzam");
            for (int i = 0; i < sits; i++) playerBet[i] = null;

            for (int i = 0; i < sits; i++) cards[i] = 0;
            for (int i = 0; i < 24; i++) allCards[i] = i;

            for (int i = 0; i < sits; i++)
                if (gameState.Player[i].Seated)
                {
                    playersLeft++;
                    cards[i] = 1;
                    gameState.Player[i].Playing = true;
                }

            // jeden przebieg - jedno rozdanie
            while (playersLeft > 1)
            {
                playersLeft = 0;
                cardsLeft = 0;
                for (int i = 0; i < sits; i++)
                {
                    if (!gameState.Player[i].Playing) cards[i] = 0;
                    else
                    {
                        playersLeft++;
                        cardsLeft += cards[i];
                    }
                }
                if (playersLeft < 2) break;

                nextToMove = dealer;
                while( cards[nextToMove] == 0)
                    nextToMove = (nextToMove + 1) % sits;

                int it = 0;
                allCards = Shuffle <int>(allCards); //tasowanie kart

                for (int i = 0; i < sits; i++)
                    if (cards[i] != 0)
                    {
                        gameState.Player[i].Playing = true;
                        gameState.Player[i].CardScore = cards[i];
                        for (int j = 0; j < cards[i]; j++)
                            gameState.Player[i].Hand[j] = CardTranslator.getCard(allCards[it++]);
                    }
                    else
                    {
                        gameState.Player[i].CardScore = 0;
                        gameState.Player[i].Playing = false;
                    }
                gameState.BetHistory = new List <Bet>();
                gameState.Round += 1;
                gameState.EndOfRound = false;

                
                looser = -1;
                lastBet = -1;
                lastMoved = -1;

                //jeden bet w jednym przebiegu
                while (true)
                {
                    if (playerBet[nextToMove] == "Left") break;
                    playerBet[nextToMove] = null;
                    gameState.NextToMove = nextToMove;
                    
                    SendGameState();
                    
                    // ja bym wprowadził jakiś timeout
                    while (playerBet[nextToMove] == null)
                    {
                        Thread.Sleep(50);
                    }

                    //host kickuje gracza
                    if (playerBet[nextToMove] == "Left") break;
                    int currentBet = BetTranslator.getBet(playerBet[nextToMove]);
                    if (currentBet > lastBet)
                    {
                        if (currentBet == ultiBet)
                        {
                            if (lastBet == -1)
                                continue;
                            if (CheckHand(allCards, cardsLeft, lastBet))
                                looser = nextToMove;
                            else
                                looser = lastMoved;
                            break;
                        }
                        gameState.BetHistory.Add(new Bet(playerBet[nextToMove]));
                        lastBet = currentBet;
                        lastMoved = nextToMove;
                        do
                        {
                            nextToMove = (nextToMove + 1) % sits;
                        } while (cards[nextToMove] == 0);
                    }
                }
                gameState.EndOfRound = true;
                SendGameState();
                Thread.Sleep(5000);
                // usuwanie typka co przegral, dodawanie kart
                if (looser != -1)
                {
                    dealer = looser;
                    cards[looser]++;
                    if (cards[looser] > 5 || (cards[looser] > 4 && playersLeft == 6))
                    {
                        cards[looser] = 0;
                        gameState.Player[looser].Playing = false;
                        gameState.Player[looser].CardScore = 0;
                    } 

                }
                else 
                {
                    cards[nextToMove] = 0;
                    gameState.Player[nextToMove] = new PlayerState(nextToMove);
                }
                gameState.EndOfRound = true;
            }
            for(int i = 0; i < sits; i++) {
                if (cards[i] != 0)
                {
                    gameState.Player[i].Winner = true;
                    Console.WriteLine("Wygral: " + i.ToString());
                }
            }
            SendGameState();
        }

        private void ServerThread(object o)
        /* proces odpowiedzialny za rozgrywkę */
        {
            BroadcastNewGame();
            Console.WriteLine("Otwieram nową grę: " + gameName);
            PlayerMsg msg;
            byte[] datagram = new byte[1518];
            int licz = 0;
            while (!gameState.Active)
            {
                if (udpsock.Poll(0, SelectMode.SelectRead))
                {

                    EndPoint ep = (EndPoint)new IPEndPoint(IPAddress.Any, 0);
                    try
                    {
                        udpsock.ReceiveFrom(datagram, ref ep);
                        msg = PlayerMsg.BytesToMsg(datagram);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        continue;
                    }
                    if (msg.msgType == "broadcast")
                    {
                        BroadcastNewGame();
                        Console.WriteLine("Zostałem poproszony o broadcast gry");
                    }
                }
                licz = (licz + 1) % 20;
                if (tcpsock.Poll(50000, SelectMode.SelectRead))
                {
                    
                    Socket tmpsock = tcpsock.Accept();
                    int it = 0;
                    while (it < sits && gameState.Player[it].Name != null) ++it;
                    if (it < sits)
                    {
                        Console.WriteLine("Gracz zajmuje miejsce");
                        gameState.Player[it].Name = "";
                        gameState.Player[it].Seated = true;
                        playerSocket[it] = tmpsock;
                        playerThread[it] = new Thread(new ParameterizedThreadStart(PlayerThread));
                        playerThread[it].IsBackground = true;
                        playerThread[it].Start(it);
                        SendGameState();
                    }
                    else
                    {
                        Console.WriteLine("Brak wolnych miejsc");
                        tmpsock.Close();
                    }

                }
                if (licz == 0) SendGameState();
            }
            try
            {
                tcpsock.Close();
            }
            catch (Exception)
            { }
            BroadcastNewGame();
            //opcjanalnie można dodać
            //SendGameState();
            //Thread.Sleep(500);
            PlayGame();
            try
            {
                udpsock.Close();
            }
            catch( Exception)
            { }
                //tutaj zakładam, że końcowy stan gry da rade wysłać w sekunde
            Thread.Sleep(1000);
            for (int i = 0; i < sits; i++)
            {
                try
                {
                    playerSocket[i].Close();
                }
                catch (Exception)
                {
                }
            }
            //koniec gry
        }

        public void Run()
        /* tworzy gniazda serwera i jego faktyczny proces w wątku process*/
        {
            EndPoint tcpipep = (EndPoint)new IPEndPoint(IPAddress.Any, tcpport);
            EndPoint udpipep = (EndPoint)new IPEndPoint(IPAddress.Any, udpport);
            

            tcpsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            while (true)
            {
                try
                {
                    tcpsock.Bind(tcpipep);
                    break;
                }
                catch (Exception e)
                {

                    throw e;
                }
            }
            tcpsock.Listen(10);

            udpsock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpsock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            try
            {
                udpsock.Bind(udpipep);
            }
            catch (Exception e)
            {

                tcpsock.Close();
                throw e;
            }
            
            serverThread = new Thread(new ParameterizedThreadStart(ServerThread));
            serverThread.IsBackground = true;
            serverThread.Start(null);
        }

    }
}