using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Classes representing state of game, used by client and server
/// </summary>

namespace BluffGame
{

    /// <summary>
    /// Enum with cards in deck
    /// </summary>
    public enum Cards
    {
        C9, D9, H9, S9,
        CT, DT, HT, ST,
        CJ, DJ, HJ, SJ,
        CQ, DQ, HQ, SQ,
        CK, DK, HK, SK,
        CA, DA, HA, SA
    }

    public static class CardTranslator
    {
        // serio to trochę bez sensu są te enumy, musiałem pisać do niech słownik :P
        private static Cards[] cards = { Cards.C9, Cards.CT, Cards.CJ, Cards.CQ, Cards.CK, Cards.CA, 
                                         Cards.D9, Cards.DT, Cards.DJ, Cards.DQ, Cards.DK, Cards.DA,
                                         Cards.H9, Cards.HT, Cards.HJ, Cards.HQ, Cards.HK, Cards.HA,
                                         Cards.S9, Cards.ST, Cards.SJ, Cards.SQ, Cards.SK, Cards.SA};

        public static int getCard(Cards card)
        {
            int it = 0;
            while (cards[it] != card) ++it;
            return it;
        }

        public static Cards getCard(int nr)
        {
            return cards[nr];
        }
    }

    /// <summary>
    /// Converter of string and int representation of bet
    /// </summary>
    public static class BetTranslator
    {
        private static Dictionary<int, string> betNames;
        private static Dictionary<string, int> betValues;

        static BetTranslator()
        {
            betNames = new Dictionary<int,string>();
            betValues = new Dictionary<string,int>();
            
            addBet("Jedna dziewiątka");
            addBet("Jedna dziesiątka");
            addBet("Jeden walet");
            addBet("Jedna dama");
            addBet("Jeden król");
            addBet("Jeden as");

            addBet("Dwie dziewiątki");
            addBet("Dwie dziesiątki");
            addBet("Dwa walety");
            addBet("Dwie damy");
            addBet("Dwa króle");
            addBet("Dwa asy");
            
            addBet("Dwie dziesiątki i dwie dziewiątki");
            addBet("Dwa walety i dwie dziewiątki");
            addBet("Dwa walety i dwie dziesiątki");
            addBet("Dwie damy i dwie dziewiątki");
            addBet("Dwie damy i dwie dziesiątki");
            addBet("Dwie damy i dwa walety");
            addBet("Dwa króle i dwie dziewiątki");
            addBet("Dwa króle i dwie dziesiątki");
            addBet("Dwa króle i dwa walety");
            addBet("Dwa króle i dwie damy");
            addBet("Dwa asy i dwie dziewiątki");
            addBet("Dwa asy i dwie dziesiątki");
            addBet("Dwa asy i dwa walety");
            addBet("Dwa asy i dwie damy");
            addBet("Dwa asy i dwa króle");

            addBet("Mały strit");
            addBet("Duży strit");

            addBet("Trzy dziewiątki");
            addBet("Trzy dziesiątki");
            addBet("Trzy walety");
            addBet("Trzy damy");
            addBet("Trzy króle");
            addBet("Trzy asy");

            addBet("Trzy dziewiątki i dwie dziesiątki");
            addBet("Trzy dziewiątki i dwa walety");
            addBet("Trzy dziewiątki i dwie damy");
            addBet("Trzy dziewiątki i dwa króle");
            addBet("Trzy dziewiątki i dwa asy");
            addBet("Trzy dziesiątki i dwie dziewiątki");
            addBet("Trzy dziesiątki i dwa walety");
            addBet("Trzy dziesiątki i dwie damy");
            addBet("Trzy dziesiątki i dwa króle");
            addBet("Trzy dziesiątki i dwa asy");
            addBet("Trzy walety i dwie dziewiątki");
            addBet("Trzy walety i dwie dziesiątki ");
            addBet("Trzy walety i dwie damy");
            addBet("Trzy walety i dwa króle");
            addBet("Trzy walety i dwa asy");
            addBet("Trzy damy i dwie dziewiątki");
            addBet("Trzy damy i dwie dziesiątki");
            addBet("Trzy damy i dwa walety");
            addBet("Trzy damy i dwa króle");
            addBet("Trzy damy i dwa asy");
            addBet("Trzy króle i dwie dziewiątki");
            addBet("Trzy króle i dwie dziesiątki");
            addBet("Trzy króle i dwa walety");
            addBet("Trzy króle i dwie damy");
            addBet("Trzy króle i dwa asy");
            addBet("Trzy asy i dwie dziewiątki");
            addBet("Trzy asy i dwie dziesiątki");
            addBet("Trzy asy i dwa walety");
            addBet("Trzy asy i dwie damy");
            addBet("Trzy asy i dwa króle");

            addBet("Kolor trefl");
            addBet("Kolor karo");
            addBet("Kolor kier");
            addBet("Kolor pik");

            addBet("Kareta dziewiątek");
            addBet("Kareta dziesiątek");
            addBet("Kareta waletów");
            addBet("Kareta dam");
            addBet("Kareta króli");
            addBet("Kareta asów");

            addBet("Mały poker trefl");
            addBet("Mały poker karo");
            addBet("Mały poker kier");
            addBet("Mały poker pik");

            addBet("Duży poker trefl");
            addBet("Duży poker karo");
            addBet("Duży poker kier");
            addBet("Duży poker pik");

            addBet("Sprawdzam");
        }

