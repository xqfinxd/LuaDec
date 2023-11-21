using System.IO;

namespace LuaDec.Decompile
{
    public class FileOutputProvider : IOutputProvider
    {

        private readonly FileStream output;
        private readonly string eol;

        public FileOutputProvider(FileStream output)
        {
            this.output = output;
            eol = System.Environment.NewLine;
        }

        public void WriteString(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                int c = s[i];
                if (c < 0 || c > 255) throw new System.InvalidOperationException();
                WriteByte((byte)c);
            }
        }

        public void WriteByte(byte b)
        {
            try
            {
                output.WriteByte(b);
            }
            catch (IOException e)
            {
                throw e;
            }
        }

        public void WriteLine()
        {
            WriteString(eol);
        }

    }

}
