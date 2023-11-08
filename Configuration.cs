
using LuaDec.Decompile;
using System.IO;

namespace LuaDec
{
    public class Configuration
    {
        public enum Mode
        {
            Decompile,
            Disassemble,
            Assemble,
        }

        public enum VariableMode
        {
            Nodebug,
            Default,
            Finder,
        }

        public bool rawString;
        public Mode mode;
        public VariableMode variable;
        public bool strictScope;
        public string opmap;
        public string output;

        public Configuration()
        {
            rawString = false;
            mode = Mode.Decompile;
            variable = VariableMode.Default;
            strictScope = false;
            opmap = null;
            output = null;
        }

        public Configuration(Configuration other)
        {
            rawString = other.rawString;
            mode = other.mode;
            variable = other.variable;
            strictScope = other.strictScope;
            opmap = other.opmap;
            output = other.output;
        }

        public Output getOutput()
        {
            if (output != null)
            {
                try
                {
                    return new Output(new FileOutputProvider(File.Open(output, FileMode.Open)));
                }
                catch (IOException e)
                {
                    Program.error(e.Message, false);
                    return null;
                }
            }
            else
            {
                return new Output();
            }
        }
    }
}
