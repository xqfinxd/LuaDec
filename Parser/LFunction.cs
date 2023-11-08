
namespace LuaDec.Parser
{
    public class LFunction : BObject
    {
        public BHeader header;
        public LString name;
        public int lineDefined;
        public int lastLineDefined;
        public LFunction parent;
        public int[] code;
        public int[] lines;
        public LAbsLineInfo[] absLineInfo;
        public LLocal[] locals;
        public LObject[] constants;
        public LUpvalue[] upvalues;
        public LFunction[] functions;
        public int maximumStackSize;
        public int numUpvalues;
        public int numParams;
        public int varArg;
        public bool stripped;
        public int level;

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
