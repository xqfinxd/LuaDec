namespace LuaDec.Parser
{
    public class LBoolean : LObject
    {
        private readonly bool value;
        public static readonly LBoolean LFALSE = new LBoolean(false);
        public static readonly LBoolean LTRUE = new LBoolean(true);
        public bool Value => value;

        private LBoolean(bool value)
        {
            this.value = value;
        }

        public override bool Equals(object o)
        {
            return this == o;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToPrintable(int flags)
        {
            return value.ToString();
        }
    }
}