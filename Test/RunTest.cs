using LuaDec;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Test
{
    [TestClass]
    public class RunTest
    {
        [TestMethod]
        [DataRow("close10")]
        public void Test(string file)
        {
            LuaSpec spec = new LuaSpec(0x52, 4);
            LuaDecSpec uspec = new LuaDecSpec();
            //uspec.disassemble = true;
            Configuration config = new Configuration();
            config.StrictScope = true;
            Assert.IsTrue(TestFiles.Suite.Run(spec, uspec, file, false, config));
        }
    }
}