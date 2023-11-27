namespace LuaDec.Parser
{
    public class LLocal : BObject
    {
        public readonly int end;
        public readonly LString name;
        public readonly int start;
        public bool forLoop = false;

        public LLocal(LString name, BInteger start, BInteger end)
        {
            this.name = name;
            this.start = start.AsInt();
            this.end = end.AsInt();
        }

        public override string ToString()
        {
            return name.Deref();
        }
    }
}