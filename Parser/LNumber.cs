using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Parser
{
    public abstract class LNumber : LObject
    {

        public static LNumber makeint(int number)
        {
            return new LIntNumber(number);
        }

        public static LNumber makeDouble(double x)
        {
            return new LDoubleNumber(x, LNumberType.NumberMode.MODE_FLOAT);
        }

        public override abstract string ToPrintable();

        //TODO: problem solution for this issue
        public abstract double value();

        public abstract bool integralType();

        public abstract long bits();
    }

    class LFloatNumber : LNumber
    {

        public readonly float number;
        public readonly LNumberType.NumberMode mode;

        public LFloatNumber(float number, LNumberType.NumberMode mode)
        {
            this.number = number;
            this.mode = mode;
        }

        public override string ToPrintable()
        {
            if (mode == LNumberType.NumberMode.MODE_NUMBER && number == (float)Math.Round(number))
            {
                if (BitConverter.DoubleToInt64Bits(number) == BitConverter.DoubleToInt64Bits(-0.0f))
                {
                    return "-0";
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

        public override bool EqualTo(object o)
        {
            if (o is LFloatNumber)
            {
                return BitConverter.DoubleToInt64Bits(number) == BitConverter.DoubleToInt64Bits(((LFloatNumber)o).number);
            }
            else if (o is LNumber)
            {
                return value() == ((LNumber)o).value();
            }
            return false;
        }

        public override double value()
        {
            return number;
        }

        public override bool integralType()
        {
            return false;
        }

        public override long bits()
        {
            return BitConverter.DoubleToInt64Bits(number);
        }

    }

    class LDoubleNumber : LNumber
    {

        public readonly double number;
        public readonly LNumberType.NumberMode mode;

        public LDoubleNumber(double number, LNumberType.NumberMode mode)
        {
            this.number = number;
            this.mode = mode;
        }

        public override string ToPrintable()
        {
            if (mode == LNumberType.NumberMode.MODE_NUMBER && number == (double)Math.Round(number))
            {
                if (BitConverter.DoubleToInt64Bits(number) == BitConverter.DoubleToInt64Bits(-0.0))
                {
                    return "-0";
                }
                else
                {
                    return ((long)number).ToString();
                }
            }
            else
            {
                return number.ToString();
            }
        }

        public override bool EqualTo(object o)
        {
            if (o is LDoubleNumber)
            {
                return BitConverter.DoubleToInt64Bits(number) == BitConverter.DoubleToInt64Bits(((LDoubleNumber)o).number);
            }
            else if (o is LNumber)
            {
                return value() == ((LNumber)o).value();
            }
            return false;
        }

        public override double value()
        {
            return number;
        }

        public override bool integralType()
        {
            return false;
        }

        public override long bits()
        {
            return BitConverter.DoubleToInt64Bits(number);
        }

    }

    class LIntNumber : LNumber
    {

        public readonly int number;

        public LIntNumber(int number)
        {
            this.number = number;
        }

        public override string ToPrintable()
        {
            return number.ToString();
        }

        public override bool EqualTo(Object o)
        {
            if (o is LIntNumber)
            {
                return number == ((LIntNumber)o).number;
            }
            else if (o is LNumber)
            {
                return value() == ((LNumber)o).value();
            }
            return false;
        }

        public override double value()
        {
            return number;
        }

        public override bool integralType()
        {
            return true;
        }

        public override long bits()
        {
            return number;
        }

    }

    class LLongNumber : LNumber
    {

        public readonly long number;

        public LLongNumber(long number)
        {
            this.number = number;
        }

        public override string ToPrintable()
        {
            return number.ToString();
        }

        public override bool EqualTo(object o)
        {
            if (o is LLongNumber)
            {
                return number == ((LLongNumber)o).number;
            }
            else if (o is LNumber)
            {
                return value() == ((LNumber)o).value();
            }
            return false;
        }

        public override double value()
        {
            return number;
        }

        public override bool integralType()
        {
            return true;
        }

        public override long bits()
        {
            return number;
        }

    }
}
