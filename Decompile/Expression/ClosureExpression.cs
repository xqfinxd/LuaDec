using LuaDec.Decompile.Target;
using LuaDec.Parser;

namespace LuaDec.Decompile.Expression
{
    public class ClosureExpression : IExpression
    {
        private readonly LFunction function;
        private int upvalueLine;

        public ClosureExpression(LFunction function, int upvalueLine)
            : base(PRECEDENCE_ATOMIC)
        {
            this.function = function;
            this.upvalueLine = upvalueLine;
        }

        private void WriteMain(Output output, Decompiler d, bool includeFirst)
        {
            output.WriteString("(");
            int start = includeFirst ? 0 : 1;
            if (function.numParams > start)
            {
                new VariableTarget(d.declarations[start]).Write(d, output, false);
                for (int i = start + 1; i < function.numParams; i++)
                {
                    output.WriteString(", ");
                    new VariableTarget(d.declarations[i]).Write(d, output, false);
                }
            }
            if (function.varArg != 0)
            {
                if (function.numParams > start)
                {
                    output.WriteString(", ...");
                }
                else
                {
                    output.WriteString("...");
                }
            }
            output.WriteString(")");
            output.WriteLine();
            output.Indent();
            Decompiler.State result = d.Decompile();
            d.Write(result, output);
            output.Dedent();
            output.WriteString("end");
            //output.println(); //This is an extra space for formatting
        }

        public override int ClosureUpvalueLine()
        {
            return upvalueLine;
        }

        public override int GetConstantIndex()
        {
            return -1;
        }

        public override bool IsClosure()
        {
            return true;
        }

        public override bool IsUngrouped()
        {
            return true;
        }

        public override bool IsUpvalueOf(int register)
        {
            for (int i = 0; i < function.upvalues.Length; i++)
            {
                LUpvalue upvalue = function.upvalues[i];
                if (upvalue.instack && upvalue.idx == register)
                {
                    return true;
                }
            }
            return false;
        }

        public override void Walk(Walker w)
        {
            w.VisitExpression(this);
        }

        public override void Write(Decompiler outer, Output output)
        {
            Decompiler d = new Decompiler(function, outer.declarations, upvalueLine);
            output.WriteString("function");
            WriteMain(output, d, true);
        }

        public override void WriteClosure(Decompiler outer, Output output, ITarget name)
        {
            Decompiler d = new Decompiler(function, outer.declarations, upvalueLine);
            output.WriteString("function ");
            if (function.numParams >= 1 && d.declarations[0].name == "self" && name is TableTarget)
            {
                name.WriteMethod(outer, output);
                WriteMain(output, d, false);
            }
            else
            {
                name.Write(outer, output, false);
                WriteMain(output, d, true);
            }
        }
    }
}