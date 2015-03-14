# CAPTCHA Breaking Scripting Language #

## Introduction ##

The CAPTCHA Breaking Library and Scripting Language provides the necessary tools for quickly creating a program capable of reading text out of an image. The actual job of determining which letter is in a given image is done with the help of **Neural Networks**, **Contour Analysis**, and **Bitmap Vector Subtraction**.

CAPTCHAs that are able to be segmented by color (i.e., each letter is a different color) may first be converted to a **perceptive color space** where distances between colors are mathematically determined based on how the human eyes perceive color, not how colors are different in the RGB color-space. This allows most multicolor CAPTCHAs to be solved quite trivially.

When completed, the goal of the scripting language is to be simple enough to break most CAPTCHAs with under 25 lines of CBL (CAPTCHA Breaking Language) code.

I am beginning to document some of the [language syntax](Syntax.md). Later I intend to add examples, tips, and links to places where you can read up on the underlying technology so that I do not try to explain something that was already said better by someone else.

Another goal of mine is to provide a complete step-by-step tutorial on CAPTCHA breaking using CBL or using the C# CAPTCHA Breaking Library (my library upon which CBL runs).

## Example ##

Here is a code snippet written in CBL that breaks a CAPTCHA originally from [here](http://www.codeproject.com/Articles/5947/CAPTCHA-Imag):

![http://captcha-breaking-library.googlecode.com/svn/trunk/ART/42028351.png](http://captcha-breaking-library.googlecode.com/svn/trunk/ART/42028351.png)

```
**********************************************************
* Scott Clayton                           April 14, 2012 *
**********************************************************
* This script is part of the CBL interpreter:            *
* http://code.google.com/p/captcha-breaking-library/     *
**********************************************************
* The CAPTCHA that this script breaks came from:         *
* http://www.codeproject.com/Articles/5947/CAPTCHA-Image *
**********************************************************

SetMode,        all
SetupSegmenter, BLOB, 4, 14, 8
SetupSolver,    MNN, "0123456789", 20, 20, 8, 150, 0.95
Load,           "mnn.solver.db"

DefinePreconditions
   Resize,           400, 100
   Subtract,         "merge3.bmp"
   Invert
   Median,           1
   MeanShift,        1, 2, 5
   Binarize,         150
   ColorFillBlobs,   80, 52
   RemoveSmallBlobs, 90, 4, 14
   HistogramRotate
   Binarize,         200
   ColorFillBlobs
EndPreconditions

Solve, %IMAGE%
```

And here is the CBL GUI running the script you see above on a CAPTCHA:

![http://captcha-breaking-library.googlecode.com/svn/wiki/main-example-01.png](http://captcha-breaking-library.googlecode.com/svn/wiki/main-example-01.png)

![http://captcha-breaking-library.googlecode.com/svn/wiki/main-example-02.png](http://captcha-breaking-library.googlecode.com/svn/wiki/main-example-02.png)

## Hello World Quick Start ##

Want to know how to create the Hello World program in CBL? Here's a short tutorial to help you get familiar with writing and executing CBL code. Go to the HelloWorld tutorial.

## Notepad++ Plugin ##

There is now a [Notepad++](http://notepad-plus-plus.org/) syntax highlighting plugin on the downloads page to help you program in CBL. Installation instructions are included in the Readme.txt file in the download.


_Scott_