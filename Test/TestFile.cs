namespace Test
{
    public class TestFile
    {
        public static readonly int DEFAULT_VERSION = 0x50;
        public static readonly int RELAXED_SCOPE = 1;

        public readonly int flags;
        public readonly string name;
        public readonly int version;

        public TestFile(string name) : this(name, DEFAULT_VERSION, 0)
        {
        }

        public TestFile(string name, int version) : this(name, version, 0)
        {
        }

        public TestFile(string name, int version, int flags)
        {
            this.name = name;
            this.version = version;
            this.flags = flags;
        }

        public bool GetFlag(int flag)
        {
            return (flags & flag) == flag;
        }
    }
}