using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScottClayton.Neural;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScottClayton.CAPTCHA.Tests.Solvers
{
    [TestClass]
    public class BitmapSubtractionSolverTests
    {
        [TestMethod]
        public void TestBitmapSubtractionSolverTrainAndSolve()
        {
            List<Pattern> patterns = new List<Pattern>();
            patterns.Add(new Pattern(new double[] { 1, 1, 1, 1, 0, 1, 1, 0, 1 }, new double[] { 1, 0, 0 })); // a
            patterns.Add(new Pattern(new double[] { 1, 0, 0, 1, 1, 1, 1, 1, 1 }, new double[] { 0, 1, 0 })); // b
            patterns.Add(new Pattern(new double[] { 1, 1, 1, 1, 0, 0, 1, 1, 1 }, new double[] { 0, 0, 1 })); // c

            Pattern a = new Pattern(new double[] { 1, 1, 1, 1, 0, 1, 1, 0, 1 }, new double[] { 0, 0, 0 }); // exact "a"
            Pattern b = new Pattern(new double[] { 1, 0.2, 0, 1, 1, 0.5, 1, 1, 0.5 }, new double[] { 0, 0, 0 }); // close to a "b"
            Pattern c = new Pattern(new double[] { 0.5, 1, 1, 0.5, 0, 0, 1, 1, 0.5 }, new double[] { 0, 0, 0 }); // close to a "c"
            List<Pattern> solve = new List<Pattern>() { c, a, b, b, a };

            BitmapSubtractionSolver bss = new BitmapSubtractionSolver("abc", 3, 3);
            bss.Train(patterns, 1);
            
            Assert.AreEqual(bss.Solve(solve), "cabba");
        }

        [TestMethod]
        public void TestBitmapSubtractionSolverSerialization()
        {
            List<Pattern> patterns = new List<Pattern>();
            patterns.Add(new Pattern(new double[] { 1, 1, 1, 1, 0, 1, 1, 0, 1 }, new double[] { 1, 0, 0 })); // a
            patterns.Add(new Pattern(new double[] { 1, 0, 0, 1, 1, 1, 1, 1, 1 }, new double[] { 0, 1, 0 })); // b
            patterns.Add(new Pattern(new double[] { 1, 1, 1, 1, 0, 0, 1, 1, 1 }, new double[] { 0, 0, 1 })); // c

            Pattern a = new Pattern(new double[] { 1, 1, 1, 1, 0, 1, 1, 0, 1 }, new double[] { 0, 0, 0 }); // exact "a"
            Pattern b = new Pattern(new double[] { 1, 0.2, 0, 1, 1, 0.5, 1, 1, 0.5 }, new double[] { 0, 0, 0 }); // close to a "b"
            Pattern c = new Pattern(new double[] { 0.5, 1, 1, 0.5, 0, 0, 1, 1, 0.5 }, new double[] { 0, 0, 0 }); // close to a "c"
            List<Pattern> solve = new List<Pattern>() { c, a, b, b, a };

            BitmapSubtractionSolver bss = new BitmapSubtractionSolver("abc", 3, 3);
            bss.Train(patterns, 1);

            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bss.Save(bw);

                ms.Position = 0;
                using (BinaryReader br = new BinaryReader(ms))
                {
                    bss = new BitmapSubtractionSolver("abc", 3, 3);
                    bss.Load(br);

                    solve = new List<Pattern>() { a, b, c };

                    Assert.AreEqual(bss.Solve(solve), "abc");
                }
            }
        }
    }
}
