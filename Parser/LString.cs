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

        public override string ToShortString()
        {
            if (this == EmptyString)
            {
                return "null";
            }
            else
            {
                int LIMIT = 20;
                string suffix = "";
                if (value.Length > LIMIT) suffix = " (truncated)";
                return Util.StringUtils.ToString(value, LIMIT) + suffix;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToPrintable()
        {
            if (this == EmptyString)
            {
                return "null";
            }
            else
            {
                string prefix = "";
                if (islong) prefix = "L";
                return prefix;
            }
        }
    }
}