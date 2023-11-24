using LuaDec.Decompile.Expression;
using LuaDec.Parser;

namespace LuaDec.Decompile
{
    public class Upvalues
    {
        private readonly LUpvalue[] upvalues;

        public Upvalues(LFunction func, Declaration[] parentDecls, int line)
        {
            this.upvalues = func.upvalues;
            foreach (LUpvalue upvalue in upvalues)
            {
                if (upvalue.name == null || upvalue.name.Length == 0)
                {
                    if (upvalue.instack)
                    {
                        if (parentDecls != null)
                        {
                            foreach (Declaration decl in parentDecls)
                            {
                                if (decl.register == upvalue.idx && line >= decl.begin && line < decl.end)
                                {
                                    upvalue.name = decl.name;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        LUpvalue[] parentvals = func.parent.upvalues;
                        if (upvalue.idx >= 0 && upvalue.idx < parentvals.Length)
                        {
                            upvalue.name = parentvals[upvalue.idx].name;
                        }
                    }
                }
            }
        }

        public UpvalueExpression GetExpression(int index)
        {
            return new UpvalueExpression(GetName(index));
        }

        public string GetName(int index)
        {
            if (index < upvalues.Length && upvalues[index].name != null && upvalues[index].name.Length != 0)
            {
                return upvalues[index].name;
            }
            else
            {
                return "_UPVALUE" + index + "_";
            }
        }
    }
}