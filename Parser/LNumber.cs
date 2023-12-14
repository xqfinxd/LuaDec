using System;

namespace LuaDec.Parser
{
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

        public override string ToPrintable()
        {
            bool isInteger = (number == (double)Math.Round(number));
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
                if(isInteger)
                {
                    return number.ToString("N1");
                }
                else
                {
                    return number.ToString();
                }
            }
        }
    }

    internal class LFloatNumber : LNumber
    {
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

        public override string ToPrintable()
        {
            if (mode == LNumberType.NumberMode.Number && number == (float)Math.Round(number))
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
                return number.ToString();
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

        public override bool Equals(Object o)
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

        public override string ToPrintable()
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

        public override string ToPrintable()
        {
            return number.ToString();
        }
    }

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

        public abstract override string ToPrintable();
    }
}