using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScottClayton.Neural;
using System.IO;

namespace ScottClayton.CAPTCHA
{
    public abstract class Solver
    {
        public virtual int ExpectedWidth { get; protected set; }

        public virtual int ExpectedHeight { get; protected set; }

        public abstract List<string> CharacterSet { get; protected set; }

        public event Action<object, PatternResult> OnTrainingComplete;

        public event Action<object, PatternResult> OnTestingComplete;

        public event Action<object, OnTrainingProgressChangeEventArgs> OnTraininProgressChanged;

        public virtual void RaiseOnTrainingComplete(PatternResult result)
        {
            if (OnTrainingComplete != null)
            {
                OnTrainingComplete(this, result);
            }
        }

        public virtual void RaiseOnTestingComplete(PatternResult result)
        {
            if (OnTestingComplete != null)
            {
                OnTestingComplete(this, result);
            }
        }

        public virtual void RaiseOnTrainingProgressChanged(OnTrainingProgressChangeEventArgs result)
        {
            if (OnTraininProgressChanged != null)
            {
                OnTraininProgressChanged(this, result);
            }
        }

        public abstract string Solve(List<Pattern> patterns);

        public abstract PatternResult Train(List<Pattern> patterns, int iterations);

        public abstract void TrainAsync(List<Pattern> patterns, int iterations);

        public abstract PatternResult Test(List<Pattern> patterns);

        public abstract void TestAsync(List<Pattern> patterns);

        public virtual void Save(BinaryWriter w)
        {
            w.Write(ExpectedWidth);
            w.Write(ExpectedHeight);
            w.Write(CharacterSet.Aggregate((c, n) => c + n));
        }

        public virtual void Load(BinaryReader r)
        {
            ExpectedWidth = r.ReadInt32();
            ExpectedHeight = r.ReadInt32();
            CharacterSet = r.ReadString().ToCharArray().Select(c => c.ToString()).ToList();
        }
    }
}
