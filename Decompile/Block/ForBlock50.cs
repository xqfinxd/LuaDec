using LuaDec.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile.Block
{
    public class ForBlock50 : ForBlock
    {

        public ForBlock50(LFunction function, int begin, int end, int register, CloseType closeType, int closeLine)
            : base(function, begin, end, register, closeType, closeLine, false)
        {
        }

        public override void resolve(Registers r)
        {
            target = r.getTarget(register, begin - 1);
            start = r.getValue(register, begin - 2);
            stop = r.getValue(register + 1, begin - 1);
            step = r.getValue(register + 2, begin - 1);
        }

        public override void handleVariableDeclarations(Registers r)
        {
            r.setExplicitLoopVariable(register, begin - 1, end - 1);
            r.setInternalLoopVariable(register + 1, begin - 1, end - 1);
            r.setInternalLoopVariable(register + 2, begin - 1, end - 1);
        }

    }

}
