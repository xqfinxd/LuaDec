using System;

namespace LuaDec.Parser
{
    public abstract class LObject : BObject
    {
        public virtual string Deref()
        {
            throw new NotImplementedException();
        }

        public virtual string ToPrintable(int flags)
        {
            throw new NotImplementedException();
        }
    }
}