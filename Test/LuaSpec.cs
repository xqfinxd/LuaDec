using System;

namespace Test
{
    public class LuaSpec
    {
        public enum NumberFormat
        {
            Default,
            Float,
            Int32,
            Int64
        }

        private bool isDefault;
        private int minorVersion;
        private NumberFormat numberFormat;
        private bool strip;
        private int version;

        public string ID
        {
            get
            {
                string id = "lua";
                id += version.ToString("X");
                id += GetMinorVersionString();
                return id;
            }
        }

        public LuaSpec()
        {
            this.isDefault = true;
            this.version = 0;
            this.numberFormat = NumberFormat.Default;
            this.strip = false;
        }

        public LuaSpec(int version) : this(version, -1)
        {
        }

        public LuaSpec(int version, int minorVersion)
        {
            this.isDefault = false;
            this.version = version;
            this.minorVersion = minorVersion;
            this.numberFormat = NumberFormat.Default;
            this.strip = false;
        }

        private string GetMinorVersionString()
        {
            if (minorVersion >= 0)
            {
                return minorVersion.ToString("X");
            }
            else
            {
                return "";
            }
        }

        private string GetNumberFormatString()
        {
            switch (numberFormat)
            {
                case NumberFormat.Default:
                    return "";

                case NumberFormat.Float:
                    return "_float";

                case NumberFormat.Int32:
                    return "_int32";

                case NumberFormat.Int64:
                    return "_int64";

                default:
                    throw new System.InvalidOperationException();
            }
        }

        private string GetVersionString()
        {
            if (isDefault)
            {
                return "";
            }
            else
            {
                return version.ToString("X");
            }
        }

        public bool Compatible(TestFile testfile)
        {
            if (testfile.version == TestFile.DEFAULT_VERSION)
            {
                return Compatible(testfile.name);
            }
            else
            {
                return this.version >= testfile.version;
            }
        }

        public bool Compatible(string filename)
        {
            int version = 0;
            int underscore = filename.IndexOf('_');
            if (underscore != -1)
            {
                string prefix = filename.Substring(0, underscore);
                try
                {
                    version = int.Parse(prefix, System.Globalization.NumberStyles.HexNumber);
                }
                catch (FormatException) { }
            }
            return version == 0 || this.version >= version;
        }

        public string[] GetArgs()
        {
            if (strip)
            {
                return new string[] { "-s" };
            }
            else
            {
                return new string[] { };
            }
        }

        public string GetLuaCName()
        {
            return "luac" + GetVersionString() + GetMinorVersionString() + GetNumberFormatString();
            //return "luac";
        }

        public void SetNumberFormat(NumberFormat format)
        {
            this.numberFormat = format;
        }

        public void SetStrip(bool strip)
        {
            this.strip = strip;
        }
    }
}