namespace LuaDec.Parser
{
    public class LUpvalue : BObject
    {

        public bool instack;
        public int idx;

        public string name;
        public LString bname;
        public int kind;

        public bool equals(object obj)
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

    }
}