using LuaDec.Parser;

namespace LuaDec.Decompile.Block
{
    public class ForBlock50 : ForBlock
    {
        public ForBlock50(LFunction function, int begin, int end, int register, CloseType closeType, int closeLine)
            : base(function, begin, end, register, closeType, closeLine, false)
        {
        }

        public override void HandleVariableDeclarations(Registers r)
        {
            r.SetExplicitLoopVariable(register, begin - 1, end - 1);
            r.SetInternalLoopVariable(register + 1, begin - 1, end - 1);
            r.SetInternalLoopVariable(register + 2, begin - 1, end - 1);
        }

        public override void Resolve(Registers r)
        {
            target = r.GetTarget(register, begin - 1);
            start = r.GetValue(register, begin - 2);
            stop = r.GetValue(register + 1, begin - 1);
            step = r.GetValue(register + 2, begin - 1);
        }
    }
}