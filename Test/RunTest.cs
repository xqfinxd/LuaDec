using LuaDec;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Test
{
    [TestClass]
    public class RunTest
    {
        [TestMethod]
        [DataRow("52_goto02")]
        public void Test(string file)
        {
            LuaSpec spec = new LuaSpec(0x54, 6);
            LuaDecSpec uspec = new LuaDecSpec();
            //uspec.disassemble = true;
            Configuration config = new Configuration();
            config.StrictScope = true;
            Assert.IsTrue(TestFiles.Suite.Run(spec, uspec, file, false, config));
        }
    }
}