        private static void addBet(string name)
        {
            int val = betNames.Keys.Count;
            betNames.Add(val, name);
            betValues.Add(name, val);
        }

        public static int getBet(string bet)
        {
            return betValues[bet];
        }

        public static string getBet(int bet)
        {
            return betNames[bet];
        }

        public static bool proper(string name)
        {
            return betValues.ContainsKey(name);
        }

        public static bool proper(int val)
        {
            return betNames.ContainsKey(val);
        }
    }

    /// <summary>
    /// Raised when bet is not in accepted bets set
    /// </summary>
    public class InproperBetException : Exception
    {
    }

    /// <summary>
    /// Single bet
    /// </summary>
    [Serializable]
    public class Bet : IComparable<Bet>
    {
        string name;

        public string Name { get { return name; } }
       
        public Bet(string name)
        {
            if (!BetTranslator.proper(name))
                throw new InproperBetException();
            this.name = name;
        }

        public Bet(int val)
        {
            if (!BetTranslator.proper(val))
                throw new InproperBetException();
            this.name = BetTranslator.getBet(val);
        }

        public int CompareTo(Bet that)
        {
            return BetTranslator.getBet(name) - BetTranslator.getBet(that.name);
        }

        public override string ToString()
        {
            return this.Name;
        }
    }

    /// <summary>
    /// First message from client
    /// </summary>
    [Serializable]
    public class PlayerInfo
    {
        public String playerName { set; get; }
    }

    /// <summary>
    /// All parameters to describe state of player
    /// </summary>
    [Serializable]
    public class PlayerState
    {
        public bool Seated { set; get; }
        public bool Playing { set; get; }
        public bool Winner { set; get; }
        public string Name { set; get; }
        public int Position { set; get; }
        public int CardScore { set; get; }
        public Cards[] Hand { set; get; }

        public PlayerState(int i)
        {
            Seated = false;
            Playing = false;
            Winner = false;
            Position = i;
            CardScore = 0;
            Hand = new Cards[5];
           // Hand[0] = Cards.CK;
            Name = null;
        }

        public override string ToString()
        {
            String result = "";
            if (!Seated)
                result = "<empty sit>";
            else
                result = "<" + Name + ", pl:" + Playing.ToString() + ", pos:" + Position.ToString() + ", card:" + CardScore.ToString() + ", win:" + Winner.ToString() + ">";
            for (int i = 0; i < CardScore; i++)
            {
                result += Hand[i].ToString() + ", ";
            }
            return result;
        }
    }

    /// <summary>
    /// All parameters describing state of game
    /// </summary>
    [Serializable]
    public class GameState
    {
        public bool Active { set; get; }
        public bool EndOfRound { set; get; }
        public int Round { set; get; }
        public List<Bet> BetHistory { set; get; }
        public PlayerState[] Player { set; get; }
        public int NextToMove { set; get; }
        public int AddressedTo { set; get; }
        public String GameName { set; get; }

        public GameState()
        {
            Active = false;
            Round = 0;
            AddressedTo = 1;
            BetHistory = new List<Bet>();
            Player = new PlayerState[6];
            for (int i = 0; i < 6; ++i)
                Player[i] = new PlayerState(i); 
            NextToMove =  1;
            EndOfRound = false;
            Round = 0;
        }

        public override string ToString()
        {
            string result = "";
            result += "Active: " + Active.ToString();
            result += "\nEndOfRound: " + EndOfRound.ToString();
            result += "\nRound: " + Round.ToString();
            result += "\nBetHistory: " + BetHistory.Count();
            result += "\nNextToMove: " + NextToMove.ToString();
            result += "\nAddressedTo: " + AddressedTo.ToString();
            result += "\nGameName: " + GameName;
            for(int i=0; i < 6; i++) if(Player[i].Seated)
                {
                    result += "\ngracz:" + Player[i].ToString();
                }
            return result;
        }

        internal GameState Copy()
        {
            GameState gs = new GameState();
            gs.Active = Active;
            gs.EndOfRound = EndOfRound;
            gs.Round = Round;
            gs.BetHistory = BetHistory;
            gs.Player = Player;
            gs.NextToMove = NextToMove;
            gs.AddressedTo = AddressedTo;
            gs.GameName = GameName;
            return gs;
        }

        public byte[] ToBytes()
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);
            stream.Position = 0;
            return stream.ToArray();
        }

        public static GameState BytesToGamestate(byte[] src)
        {
            MemoryStream stream = new MemoryStream(src);
            BinaryFormatter formatter = new BinaryFormatter();
            return (GameState)formatter.Deserialize(stream);
        }
    }
}
