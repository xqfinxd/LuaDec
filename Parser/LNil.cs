using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Parser
{
    public class LNil : LObject
    {

        public static readonly LNil NIL = new LNil();

        private LNil()
        {

        }

        public override string ToPrintable()
        {
            return "nil";
        }

        public override bool EqualTo(object o)
        {
            return this == o;
        }

    }

}
