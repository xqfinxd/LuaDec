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
        public override void walk(Walker w)
        {
            w.VisitStatement(this);
            call.walk(w);
        }
        public override void print(Decompiler d, Output output)
        {
            call.print(d, output);
        }
        public override bool beginsWithParen()
        {
            return call.beginsWithParen();
        }
    }

}
