namespace LuaDec.Parser
{
    public class LUpvalue : BObject
    {
        public LString bname;
        public int idx;
        public bool instack;
        public int kind;
        public string name;

        public override bool Equals(object obj)
        {
            if (obj is LUpvalue)
            {
                LUpvalue upvalue = (LUpvalue)obj;
                if (!(instack == upvalue.instack && idx == upvalue.idx && kind == upvalue.kind))
                {
                    return false;
                }
                if (name == upvalue.name)
                {
                    return true;
                }
                return name != null && name == upvalue.name;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}