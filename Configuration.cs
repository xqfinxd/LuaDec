using LuaDec.Decompile;
using System.IO;

namespace LuaDec
{
    public class Configuration
    {
        public enum OpMode
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

        private OpMode mode;
        private string opMapFile;
        private string output;
        private bool rawString;
        private bool strictScope;
        private VariableMode variable;

        public OpMode Mode { get => mode; set => mode = value; }
        public string OpMapFile { get => opMapFile; set => opMapFile = value; }
        public string Output { get => output; set => output = value; }
        public bool RawString { get => rawString; set => rawString = value; }
        public bool StrictScope { get => strictScope; set => strictScope = value; }
        public VariableMode Variable { get => variable; set => variable = value; }

        public Configuration()
        {
            rawString = false;
            mode = OpMode.Decompile;
            variable = VariableMode.Default;
            strictScope = false;
            opMapFile = null;
            output = null;
        }

        public Configuration(Configuration other)
        {
            rawString = other.RawString;
            mode = other.Mode;
            variable = other.Variable;
            strictScope = other.StrictScope;
            opMapFile = other.OpMapFile;
            output = other.Output;
        }

        public Output GetOutput()
        {
            if (Output != null)
            {
                try
                {
                    return new Output(new FileOutputProvider(File.Open(Output, FileMode.OpenOrCreate)));
                }
                catch (IOException e)
                {
                    Program.Error(e.Message, false);
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