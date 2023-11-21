using LuaDec.Decompile.Expression;
using LuaDec.Decompile.Statement;

namespace LuaDec.Decompile
{
    public class Walker
    {
        public virtual void VisitExpression(IExpression expr)
        {
            throw new System.NotImplementedException();
        }

        public virtual void VisitStatement(IStatement stmt)
        {
            throw new System.NotImplementedException();
        }
    }
}