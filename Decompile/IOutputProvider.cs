namespace LuaDec.Decompile
{
    public interface IOutputProvider
    {
        void WriteByte(byte b);
        void WriteLine();
        void WriteString(string s);
    }
}