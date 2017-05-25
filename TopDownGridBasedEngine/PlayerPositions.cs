using System;

namespace TopDownGridBasedEngine
{
    public struct PlayerPositions
    {
        public int[] X;
        public int[] Y;
        public float[] VelX;
        public float[] VelY;

        public PlayerPositions(Joueur[] players)
        {
            X = new int[4];
            Y = new int[4];
            VelX = new float[4];
            VelY = new float[4];

            for (int i = 0; i < 4; i++)
            {
                if (players[i] != null)
                {
                    X[i] = players[i].X;
                    Y[i] = players[i].Y;
                    VelX[i] = players[i].VelX;
                    VelY[i] = players[i].VelY;
                }
            }
        }

        public void Update(byte id, int x, int y, float velx, float vely)
        {
            X[id] = x;
            Y[id] = y;
            VelX[id] = velx;
            VelY[id] = vely;
        }

        public byte[] ToByteArray()
        {
            byte[] ret = new byte[64];
            for (int i = 0; i < 4; i++)
            {
                BitConverter.GetBytes(X[i]).CopyTo(ret, i * 4);
                BitConverter.GetBytes(Y[i]).CopyTo(ret, 16 + i * 4);
                BitConverter.GetBytes(VelX[i]).CopyTo(ret, 32 + i * 4);
                BitConverter.GetBytes(VelY[i]).CopyTo(ret, 48 + i * 4);
            }
            return ret;
        }

        public bool Decode(byte[] data, ref int pos)
        {
            if (data.Length < pos + 64)
                return false;
            X = new int[4];
            Y = new int[4];
            VelX = new float[4];
            VelY = new float[4];
            for (int i = 0; i < 4; i++)
            {
                X[i] =    BitConverter.ToInt32(data, pos + i * 4);
                Y[i] =    BitConverter.ToInt32(data, 16 + pos + i * 4);
                VelX[i] = BitConverter.ToSingle(data, 32 + pos + i * 4);
                VelY[i] = BitConverter.ToSingle(data, 48 + pos + i * 4);
            }
            pos += 64;
            return true;
        }
    } 
}
