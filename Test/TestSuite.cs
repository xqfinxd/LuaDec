using LuaDec;
using System;
using System.IO;

namespace Test
{
    public class TestSuite
    {
        private static string compiled = "luac.output";
        private static string decompiled = "LuaDec.output";
        private static string recompiled = "Test.output";
        private static string workDir = ".\\working\\";
        private string ext = ".lua";
        private TestFile[] files;
        private string name;
        private string path;

        public TestSuite(string name, string path, TestFile[] files)
        {
            this.name = name;
            this.path = path;
            this.files = files;
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

        private TestResult Test(LuaSpec spec, LuaDecSpec uspec, string file, Configuration config)
        {
            try
            {
                LuaC.Compile(spec, file, workDir + compiled);
            }
            catch (Exception)
            {
                return TestResult.Skipped;
            }

            try
            {
                uspec.Run(workDir + compiled, workDir + decompiled, config);
                if (!uspec.disassemble)
                {
                    LuaC.Compile(spec, workDir + decompiled, workDir + recompiled);
                }
                else
                {
                    Program.Assemble(workDir + decompiled, workDir + recompiled);
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
                return compare.ByteCodeEqual(workDir + compiled, workDir + recompiled) ? TestResult.Ok : TestResult.Failed;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return TestResult.Failed;
            }
        }

        private TestResult TestC(LuaSpec spec, LuaDecSpec uspec, string file, Configuration config)
        {
            try
            {
                uspec.Run(file, workDir + decompiled, config);
                LuaC.Compile(spec, workDir + decompiled, workDir + recompiled);
                Compare compare = new Compare(Compare.Mode.Normal);
                return compare.ByteCodeEqual(file, workDir + recompiled) ? TestResult.Ok : TestResult.Failed;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return TestResult.Failed;
            }
        }

        public bool Run(LuaSpec spec, LuaDecSpec uspec, TestReport report, Configuration basic)
        {
            int failed = 0;
            if (!Directory.Exists(workDir))
            {
                Directory.CreateDirectory(workDir);
            }
            foreach (TestFile testfile in files)
            {
                string name = testfile.name;
                if (spec.Compatible(name))
                {
                    Configuration config = Configure(testfile, basic);
                    TestResult result = Test(spec, uspec, path + name + ext, config);
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
            if (!Directory.Exists(workDir))
            {
                Directory.CreateDirectory(workDir);
            }

            {
                string name = file;
                string full;
                if (!file.Contains("/"))
                {
                    full = path + name + ext;
                }
                else
                {
                    full = name;
                }
                TestResult result;
                if (!compiled)
                {
                    result = Test(spec, uspec, full, config);
                }
                else
                {
                    result = TestC(spec, uspec, full, config);
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