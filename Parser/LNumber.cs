using LuaDec.Decompile;
using System;

namespace LuaDec.Parser
{
    public abstract class LNumber : LObject
    {
        public static LNumber MakeDouble(double x)
        {
            return new LDoubleNumber(x, LNumberType.NumberMode.Float);
        }

        public static LNumber MakeInt(int number)
        {
            return new LIntNumber(number);
        }

        public abstract long Bits();

        public abstract double GetValue();

        public abstract bool IntegralType();

        public abstract override string ToPrintable(int flags);
    }

    internal class LFloatNumber : LNumber
    {
        public readonly static int NAN_SHIFT_OFFSET = 52 - 23;
        public readonly LNumberType.NumberMode mode;
        public readonly float number;

        public LFloatNumber(float number, LNumberType.NumberMode mode)
        {
            this.number = number;
            this.mode = mode;
        }

        public override long Bits()
        {
            return BitConverter.DoubleToInt64Bits(number);
        }

        public override bool Equals(object o)
        {
            if (o is LFloatNumber)
            {
                return BitConverter.DoubleToInt64Bits(number) == BitConverter.DoubleToInt64Bits(((LFloatNumber)o).number);
            }
            else if (o is LNumber)
            {
                return GetValue() == ((LNumber)o).GetValue();
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override double GetValue()
        {
            return number;
        }

        public override bool IntegralType()
        {
            return false;
        }

        public override string ToPrintable(int flags)
        {
            bool isInteger = float.IsInfinity(number) ? false : (number == Math.Round(number));
            if (mode == LNumberType.NumberMode.Number && isInteger)
            {
                if (BitConverter.DoubleToInt64Bits(number) == BitConverter.DoubleToInt64Bits(-0.0f))
                {
                    return "(-0)";
                }
                else
                {
                    return ((int)number).ToString();
                }
            }
            else
            {
                if (float.IsInfinity(number))
                {
                    return number > 0.0 ? "1e9999" : "-1e9999";
                }
                else if (float.IsNaN(number))
                {
                    if (PrintFlag.Test(flags, PrintFlag.DISASSEMBLER))
                    {
                        uint bits = BitConverter.ToUInt32(BitConverter.GetBytes(number), 0);
                        int canonical = 0x7FC00000;
                        if (bits == canonical)
                        {
                            return "NaN";
                        }
                        else
                        {
                            string sign = "+";
                            if (bits < 0)
                            {
                                bits ^= 0x80000000;
                                sign = "-";
                            }
                            long lbits = bits ^ canonical;
                            return "NaN" + sign + (lbits << NAN_SHIFT_OFFSET).ToString("X");
                        }
                    }
                    else
                    {
                        return "(0/0)";
                    }
                }
                else
                {
                    if (isInteger)
                    {
                        return number.ToString("N1");
                    }
                    else
                    {
                        return number.ToString("R");
                    }
                }
            }
        }
    }

    internal class LDoubleNumber : LNumber
    {
        public readonly LNumberType.NumberMode mode;
        public readonly double number;

        public LDoubleNumber(double number, LNumberType.NumberMode mode)
        {
            this.number = number;
            this.mode = mode;
        }

        public override long Bits()
        {
            return BitConverter.DoubleToInt64Bits(number);
        }

        public override bool Equals(object o)
        {
            if (o is LDoubleNumber)
            {
                return BitConverter.DoubleToInt64Bits(number) == BitConverter.DoubleToInt64Bits(((LDoubleNumber)o).number);
            }
            else if (o is LNumber)
            {
                return GetValue() == ((LNumber)o).GetValue();
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override double GetValue()
        {
            return number;
        }

        public override bool IntegralType()
        {
            return false;
        }

        public override string ToPrintable(int flags)
        {
            bool isInteger = double.IsInfinity(number) ? false : (number == Math.Round(number));
            if (mode == LNumberType.NumberMode.Number && isInteger)
            {
                if (BitConverter.DoubleToInt64Bits(number) == BitConverter.DoubleToInt64Bits(-0.0))
                {
                    return "(-0)";
                }
                else
                {
                    return ((long)number).ToString();
                }
            }
            else
            {
                if (double.IsInfinity(number))
                {
                    return number > 0.0 ? "1e9999" : "-1e9999";
                }
                else if (double.IsNaN(number))
                {
                    if (PrintFlag.Test(flags, PrintFlag.DISASSEMBLER))
                    {
                        ulong bits = BitConverter.ToUInt64(BitConverter.GetBytes(number), 0);
                        ulong canonical = 0x7FF8000000000000L;
                        if (bits == canonical)
                        {
                            return "NaN";
                        }
                        else
                        {
                            string sign = "+";
                            if (bits < 0)
                            {
                                bits ^= 0x8000000000000000L;
                                sign = "-";
                            }
                            return "NaN" + sign + (bits ^ canonical).ToString("X");
                        }
                    }
                    else
                    {
                        return "(0/0)";
                    }
                }
                else
                {
                    if (isInteger)
                    {
                        return number.ToString("N1");
                    }
                    else
                    {
                        return number.ToString("R");
                    }
                }
            }
        }
    }

    internal class LIntNumber : LNumber
    {
        public readonly int number;

        public LIntNumber(int number)
        {
            this.number = number;
        }

        public override long Bits()
        {
            return number;
        }

        public override bool Equals(object o)
        {
            if (o is LIntNumber)
            {
                return number == ((LIntNumber)o).number;
            }
            else if (o is LNumber)
            {
                return GetValue() == ((LNumber)o).GetValue();
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override double GetValue()
        {
            return number;
        }

        public override bool IntegralType()
        {
            return true;
        }

        public override string ToPrintable(int flags)
        {
            return number.ToString();
        }
    }

    internal class LLongNumber : LNumber
    {
        public readonly long number;

        public LLongNumber(long number)
        {
            this.number = number;
        }

        public override long Bits()
        {
            return number;
        }

        public override bool Equals(object o)
        {
            if (o is LLongNumber)
            {
                return number == ((LLongNumber)o).number;
            }
            else if (o is LNumber)
            {
                return GetValue() == ((LNumber)o).GetValue();
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override double GetValue()
        {
            return number;
        }

        public override bool IntegralType()
        {
            return true;
        }

        public override string ToPrintable(int flags)
        {
            return number.ToString();
        }
    }

}