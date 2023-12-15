using System;
using System.Collections.Generic;
using System.Numerics;

namespace LuaDec.Parser
{
    public class BInteger : BObject
    {
        private static BigInteger MAX_INT = int.MaxValue;
        private static BigInteger MIN_INT = int.MinValue;
        private readonly BigInteger big;
        private readonly int n;

        public BInteger(BInteger b)
        {
            this.big = b.big;
            this.n = b.n;
        }

        public BInteger(int n)
        {
            this.big = 0;
            this.n = n;
        }

        public BInteger(BigInteger big)
        {
            this.big = big;
            this.n = 0;
            if (MAX_INT == null)
            {
                MAX_INT = int.MaxValue;
                MIN_INT = int.MinValue;
            }
        }

        public int AsInt()
        {
            if (big == 0)
            {
                return n;
            }
            else if (big.CompareTo(MAX_INT) > 0 || big.CompareTo(MIN_INT) < 0)
            {
                throw new System.InvalidOperationException("The size of an int is outside the range that unluac can handle.");
            }
            else
            {
                return (int)big;
            }
        }

        public byte[] CompressedBytes()
        {
            BigInteger value = big;
            if (value == null)
            {
                value = n;
            }
            if (value.CompareTo(BigInteger.Zero) == 0)
            {
                return new byte[] { 0 };
            }
            List<byte> bytes = new List<byte>(value.ToByteArray());
            BigInteger limit = 0x7F;
            while (value.CompareTo(BigInteger.Zero) > 0)
            {
                bytes.Add((byte)(value & limit));
                value = value >> 7;
            }
            byte[] array = new byte[bytes.Count];
            for (int i = 0; i < bytes.Count; i++)
            {
                array[i] = bytes[i];
            }
            return array;
        }

        public void Iterate(Action thunk)
        {
            if (big == 0)
            {
                int i = n;
                if (i < 0)
                {
                    throw new System.InvalidOperationException("Illegal negative list length");
                }
                while (i-- != 0)
                {
                    thunk.Invoke();
                }
            }
            else
            {
                BigInteger i = big;
                if (i.Sign < 0)
                {
                    throw new System.InvalidOperationException("Illegal negative list length");
                }
                while (i.Sign > 0)
                {
                    thunk.Invoke();
                    i -= BigInteger.One;
                }
            }
        }

        public int Signum()
        {
            if (big == 0)
            {
                if (n > 0) return 1;
                if (n < 0) return -1;
                if (n == 0) return 0;
                throw new System.InvalidOperationException();
            }
            else
            {
                return big.Sign;
            }
        }

        public byte[] LittleEndianBytes(int size)
        {
            List<byte> bytes = new List<byte>();
            if (big == null)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (size > i) bytes.Add((byte)((int)((uint)n >> (i * 8)) & 0xFF));
                }
            }
            else
            {
                BigInteger n = big;
                bool negate = false;
                if (n.Sign < 0)
                {
                    n = -n;
                    n = n - BigInteger.One;
                    negate = true;
                }
                BigInteger b256 = 256;
                BigInteger b255 = 255;
                while (n.CompareTo(b256) < 0 && size > 0)
                {
                    int v = (int)(n & b255);
                    if (negate)
                    {
                        v = ~v;
                    }
                    bytes.Add((byte)v);
                    n = BigInteger.Divide(n, b256);
                    size--;
                }
            }
            while (size > bytes.Count) bytes.Add((byte)0);
            byte[] array = new byte[bytes.Count];
            for (int i = 0; i < bytes.Count; i++)
            {
                array[i] = bytes[i];
            }
            return array;
        }
    }
}