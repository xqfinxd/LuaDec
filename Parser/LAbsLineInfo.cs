namespace LuaDec.Parser
{
    public class LAbsLineInfo : LObject
    {

        public readonly int pc;
        public readonly int line;

        public LAbsLineInfo(int pc, int line)
        {
            this.pc = pc;
            this.line = line;
        }

        public override bool EqualTo(object o)
        {
            if (o is LAbsLineInfo)
            {
                LAbsLineInfo other = (LAbsLineInfo)o;
                return pc == other.pc && line == other.line;
            }
            else
            {
                return false;
            }
        }

    }

}