using LuaDec.Decompile.Expression;

namespace LuaDec.Decompile.Statement
{
    public class FunctionCallStatement : IStatement
    {
        private FunctionCall call;

        public FunctionCallStatement(FunctionCall call)
        {
            this.call = call;
        }

        public override bool BeginsWithParen()
        {
            return call.beginsWithParen();
        }

        public override void Walk(Walker w)
        {
            w.VisitStatement(this);
            call.walk(w);
        }

        public override void Write(Decompiler d, Output output)
        {
            call.print(d, output);
        }
    }
}