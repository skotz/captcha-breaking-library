using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ScottClayton.Image
{
    interface IColor<T>
    {
        Color ToRGB();

        S ToOther<S>();

        T FromRGB(Color color);

        T FromOther<S>(S color)
            where S : IColor<S>, new();
    }

    interface IColorComparer<T>
        where T : IColor<T>
    {
        double Compare(T left, T right);

        double Compare(T left, Color right);

        double Compare(T left, byte[] right);


        double Compare(Color left, T right);

        double Compare(Color left, Color right);

        double Compare(Color left, byte[] right);


        double Compare(byte[] left, T right);
        
        double Compare(byte[] left, Color right);

        double Compare(byte[] left, byte[] right);
    }
}
