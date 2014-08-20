using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluffGame
{
    public class ClientState
    {
        public GameState CurrentGameState { set; get; }
        public int ClientPosition { set; get; }
        public bool ClientMove { set; get; }
        public bool Host { set; get; }
        public List<Bet> RemainingBets { set; get; }
        public String PlayerName { set; get; }
        public String HostIP { set; get; }
        public BluffClient Client { set; get; }
        public BluffServer Server { set; get; }

        public ClientState()
        {
            ClientPosition = -1;
            ClientMove = false;
            RemainingBets = new List<Bet>();
            for (int i=0; i<83; ++i)
                RemainingBets.Add(new Bet(i));
            PlayerName = "Anonim";
            Host = false;
            CurrentGameState = new GameState();
        }
    }
}
