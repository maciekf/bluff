using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BluffGame
{
    public static class Positions
    {
        private static Dictionary<int, Tuple<int, int>> nameLabels;
        private static Dictionary<int, Tuple<int, int>> cards;
        public static int Height { set;  get; }
        public static int Width { set; get; }
        static Positions()
        {
            nameLabels = new Dictionary<int, Tuple<int, int>>();
            cards = new Dictionary<int,Tuple<int,int>>();
            Height = 90;
            Width = 60;

            nameLabels.Add(0, new Tuple<int, int>(30,20));
            nameLabels.Add(1, new Tuple<int, int>(285, -10));
            nameLabels.Add(2, new Tuple<int, int>(540, 20));
            nameLabels.Add(3, new Tuple<int, int>(565, 355));
            nameLabels.Add(4, new Tuple<int, int>(285, 400));
            nameLabels.Add(5, new Tuple<int, int>(0, 355));

            cards.Add(0, new Tuple<int, int>(60,80));
            cards.Add(1, new Tuple<int, int>(295, 50));
            cards.Add(2, new Tuple<int, int>(575, 80));
            cards.Add(3, new Tuple<int, int>(575, 265));
            cards.Add(4, new Tuple<int, int>(305, 310));
            cards.Add(5, new Tuple<int, int>(60, 265));
        }

        public static Tuple<int, int> NameLabelPosition(int position)
        {
            return nameLabels[position];
        }
        public static Tuple<int, int> CardPosition(int position, int cardNumber)
        {
            return new Tuple<int, int>(cards[position].Item1 + (Width / 2) * (cardNumber - 2), cards[position].Item2);
        }
    }
}
