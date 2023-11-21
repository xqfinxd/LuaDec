using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile
{
    public class Output
    {
        private class DefaultOutputProvide : IOutputProvider
        {
            public void WriteString(string s)
            {
                Console.Write(s);
            }

            public void WriteByte(byte b)
            {
                Console.Write(b);
            }

            public void WriteLine()
            {
                Console.WriteLine();
            }
        }


        private IOutputProvider output;
        private int indentationLevel = 0;
        private int position = 0;

        public Output() : this(new DefaultOutputProvide())
        {

        }

        public Output(IOutputProvider output)
        {
            this.output = output;
        }

        public void indent()
        {
            indentationLevel += 2;
        }

        public void dedent()
        {
            indentationLevel -= 2;
        }

        public int getIndentationLevel()
        {
            return indentationLevel;
        }

        public int getPosition()
        {
            return position;
        }

        public void setIndentationLevel(int indentationLevel)
        {
            this.indentationLevel = indentationLevel;
        }

        private void start()
        {
            if (position == 0)
            {
                for (int i = indentationLevel; i != 0; i--)
                {
                    output.WriteString(" ");
                    position++;
                }
            }
        }

        public void print(string s)
        {
            start();
            for (int i = 0; i < s.Length; i++)
            {
                output.WriteByte((byte)s[i]);
            }
            position += s.Length;
        }

        public void print(byte b)
        {
            start();
            output.WriteByte(b);
            position += 1;
        }

        public void println()
        {
            start();
            output.WriteLine();
            position = 0;
        }

        public void println(string s)
        {
            print(s);
            println();
        }

    }

}
