namespace LuaDec.Parser
{
    public class LString : LObject
    {
        public static readonly LString EmptyString = new LString("");

        public readonly string value;
        public bool islong;

        public LString(string value, bool islong)
        {
            this.value = value;
            this.islong = islong;
        }

        public LString(string value) : this(value, false)
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