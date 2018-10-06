using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

namespace ScottClayton.Image
{
    public abstract class SegmentMethod
    {
        public abstract List<Bitmap> Segment(Bitmap image);
        public abstract SegmentMethod Clone();
    }
}
