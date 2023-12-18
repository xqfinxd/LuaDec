using LuaDec.Decompile;
using LuaDec.Util;

namespace LuaDec.Parser
{
    public class LString : LObject
    {
        public static readonly LString EmptyString = new LString("");

        public readonly string value;
        public readonly char terminator;
        public bool islong;

        public LString(string value, char terminator, bool islong)
        {
            this.value = value;
            this.islong = islong;
            this.terminator = terminator;
        }

        public LString(string value, char terminator) : this(value, terminator, false)
        {
        }

        public LString(string value) : this(value, '\0', false)
        {
        }

        public override string Deref()
        {
            return value;
        }

        public override bool Equals(object o)
        {
            if (this == EmptyString || o == EmptyString)
            {
                return this == o;
            }
            else if (o is LString)
            {
                LString os = (LString)o;
                return os.value.Equals(value) && os.islong == islong;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToPrintable(int flags)
        {
            if (this == EmptyString)
            {
                return "null";
            }
            else
            {
                string prefix = "";
                string suffix = "";
                if (islong) prefix = "L";
                if (PrintFlag.Test(flags, PrintFlag.SHORT))
                {
                    const int LIMIT = 20;
                    if (value.Length > LIMIT) suffix = " (truncated)";
                    return prefix + StringUtils.ToString(value, LIMIT) + suffix;
                }
                else
                {
                    return prefix + StringUtils.ToString(value);
                }
            }
        }
    }
}