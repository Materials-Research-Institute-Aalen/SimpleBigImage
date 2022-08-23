using System;

namespace SimpleBigImage2
{
    public class BasicDefinitions
    {
        public const int TOP = 0;
        public const int RIG = 1;
        public const int BOT = 2;
        public const int LEF = 3;

        public const int VAL_MAX = 3;
        public const int VAL_LOOPBOUND = 4;

        public static int OPPOSITE(int i)
        {
            switch (i)
            {
                case TOP: return BOT;
                case LEF: return RIG;
                case BOT: return TOP;
                case RIG: return LEF;
            }
            throw new Exception("not possible");
        }
    }
}
