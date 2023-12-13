using LuaDec;
using System;
using System.IO;

namespace Test
{
    public class TestSuite
    {
        private TestFile[] files;
        private string name;
        private string path;

        public string Ext => ".lua";
        public string WorkDir => ".\\working\\";

        public TestSuite(string name, string path, TestFile[] files)
        {
            this.name = name;
            this.path = path;
            this.files = files;
        }

        private string CompiledFile(string file)
        {
            return Path.Combine(WorkDir, file + ".compile.bin");
        }
        private string DecompiledFile(string file)
        {
            return Path.Combine(WorkDir, file + ".decompile.lua");
        }
        private string RecompiledFile(string file)
        {
            return Path.Combine(WorkDir, file + ".recompile.bin");
        }

        private Configuration Configure(TestFile testfile, Configuration config)
        {
            Configuration modified = null;
            if (testfile.GetFlag(TestFile.RELAXED_SCOPE) && config.StrictScope)
            {
                modified = Extend(config, modified);
                modified.StrictScope = false;
            }
            return modified != null ? modified : config;
        }

        private Configuration Extend(Configuration basic, Configuration modified)
        {
            return modified != null ? modified : new Configuration(basic);
        }

        private TestResult Test(LuaSpec spec, LuaDecSpec uspec, string file, Configuration config, bool outputTrace)
        {
            var fn = Path.GetFileNameWithoutExtension(file);
            string compiledFile = CompiledFile(fn);
            string decompiledFile = DecompiledFile(fn);
            string recompiledFile = RecompiledFile(fn);

            try
            {
                LuaC.Compile(spec, file, compiledFile);
            }
            catch (Exception)
            {
                return TestResult.Skipped;
            }

            try
            {
                uspec.Run(compiledFile, decompiledFile, config);
                if (!uspec.disassemble)
                {
                    LuaC.Compile(spec, decompiledFile, recompiledFile);
                }
                else
                {
                    Program.Assemble(decompiledFile, recompiledFile);
                }
                Compare compare;
                if (!uspec.disassemble)
                {
                    compare = new Compare(Compare.Mode.Normal);
                }
                else
                {
                    compare = new Compare(Compare.Mode.Full);
                }
                return compare.ByteCodeEqual(compiledFile, recompiledFile) ? TestResult.Ok : TestResult.Failed;
            }
            catch (Exception e)
            {
                if (outputTrace)
                {
                    Console.WriteLine(e.StackTrace);
                }
                return TestResult.Failed;
            }
        }

        private TestResult TestC(LuaSpec spec, LuaDecSpec uspec, string file, Configuration config, bool outputTrace)
        {
            string decompiledFile = DecompiledFile(file);
            string recompiledFile = RecompiledFile(file);

            try
            {
                uspec.Run(file, decompiledFile, config);
                LuaC.Compile(spec, decompiledFile, recompiledFile);
                Compare compare = new Compare(Compare.Mode.Normal);
                return compare.ByteCodeEqual(file, recompiledFile) ? TestResult.Ok : TestResult.Failed;
            }
            catch (Exception e)
            {
                if (outputTrace)
                {
                    Console.WriteLine(e.StackTrace);
                }
                return TestResult.Failed;
            }
        }

        public bool Run(LuaSpec spec, LuaDecSpec uspec, TestReport report, Configuration basic)
        {
            int failed = 0;
            if (!Directory.Exists(WorkDir))
            {
                Directory.CreateDirectory(WorkDir);
            }
            foreach (TestFile testfile in files)
            {
                string name = testfile.name;
                if (spec.Compatible(name))
                {
                    Configuration config = Configure(testfile, basic);
                    TestResult result = Test(spec, uspec, path + name + Ext, config, false);
                    report.Result(TestName(spec, name), result);
                    switch (result)
                    {
                        case TestResult.Ok:
                            Console.Write(".");
                            break;

                        case TestResult.Skipped:
                            Console.Write(",");
                            break;

                        default:
                            Console.Write("!");
                            failed++;
                            break;
                    }
                }
            }
            return failed == 0;
        }

        public bool Run(LuaSpec spec, LuaDecSpec uspec, string file, bool compiled, Configuration config)
        {
            int passed = 0;
            int skipped = 0;
            int failed = 0;
            if (!Directory.Exists(WorkDir))
            {
                Directory.CreateDirectory(WorkDir);
            }

            {
                string name = file;
                string full;
                if (!file.Contains("/"))
                {
                    full = path + name + Ext;
                }
                else
                {
                    full = name;
                }
                TestResult result;
                if (!compiled)
                {
                    result = Test(spec, uspec, full, config, true);
                }
                else
                {
                    result = TestC(spec, uspec, full, config, true);
                }
                switch (result)
                {
                    case TestResult.Ok:
                        Console.WriteLine("Passed: " + name);
                        passed++;
                        break;

                    case TestResult.Skipped:
                        Console.WriteLine("Skipped: " + name);
                        skipped++;
                        break;

                    default:
                        Console.WriteLine("Failed: " + name);
                        failed++;
                        break;
                }
            }
            if (failed == 0 && skipped == 0)
            {
                Console.WriteLine(spec.GetLuaCName() + ": All tests passed!");
            }
            else
            {
                Console.WriteLine(spec.GetLuaCName() + ": Failed " + failed + " of " + (failed + passed) + " tests, skipped " + skipped + " tests.");
            }
            return failed == 0;
        }

        public string TestName(LuaSpec spec, string file)
        {
            if (name == null)
            {
                return spec.ID + ": " + file;
            }
            else
            {
                return spec.ID + ": " + name + "/" + file.Replace('\\', '/');
            }
        }
    }
}