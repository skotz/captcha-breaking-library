using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScottClayton.Neural;

namespace ScottClayton.CAPTCHA
{
    public class SolutionSet
    {
        public List<Pattern> Patterns { get; private set; }

        protected SolutionSet(List<Pattern> patterns)
        {
            Patterns = patterns;
        }
    }

    class SolutionSetCreator : SolutionSet
    {
        public SolutionSetCreator(List<Pattern> patterns)
            : base(patterns)
        {
        }
    }
}
