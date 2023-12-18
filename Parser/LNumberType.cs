using System;
using System.IO;
using System.Numerics;

namespace LuaDec.Parser
{
    public class LNumberType : BObjectType<LNumber>
    {
        public enum NumberMode
        {
            Number, // Used for Lua 5.0 - 5.2 where numbers can represent ints or floats
            Float, // Used for floats in Lua 5.3+
            Integer, // Used for ints in Lua 5.3+
        }

        public readonly bool integral;
        public readonly NumberMode mode;
        public readonly int size;

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

        public double Convert(double number)
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

        public LNumber CreateNaN(long bits)
        {
            if (integral)
            {
                throw new System.InvalidOperationException();
            }
            else
            {
                switch (size)
                {
                    case 4:
                    {
                        uint fbits = 0x7FC00000;
                        ulong ubits = (ulong)bits;
                        if (bits < 0)
                        {
                            ubits ^= 0x8000000000000000L;
                            fbits ^= 0x80000000;
                        }
                        fbits |= (uint)(ubits >> LFloatNumber.NAN_SHIFT_OFFSET);
                        float number = BitConverter.ToSingle(BitConverter.GetBytes(fbits), 0);
                        return new LFloatNumber(number, mode);
                    }
                    case 8:
                    {
                        double number = BitConverter.ToDouble(BitConverter.GetBytes(0x7FF8000000000000L ^ bits), 0);
                        return new LDoubleNumber(number, mode);
                    }
                    default:
                        throw new System.InvalidOperationException();
                }
            }
        }

        public LNumber Create(double x)
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

        public LNumber Create(BigInteger x)
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

        public override LNumber Parse(BinaryReader buffer, BHeader header)
        {
            LNumber value = null;
            if (integral)
            {
                switch (size)
                {
                    case 4:
                        value = new LIntNumber(buffer.ReadInt32());
                        break;

                    case 8:
                        value = new LLongNumber(buffer.ReadInt64());
                        break;
                }
            }
            else
            {
                switch (size)
                {
                    case 4:
                        value = new LFloatNumber(buffer.ReadSingle(), mode);
                        break;

                    case 8:
                        value = new LDoubleNumber(buffer.ReadDouble(), mode);
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

        public override void Write(BinaryWriter output, BHeader header, LNumber n)
        {
            long bits = n.Bits();
            if (header.lheader.endianness == LHeader.LEndianness.Little)
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
    }
}