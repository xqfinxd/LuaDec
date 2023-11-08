using System;
using System.IO;
using System.Numerics;

namespace LuaDec.Parser
{
    public class BIntegerType : BObjectType<BInteger>
    {
        public override BInteger parse(BinaryReader buffer, BHeader header)
        {
            throw new NotImplementedException();
        }

        public override void write(BinaryWriter output, BHeader header, BInteger o)
        {
            throw new NotImplementedException();
        }

        public static BIntegerType create50Type(int intSize)
        {
            return new BIntegerType50(intSize);
        }

        public static BIntegerType create54()
        {
            return new BIntegerType54();
        }

        public virtual int getSize()
        {
            throw new NotImplementedException();
        }

        public BInteger create(int n)
        {
            return new BInteger(n);
        }

    }

    class BIntegerType50 : BIntegerType
    {

        public readonly int intSize;

        public BIntegerType50(int intSize)
        {
            this.intSize = intSize;
        }

        protected BInteger raw_parse(BinaryReader buffer, BHeader header)
        {
            BInteger value;
            switch (intSize)
            {
                case 0:
                    value = new BInteger(0);
                    break;
                case 1:
                    value = new BInteger(buffer.ReadByte());
                    break;
                case 2:
                    byte[] shortBuffer = new byte[2];
                    buffer.Read(shortBuffer, 0, 2);
                    value = new BInteger(BitConverter.ToInt16(shortBuffer, 0));
                    break;
                case 4:
                    byte[] intBuffer = new byte[4];
                    buffer.Read(intBuffer, 0, 2);
                    value = new BInteger(BitConverter.ToInt32(intBuffer, 0));
                    break;
                default:
                    {
                        byte[] bytes = new byte[intSize];
                        int start = 0;
                        int delta = 1;
                        if (BitConverter.IsLittleEndian)
                        {
                            start = intSize - 1;
                            delta = -1;
                        }
                        for (int i = start; i >= 0 && i < intSize; i += delta)
                        {
                            bytes[i] = buffer.ReadByte();
                        }
                        value = new BInteger(new BigInteger(bytes));
                    }
                    break;
            }
            return value;
        }

        protected void raw_write(BinaryWriter output, BHeader header, BInteger o)
        {

            byte[] bytes = BitConverter.GetBytes(intSize);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            if (header.lheader.endianness == LHeader.LEndianness.LITTLE)
            {
                foreach (byte b in bytes)
                {
                    output.Write(b);
                }
            }
            else
            {
                for (int i = bytes.Length - 1; i >= 0; i--)
                {
                    output.Write(bytes[i]);
                }
            }
        }

        public override BInteger parse(BinaryReader buffer, BHeader header)
        {
            BInteger value = raw_parse(buffer, header);
            if (header.debug)
            {
                Console.WriteLine("-- parsed <int> " + value.asInt());
            }
            return value;
        }

        public override void write(BinaryWriter output, BHeader header, BInteger o)
        {
            raw_write(output, header, o);
        }

        public override int getSize()
        {
            return intSize;
        }

    }

    class BIntegerType54 : BIntegerType
    {

        public BIntegerType54()
        {

        }

        public override BInteger parse(BinaryReader buffer, BHeader header)
        {
            long x = 0;
            byte b;
            do
            {
                b = (byte)buffer.ReadByte();
                x = (x << 7) | (long)(b & 0x7F);
            } while ((b & 0x80) == 0);
            if (int.MinValue <= x && x <= int.MaxValue)
            {
                return new BInteger((int)x);
            }
            else
            {
                return new BInteger(new BigInteger(x));
            }
        }

        public override void write(BinaryWriter output, BHeader header, BInteger o)
        {
            byte[] bytes = o.compressedBytes();
            for (int i = bytes.Length - 1; i >= 1; i--)
            {
                output.Write(bytes[i]);
            }
            output.Write((byte)(bytes[0] | 0x80));
        }

    }

}