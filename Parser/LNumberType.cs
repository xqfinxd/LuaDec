using System;
using System.IO;
using System.Numerics;

namespace LuaDec.Parser
{
    public class LNumberType : BObjectType<LNumber>
    {

        public enum NumberMode
        {
            MODE_NUMBER, // Used for Lua 5.0 - 5.2 where numbers can represent ints or floats
            MODE_FLOAT, // Used for floats in Lua 5.3
            MODE_int, // Used for ints in Lua 5.3
        }

        public readonly int size;
        public readonly bool integral;
        public readonly NumberMode mode;

        public LNumberType(int size, bool integral, NumberMode mode)
        {
            this.size = size;
            this.integral = integral;
            this.mode = mode;
            if (!(size == 4 || size == 8))
            {
                throw new System.InvalidOperationException("The input chunk has an unsupported Lua number size: " + size);
            }
        }

        public double convert(double number)
        {
            if (integral)
            {
                switch (size)
                {
                    case 4:
                        return (int)number;
                    case 8:
                        return (long)number;
                }
            }
            else
            {
                switch (size)
                {
                    case 4:
                        return (float)number;
                    case 8:
                        return number;
                }
            }
            throw new System.InvalidOperationException("The input chunk has an unsupported Lua number format");
        }

        public override LNumber parse(BinaryReader buffer, BHeader header)
        {
            LNumber value = null;
            if (integral)
            {
                switch (size)
                {
                    case 4:
                        byte[] intBytes = new byte[4];
                        buffer.Read(intBytes, 0, 4);
                        value = new LIntNumber(BitConverter.ToInt32(intBytes, 0));
                        break;
                    case 8:
                        byte[] longBytes = new byte[8];
                        buffer.Read(longBytes, 0, 8);
                        value = new LLongNumber(BitConverter.ToInt64(longBytes, 0));
                        break;
                }
            }
            else
            {
                switch (size)
                {
                    case 4:
                        byte[] floatBytes = new byte[4];
                        buffer.Read(floatBytes, 0, 4);
                        value = new LFloatNumber(BitConverter.ToSingle(floatBytes, 0), mode);
                        break;
                    case 8:
                        byte[] doubleBytes = new byte[8];
                        buffer.Read(doubleBytes, 0, 8);
                        value = new LDoubleNumber(BitConverter.ToDouble(doubleBytes, 0), mode);
                        break;
                }
            }
            if (value == null)
            {
                throw new System.InvalidOperationException("The input chunk has an unsupported Lua number format");
            }
            if (header.debug)
            {
                Console.WriteLine("-- parsed <number> " + value);
            }
            return value;
        }

        public override void write(BinaryWriter output, BHeader header, LNumber n)
        {
            long bits = n.bits();
            if (header.lheader.endianness == LHeader.LEndianness.LITTLE)
            {
                for (int i = 0; i < size; i++)
                {
                    output.Write((byte)(bits & 0xFF));
                    bits = (int)((uint)bits >> 8);
                }
            }
            else
            {
                for (int i = size - 1; i >= 0; i--)
                {
                    output.Write((byte)((bits >> (i * 8)) & 0xFF));
                }
            }
        }

        public LNumber create(double x)
        {
            if (integral)
            {
                switch (size)
                {
                    case 4:
                        return new LIntNumber((int)x);
                    case 8:
                        return new LLongNumber((long)x);
                    default:
                        throw new System.InvalidOperationException();
                }
            }
            else
            {
                switch (size)
                {
                    case 4:
                        return new LFloatNumber((float)x, mode);
                    case 8:
                        return new LDoubleNumber(x, mode);
                    default:
                        throw new System.InvalidOperationException();
                }
            }
        }

        public LNumber create(BigInteger x)
        {
            if (integral)
            {
                switch (size)
                {
                    case 4:
                        return new LIntNumber((int)x);
                    case 8:
                        return new LLongNumber((long)x);
                    default:
                        throw new System.InvalidOperationException();
                }
            }
            else
            {
                switch (size)
                {
                    case 4:
                        return new LFloatNumber((float)x, mode);
                    case 8:
                        return new LDoubleNumber((double)x, mode);
                    default:
                        throw new System.InvalidOperationException();
                }
            }
        }

    }

}