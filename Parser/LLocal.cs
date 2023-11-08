namespace LuaDec.Parser
{
    public class LLocal : BObject
    {

        public readonly LString name;
        public readonly int start;
        public readonly int end;

        /* Used by the decompiler for annotation. */
        public bool forLoop = false;

        public LLocal(LString name, BInteger start, BInteger end)
        {
            this.name = name;
            this.start = start.asInt();
            this.end = end.asInt();
        }

        public string tostring()
        {
            return name.Deref();
        }

    }

}