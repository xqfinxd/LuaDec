namespace LuaDec.Parser
{
    public class LFunction : BObject
    {
        public LAbsLineInfo[] absLineInfo;
        public int[] code;
        public LObject[] constants;
        public LFunction[] functions;
        public BHeader header;
        public int lastLineDefined;
        public int level;
        public int lineDefined;
        public int[] lines;
        public LLocal[] locals;
        public int maximumStackSize;
        public LString name;
        public int numParams;
        public int numUpvalues;
        public LFunction parent;
        public bool stripped;
        public LUpvalue[] upvalues;
        public int varArg;

        public LFunction(BHeader header, LString name, int linedefined, int lastlinedefined, int[] code, int[] lines, LAbsLineInfo[] abslineinfo, LLocal[] locals, LObject[] constants, LUpvalue[] upvalues, LFunction[] functions, int maximumStackSize, int numUpValues, int numParams, int vararg)
        {
            this.header = header;
            this.name = name;
            this.lineDefined = linedefined;
            this.lastLineDefined = lastlinedefined;
            this.code = code;
            this.lines = lines;
            this.absLineInfo = abslineinfo;
            this.locals = locals;
            this.constants = constants;
            this.upvalues = upvalues;
            this.functions = functions;
            this.maximumStackSize = maximumStackSize;
            this.numUpvalues = numUpValues;
            this.numParams = numParams;
            this.varArg = vararg;
            this.stripped = false;
        }

        public void SetLevel(int level)
        {
            this.level = level;
            foreach (var f in functions)
            {
                f.SetLevel(level + 1);
            }
        }
    }
}