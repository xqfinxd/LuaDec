using System;

namespace LuaDec.Decompile
{
    public class Output
    {
        private class DefaultOutputProvide : IOutputProvider
        {
            public void WriteByte(byte b)
            {
                Console.Write(b);
            }

            public void WriteLine()
            {
                Console.WriteLine();
            }

            public void WriteString(string s)
            {
                Console.Write(s);
            }
        }

        private int indentLevel = 0;
        private IOutputProvider output;
        private int position = 0;

        public int IndentLevel { get => indentLevel; set => indentLevel = value; }

        public int Position => position;

        public int IndentWidth => 4;

        public Output() : this(new DefaultOutputProvide())
        {
        }

        public Output(IOutputProvider output)
        {
            this.output = output;
        }

        private void Start()
        {
            if (position == 0)
            {
                for (int i = indentLevel; i != 0; i--)
                {
                    output.WriteString(" ");
                    position++;
                }
            }
        }

        public void Dedent()
        {
            indentLevel -= IndentWidth;
        }

        public void Indent()
        {
            indentLevel += IndentWidth;
        }

        public void WriteByte(byte b)
        {
            Start();
            output.WriteByte(b);
            position += 1;
        }

        public void WriteLine()
        {
            Start();
            output.WriteLine();
            position = 0;
        }

        public void WriteLine(string s)
        {
            Write(s);
            WriteLine();
        }

        public void Write(string s)
        {
            Start();
            output.WriteString(s);
            position += s.Length;
        }
    }
}