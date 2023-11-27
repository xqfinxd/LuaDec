using System;

namespace LuaDec.Assemble
{
    public class AssemblerException : Exception
    {
        public AssemblerException(string msg) : base(msg)
        {
        }
    }
}