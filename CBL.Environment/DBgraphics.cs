using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

class DBGraphics
{
    private Graphics g;
    /// <summary>
    /// The Graphics object that you can draw on.
    /// </summary>
    public Graphics drawing;
    public Bitmap bit;
    private Panel p;
    private int width;
    private int height;

    /// <summary>
    /// Creates a new instance of an automatic double buffering graphics object.
    /// </summary>
    /// <param name="p">The panel to draw on.</param>
    public DBGraphics(ref Panel p)
    {
        this.p = p;
        g = p.CreateGraphics();
        this.height = p.Height;
        this.width = p.Width;
        bit = new Bitmap(width, height);
        drawing = Graphics.FromImage(bit);
    }

    /// <summary>
    /// Clear the drawing screen.
    /// </summary>
    public void clear()
    {
        drawing.Clear(Color.White);
    }

    /// <summary>
    /// Clear the drawing screen.
    /// </summary>
    /// <param name="letter">Color to fill the cleared screen with.</param>
    public void clear(Color c)
    {
        drawing.Clear(c);
    }

    /// <summary>
    /// Update the screen with all drawing changes since last clear().
    /// </summary>
    public void sync()
    {
        try
        {
            g.DrawImage(bit, 0, 0);
        }
        catch { }
    }

    /// <summary>
    /// Get the current dimentions of the Panel, Graphics objects, and Bitmap.
    /// </summary>
    /// <returns>A Point containing the (width, height).</returns>
    public Point GetDimentions()
    {
        return new Point(width, height);
    }

    /// <summary>
    /// Resizes the panel, graphics objects, and bitmap.
    /// </summary>
    /// <param name="newX">The new width of the objects.</param>
    /// <param name="newY">The new height of the objects.</param>
    public void resize(int newX, int newY)
    {
        p.Width = newX;
        p.Height = newY;
        g = p.CreateGraphics();
        this.height = p.Height;
        this.width = p.Width;
        bit = new Bitmap(width, height);
        drawing = Graphics.FromImage(bit);
    }
}
