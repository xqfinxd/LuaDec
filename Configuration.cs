﻿using LuaDec.Decompile;
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
        private string opmap;
        private string output;
        private bool rawString;
        private bool strictScope;
        private VariableMode variable;

        public OpMode Mode { get => mode; set => mode = value; }
        public string Opmap { get => opmap; set => opmap = value; }
        public string Output { get => output; set => output = value; }
        public bool RawString { get => rawString; set => rawString = value; }
        public bool StrictScope { get => strictScope; set => strictScope = value; }
        public VariableMode Variable { get => variable; set => variable = value; }

        public Configuration()
        {
            RawString = false;
            Mode = OpMode.Decompile;
            Variable = VariableMode.Default;
            StrictScope = false;
            Opmap = null;
            Output = null;
        }

        public Configuration(Configuration other)
        {
            RawString = other.RawString;
            Mode = other.Mode;
            Variable = other.Variable;
            StrictScope = other.StrictScope;
            Opmap = other.Opmap;
            Output = other.Output;
        }

        public Output getOutput()
        {
            if (Output != null)
            {
                try
                {
                    return new Output(new FileOutputProvider(File.Open(Output, FileMode.Open)));
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