using LuaDec;

namespace Test
{
    public class LuaDecSpec
    {
        public bool disassemble;

        public LuaDecSpec()
        {
            disassemble = false;
        }

        public void Run(string input, string output, Configuration config)
        {
            if (!disassemble)
            {
                Program.Decompile(input, output, config);
            }
            else
            {
                Program.Disassemble(input, output);
            }
        }
    }
}