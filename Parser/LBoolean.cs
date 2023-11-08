namespace LuaDec.Parser
{
    public class LBoolean : LObject
    {

        public static readonly LBoolean LTRUE = new LBoolean(true);
        public static readonly LBoolean LFALSE = new LBoolean(false);

        private readonly bool value;

        private LBoolean(bool value)
        {
            this.value = value;
        }

        public bool Value => value;

        public string toPrintstring()
        {
            return value.ToString();
        }

        public override bool EqualTo(object o)
        {
            return this == o;
        }

    }

}