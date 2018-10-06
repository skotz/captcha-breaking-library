using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScottClayton.CAPTCHA.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScottClayton.CAPTCHA.Tests
{
    [TestClass]
    public class ConsoleProgressTests
    {
        [TestMethod]
        public void TestConsoleProgress()
        {
            StringProgressOutput output = new StringProgressOutput();

            ConsoleProgress prog = new ConsoleProgress("Test", 1, output);
            Assert.AreEqual(output.ProgressOutput, "Test    0% -");

            prog.SetPercent(25);
            Assert.AreEqual(output.ProgressOutput, "Test ......  25% -");

            prog.SetPercent(50);
            Assert.AreEqual(output.ProgressOutput, "Test ............  50% -");
            
            prog.Stop();
            Assert.AreEqual(output.ProgressOutput, "Test ......................... 100%");
        }
    }
}
