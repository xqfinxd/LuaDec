using LuaDec.Decompile.Expression;
using LuaDec.Decompile.Statement;

namespace LuaDec.Decompile
{
    public class Walker
    {
        public virtual void VisitExpression(IExpression expr)
        {
            // nothing to do
        }

        public virtual void VisitStatement(IStatement stmt)
        {
            // nothing to do
        }
    }
}