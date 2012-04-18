using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ScottClayton.Image
{
    /// <summary>
    /// The L*ab color space
    /// http://www.easyrgb.com/index.php?X=MATH&H=07#text7
    /// </summary>
    public struct LAB // : IColor<LAB>
    {
        public double L { get; set; }
        public double a { get; set; }
        public double b { get; set; }
        
        //private XYZ GetXYZ(Color c)
        //{
        //    // Adapted from http://www.easyrgb.com/index.php?X=MATH&H=07#text7

        //    double var_R = (c.R / 255.0);
        //    double var_G = (c.G / 255.0);
        //    double var_B = (c.B / 255.0);

        //    if (var_R > 0.04045) var_R = Math.Pow(((var_R + 0.055) / 1.055), 2.4);
        //    else var_R = var_R / 12.92;
        //    if (var_G > 0.04045) var_G = Math.Pow(((var_G + 0.055) / 1.055), 2.4);
        //    else var_G = var_G / 12.92;
        //    if (var_B > 0.04045) var_B = Math.Pow(((var_B + 0.055) / 1.055), 2.4);
        //    else var_B = var_B / 12.92;

        //    var_R = var_R * 100;
        //    var_G = var_G * 100;
        //    var_B = var_B * 100;

        //    XYZ xyz = new XYZ();
        //    xyz.X = var_R * 0.4124 + var_G * 0.3576 + var_B * 0.1805;
        //    xyz.Y = var_R * 0.2126 + var_G * 0.7152 + var_B * 0.0722;
        //    xyz.Z = var_R * 0.0193 + var_G * 0.1192 + var_B * 0.9505;

        //    return xyz;
        //}

        //private LAB GetLAB(XYZ c)
        //{
        //    // Adapted from http://www.easyrgb.com/index.php?X=MATH&H=07#text7

        //    double var_X = c.X / 95.047;
        //    double var_Y = c.Y / 100.000;
        //    double var_Z = c.Z / 108.883;

        //    if (var_X > 0.008856) var_X = Math.Pow(var_X, (1.0 / 3));
        //    else var_X = (7.787 * var_X) + (16.0 / 116);
        //    if (var_Y > 0.008856) var_Y = Math.Pow(var_Y, (1.0 / 3));
        //    else var_Y = (7.787 * var_Y) + (16.0 / 116);
        //    if (var_Z > 0.008856) var_Z = Math.Pow(var_Z, (1.0 / 3));
        //    else var_Z = (7.787 * var_Z) + (16.0 / 116);

        //    LAB lab = new LAB();
        //    lab.L = (116 * var_Y) - 16;
        //    lab.a = 500 * (var_X - var_Y);
        //    lab.b = 200 * (var_Y - var_Z);

        //    return lab;
        //}
        
        //private LAB GetLAB(Color c)
        //{
        //    return c.GetXYZ().GetLAB();
        //}

        //public Color ToRGB()
        //{
        //    // TODO
        //}

        //public S ToOther<S>()
        //    where S : IColor<S>, new()
        //{
        //    return new S().FromRGB(ToRGB());
        //}

        //public LAB FromRGB(Color color)
        //{
        //    return GetLAB(color);
        //}

        //public LAB FromOther<S>(S color)
        //    where S : IColor<S>, new()
        //{
        //    // TODO
        //}
    }

    /// <summary>
    /// The XYZ color space
    /// http://www.easyrgb.com/index.php?X=MATH&H=07#text7
    /// </summary>
    public struct XYZ
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }
}
