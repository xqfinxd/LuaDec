namespace LuaDec.Parser
{
    public class LNil : LObject
    {
        public static readonly LNil NIL = new LNil();

        private LNil()
        {
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
            return "nil";
        }
    }
}