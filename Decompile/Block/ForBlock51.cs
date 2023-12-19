using LuaDec.Parser;

namespace LuaDec.Decompile.Block
{
    public class ForBlock51 : ForBlock
    {
        protected bool forvarPostClose;

        public ForBlock51(LFunction function, int begin, int end, int register,
            CloseType closeType, int closeLine, bool forvarPreClose, bool forvarPostClose)
            : base(function, begin, end, register, closeType, closeLine, forvarPreClose)
        {
            this.forvarPostClose = forvarPostClose;
        }

        public override void HandleVariableDeclarations(Registers r)
        {
            int implicitEnd = end - 1;
            if (forvarPostClose) implicitEnd++;
            r.SetInternalLoopVariable(register, begin - 2, implicitEnd);
            r.SetInternalLoopVariable(register + 1, begin - 2, implicitEnd);
            r.SetInternalLoopVariable(register + 2, begin - 2, implicitEnd);

            int explicitEnd = end - 2;
            if (forvarPreClose && r.GetVersion().closeSemantics.Value == Version.CloseSemantics.Default)
                explicitEnd--;
            r.SetExplicitLoopVariable(register + 3, begin - 1, explicitEnd);
        }

        public override void Resolve(Registers r)
        {
            target = r.GetTarget(register + 3, begin - 1);
            start = r.GetValue(register, begin - 1);
            stop = r.GetValue(register + 1, begin - 1);
            step = r.GetValue(register + 2, begin - 1);
        }
    }
}