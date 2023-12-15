using System;
using System.IO;
using System.Numerics;

namespace LuaDec.Parser
{
    internal class BIntegerType50 : BIntegerType
    {
        public readonly bool signed;
        public readonly int intSize;
        public readonly bool allowNegative;

        public BIntegerType50(bool signed, int intSize, bool allowNegative)
        {
            this.signed = signed;
            this.intSize = intSize;
            this.allowNegative = allowNegative;
        }

        protected BInteger RawParse(BinaryReader buffer, BHeader header)
        {
            BInteger value;
            if (signed && (intSize == 0 || intSize == 1 || intSize == 2 || intSize == 4))
            {
                switch (intSize)
                {
                    case 0:
                        value = new BInteger(0);
                        break;
                    case 1:
                        value = new BInteger(buffer.ReadByte());
                        break;
                    case 2:
                        value = new BInteger(buffer.ReadInt16());
                        break;
                    case 4:
                        value = new BInteger(buffer.ReadInt32());
                        break;
                    default:
                        throw new System.InvalidOperationException();
                }
            }
            else
            {
                byte[] bytes = new byte[intSize];
                int start = 0;
                int delta = 1;
                if (!BitConverter.IsLittleEndian)
                {
                    start = intSize - 1;
                    delta = -1;
                }
                for (int i = start; i >= 0 && i < intSize; i += delta)
                {
                    bytes[i] = buffer.ReadByte();
                }

                var innerVal = new BigInteger(bytes);
                if (!signed && innerVal.Sign < 0)
                {
                    innerVal = -innerVal;
                }
                value = new BInteger(innerVal);
            }

            if (!allowNegative && value.Signum() < 0)
            {
                throw new System.InvalidOperationException("Illegal number");
            }
            return value;
        }

        protected void RawWrite(BinaryWriter output, BHeader header, BInteger o)
        {
            byte[] bytes = BitConverter.GetBytes(intSize);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            if (header.lheader.endianness == LHeader.LEndianness.Little)
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

        public override int GetSize()
        {
            return intSize;
        }

        public override BInteger Parse(BinaryReader buffer, BHeader header)
        {
            BInteger value = RawParse(buffer, header);
            if (header.debug)
            {
                Console.WriteLine("-- parsed <int> " + value.AsInt());
            }
            return value;
        }

        public override void Write(BinaryWriter output, BHeader header, BInteger o)
        {
            RawWrite(output, header, o);
        }
    }

    internal class BIntegerType54 : BIntegerType
    {
        public BIntegerType54()
        {
        }

        public override BInteger Parse(BinaryReader buffer, BHeader header)
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

        public override void Write(BinaryWriter output, BHeader header, BInteger o)
        {
            byte[] bytes = o.CompressedBytes();
            for (int i = bytes.Length - 1; i >= 1; i--)
            {
                output.Write(bytes[i]);
            }
            output.Write((byte)(bytes[0] | 0x80));
        }
    }

    public class BIntegerType : BObjectType<BInteger>
    {
        public static BIntegerType Create50Type(bool signed, int intSize, bool allowNegative)
        {
            return new BIntegerType50(signed, intSize, allowNegative);
        }

        public static BIntegerType Create54()
        {
            return new BIntegerType54();
        }

        public BInteger Create(int n)
        {
            return new BInteger(n);
        }

        public virtual int GetSize()
        {
            throw new NotImplementedException();
        }

        public override BInteger Parse(BinaryReader buffer, BHeader header)
        {
            throw new NotImplementedException();
        }

        public override void Write(BinaryWriter output, BHeader header, BInteger o)
        {
            throw new NotImplementedException();
        }
    }
}