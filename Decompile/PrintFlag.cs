﻿namespace LuaDec.Decompile
{
    public class PrintFlag
    {
        public static readonly int DISASSEMBLER = 0x00000001;
        public static readonly int SHORT = 0x00000002;

        public static bool Test(int flags, int flag)
        {
            return (flags & flag) != 0;
        }
    }
}