using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessZone
{
    internal class MovementInformation
    {
        public int[] FromPos { get; set; }
        public int[] ToPos { get; set; }

        public MovementInformation(int[] fromPos, int[] toPos)
        {
            this.FromPos = fromPos;
            this.ToPos = toPos;
        }
    }
}
