using LuaDec.Decompile.Expression;
using LuaDec.Decompile.Statement;

namespace LuaDec.Decompile
{
    public class Walker
    {
        public virtual void visitExpression(IExpression expr)
        {
        }

        public virtual void visitStatement(IStatement stmt)
        {
        }
    }
}