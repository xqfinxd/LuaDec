using LuaDec.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile.Block
{
    public class ForBlock51 : ForBlock
    {

        public ForBlock51(LFunction function, int begin, int end, int register, CloseType closeType, int closeLine, bool forvarClose)
            : base(function, begin, end, register, closeType, closeLine, forvarClose)
        {
        }

        public override void resolve(Registers r)
        {
            target = r.GetTarget(register + 3, begin - 1);
            start = r.GetValue(register, begin - 1);
            stop = r.GetValue(register + 1, begin - 1);
            step = r.GetValue(register + 2, begin - 1);
        }

        public override void handleVariableDeclarations(Registers r)
        {
            r.SetInternalLoopVariable(register, begin - 2, end - 1);
            r.SetInternalLoopVariable(register + 1, begin - 2, end - 1);
            r.SetInternalLoopVariable(register + 2, begin - 2, end - 1);
            int explicitEnd = end - 2;
            if (forvarClose && r.GetVersion().closeSemantics.Value != Version.CloseSemantics.Lua54) explicitEnd--;
            r.SetExplicitLoopVariable(register + 3, begin - 1, explicitEnd);
        }

    }

}
