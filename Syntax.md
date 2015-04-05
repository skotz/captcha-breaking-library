
# CBL Syntax Documentation 

Here is the basic documentation for each CBL (CAPTCHA Breaking Language) command currently available within the scripting language (there is more you can access when using the .NET library directly). 

The plan is to have a programming guide complete with examples and a program structure guide in the near future, but that is dependent on outside forces at the moment. 




##Setup
These are the functions that are placed towards the top of CBL scripts. They set up the methods that will be used to segment and untimately solve the CAPTCHA images.

Method | Description
------ | ------
[ENDPRECONDITIONS](#ENDPRECONDITIONS) | End the preconditioning loop block. 
[SETMODE](#SETMODE) | Set the level of debugger output to the screen when the script is run in a console. 
[SETUPSOLVER](#SETUPSOLVER) | Set up the solver which is responsible for determining what letter an individual picture represents. 
[DEFINEPRECONDITIONS](#DEFINEPRECONDITIONS) | Start the preconditioning loop block ("loop" because it's run for each image being processed). 
[SETUPSEGMENTER](#SETUPSEGMENTER) | Set up the segmenter which is responsible for extracting individual letters from an image after preprocessing. 


##Preprocess
These are the functions that are placed between the *DEFINEPRECONDITIONS* and *ENDPRECONDITIONS* commands. They are run for each image in the set to precondition the image before trying to segment out individual letters.

Method | Description
------ | ------
[RESIZE](#RESIZE) | Resize the image to a specified width and height. 
[ERODE](#ERODE) | Erodes the edges of blobs within an image. 
[GROW](#GROW) | Grow the size of all blobs in the image by one pixel. 
[OUTLINE](#OUTLINE) | Performs a convolutional filter on the image that outlines edges. 
[SUBTRACT](#SUBTRACT) | Perform a pixel-by-pixel subtraction of a given image from the working image and set each pixel value as the difference between the two. 
[MEDIAN](#MEDIAN) | Perform a convolutional median filter on the image. 
[INVERT](#INVERT) | Invert the colors in the image. 
[CROP](#CROP) | Crop the image. 
[BILATERALSMOOTH](#BILATERALSMOOTH) | Performs a bilateral smoothing (edge preserving smoothing) and noise reduction filter on an image. 
[COLORFILLBLOBS](#COLORFILLBLOBS) | Fill each unique blob in an image with a random color. A group of adjacent pixels is considered a single blob when they are all similar to each other in the L`*`a`*`b`*` color space below a given threshold. In the L`*`a`*`b`*` color space, a threshold of 2.3 is considered to be a change "just noticible to the human eye." 
[REMOVESMALLBLOBS](#REMOVESMALLBLOBS) | Remove blobs (by filling them with the background color) from an image that are too small. 
[BLACKANDWHITE](#BLACKANDWHITE) | Convert the image to black and white, where anything not white turns black (even the color #FEFEFE). If you need to choose the threshold yourself, then see BINARIZE. 
[BINARIZE](#BINARIZE) | Convert the image to black and white, where anything above a certain threshold is turned white. 
[REMOVENONCOLOR](#REMOVENONCOLOR) | White out all pixels that are not a color (any shade of grey). (Useful when a CAPTCHA only colors the letters and not the background.) 
[KEEPONLYMAINCOLOR](#KEEPONLYMAINCOLOR) | Finds the color that occurrs most often in the image and removes all other colors that are not the most common color.  This is great if the main CAPTCHA text is all one color and that text always represents the most common color in the image (in which case this function single-handedly segments the letters from the background). 
[SAVESAMPLE](#SAVESAMPLE) | Save a sample of the working image for debugging purposes. This is helpful when writing a script, as you can see every step along the way if you wish. 
[MEANSHIFT](#MEANSHIFT) | Apply a mean shift filter to the image. This will effectively flatten out color groups within a certain tolerance. 
[FILLWHITE](#FILLWHITE) | Fill a color into a region of an image. 
[CONVOLUTE](#CONVOLUTE) | Perform a convolutional filter on the image. 
[HISTOGRAMROTATE](#HISTOGRAMROTATE) | Rotate an image using trial and error until a best angle is found (measured by a vertical histogram).  Use this when an image has slanted letters and you want them to be right side up. 
[WAIT](#WAIT) | Wait for a key press from the user to continue. 


##Working
These are the functions that are used towards the end of CBL scripts. Most of the functions in this group are used temporarily while developing, testing, or measuring the effectiveness of the script.

Method | Description
------ | ------
[TESTSEGMENT](#TESTSEGMENT) | Test the preprocessing and segmentation setup on a test image and save the segmented parts to a folder. 
[TRAIN](#TRAIN) | Train the solver on the patterns acquired or loaded. 
[TEST](#TEST) | Test the solver's ability to produce correct predictions on the patterns acquired or loaded. (Use patterns that were not used in training or you will get skewed results.) 
[FULLTEST](#FULLTEST) | Perform a full test (completely solving a CAPTCHA) and give the actual percentage of CAPTCHAs that were completely and correctly solved. 
[SOLVE](#SOLVE) | Solve a given image using the logic developed and trained for in the CBL script and output the solution. 
[SAVE](#SAVE) | Save the DataBase of trained patterns to a file so that it can be loaded later. The idea is to distribute the database file with your finished script (the finished script shouldn't do any training, only efficient solving). 
[LOAD](#LOAD) | Load a pattern database. The database you load needs to have been saved under the same setup conditions as the script is being loaded under. 
[SAY](#SAY) | Print out a line of debug text to the console. 
[WAIT](#WAIT) | Wait for the user to press a key. 


----
# *Setup* Section
These are the functions that are placed towards the top of CBL scripts. They set up the methods that will be used to segment and untimately solve the CAPTCHA images.

----
## DEFINEPRECONDITIONS
Start the preconditioning loop block ("loop" because it's run for each image being processed). 

### Overload 1
    DEFINEPRECONDITIONS

#### Description
Start the preconditioning loop block.


----
## ENDPRECONDITIONS
End the preconditioning loop block. 

### Overload 1
    ENDPRECONDITIONS

#### Description
End the preconditioning loop block.


----
## SETMODE
Set the level of debugger output to the screen when the script is run in a console. 

### Overload 1
    SETMODE, WARN

#### Description
Set the level of debugger output to the screen when the script is run in a console.

#### Parameters
Name | Type | Description
----- | ----- | -----
*WARN* | *Literal Value* | Only output error or warning messages.



### Overload 2
    SETMODE, QUIET

#### Description
Set the level of debugger output to the screen when the script is run in a console.

#### Parameters
Name | Type | Description
----- | ----- | -----
*QUIET* | *Literal Value* | Do not print any information to the screen unless something fatal happened.



### Overload 3
    SETMODE, ALL

#### Description
Set the level of debugger output to the screen when the script is run in a console.

#### Parameters
Name | Type | Description
----- | ----- | -----
*ALL* | *Literal Value* | Output all messages including errors, warnings, and normal informational messages. 


----
## SETUPSEGMENTER
Set up the segmenter which is responsible for extracting individual letters from an image after preprocessing. 

### Overload 1
    SETUPSEGMENTER, BLOB, MinWidth, MinHeight

#### Description
Use the blob segmenter, where each extracted image is represented as a separate, uniquely colored block of pixels in the source image.

#### Parameters
Name | Type | Description
----- | ----- | -----
*BLOB* | *Literal Value* | Use the blob segmenter to extract individual symbols.
MinWidth | Whole Number | The minimum width a blob must be to be considered a blob worthy of extraction.
MinHeight | Whole Number | The minimum height a blob must be to be considered a blob worthy of extraction.



### Overload 2
    SETUPSEGMENTER, BLOB, MinWidth, MinHeight, NumBlobs

#### Description
Use the blob segmenter, where each extracted image is represented as a separate, uniquely colored block of pixels in the source image.

#### Parameters
Name | Type | Description
----- | ----- | -----
*BLOB* | *Literal Value* | Use the blob segmenter to extract individual symbols.
MinWidth | Whole Number | The minimum width a blob must be to be considered a blob worthy of extraction.
MinHeight | Whole Number | The minimum height a blob must be to be considered a blob worthy of extraction.
NumBlobs | Whole Number | The fixed number of blobs to extract from the image. If fewer than this number are found, then the largest blobs will be split up until there are this many blobs. If there are too many, then the smallest will be ignored.



### Overload 3
    SETUPSEGMENTER, HIST, Tolerance

#### Description
Use histograms to determine where the best place in the image is to slice between letters.

#### Parameters
Name | Type | Description
----- | ----- | -----
*HIST* | *Literal Value* | Use histograms to divide up the image.
Tolerance | Whole Number | Any number of non-background pixels below this number (on any given vertical slice of the image) will be considered a valid split point.



### Overload 4
    SETUPSEGMENTER, HIST, Tolerance, NumberOfChars

#### Description
Use histograms to determine where the best place in the image is to slice between letters.

#### Parameters
Name | Type | Description
----- | ----- | -----
*HIST* | *Literal Value* | Use histograms to divide up the image.
Tolerance | Whole Number | Any number of non-background pixels below this number (on any given vertical slice of the image) will be considered a valid split point.
NumberOfChars | Whole Number | The number of characters you expect to have extracted from the image. If there are more than this, then the least likely matches will be discarded. If there are fewer than this, then the largest ones will be subdivided.


----
## SETUPSOLVER
Set up the solver which is responsible for determining what letter an individual picture represents. 

### Overload 1
    SETUPSOLVER, SNN, CharacterSet, Width, Height

#### Description
Set up the solver to use a fully connected, backpropagation neural network.

#### Parameters
Name | Type | Description
----- | ----- | -----
*SNN* | *Literal Value* | Set up the solver to use a Simple Neural Network (fully connected with backpropagation error correction).
CharacterSet | Quoted String | A string containing all possible characters that could be used in the CAPTCHA system.
Width | Whole Number | The width that will be used for each input image.
Height | Whole Number | The height that will be used for each input image.



### Overload 2
    SETUPSOLVER, SNN, CharacterSet, Width, Height, Characters

#### Description
Set up the solver to use a fully connected, backpropagation neural network.

#### Parameters
Name | Type | Description
----- | ----- | -----
*SNN* | *Literal Value* | Set up the solver to use a Simple Neural Network (fully connected with backpropagation error correction).
CharacterSet | Quoted String | A string containing all possible characters that could be used in the CAPTCHA system.
Width | Whole Number | The width that will be used for each input image.
Height | Whole Number | The height that will be used for each input image.
Characters | Whole Number | The fixed number of characters that are in every CAPTCHA for this system. If the number of characters varies, then use a different overload of this function.



### Overload 3
    SETUPSOLVER, SNN, CharacterSet, Width, Height, HiddenNeurons, Characters

#### Description
Set up the solver to use a fully connected, backpropagation neural network.

#### Parameters
Name | Type | Description
----- | ----- | -----
*SNN* | *Literal Value* | Set up the solver to use a Simple Neural Network (fully connected with backpropagation error correction).
CharacterSet | Quoted String | A string containing all possible characters that could be used in the CAPTCHA system.
Width | Whole Number | The width that will be used for each input image.
Height | Whole Number | The height that will be used for each input image.
HiddenNeurons | Whole Number | The number of neurons to put in the middle (hidden) layer of the neural network.
Characters | Whole Number | The fixed number of characters that are in every CAPTCHA for this system. If the number of characters varies, then use a different overload of this function.



### Overload 4
    SETUPSOLVER, SNN, CharacterSet, Width, Height, HiddenNeurons, Characters, LearnRate

#### Description
Set up the solver to use a fully connected, backpropagation neural network.

#### Parameters
Name | Type | Description
----- | ----- | -----
*SNN* | *Literal Value* | Set up the solver to use a Simple Neural Network (fully connected with backpropagation error correction).
CharacterSet | Quoted String | A string containing all possible characters that could be used in the CAPTCHA system.
Width | Whole Number | The width that will be used for each input image.
Height | Whole Number | The height that will be used for each input image.
HiddenNeurons | Whole Number | The number of neurons to put in the middle (hidden) layer of the neural network.
Characters | Whole Number | The fixed number of characters that are in every CAPTCHA for this system. If the number of characters varies, then use a different overload of this function.
LearnRate | Decimal Value | The learning rate of descent for training the neural network. The value should be between 0.0 and 1.0, however anything below 0.9 will descend way too quickly.



### Overload 5
    SETUPSOLVER, MNN, CharacterSet, Width, Height

#### Description
Set up the solver to use a set of neural networks (one per pattern instead of one for all patterns).

#### Parameters
Name | Type | Description
----- | ----- | -----
*MNN* | *Literal Value* | Set up the solver to use a set of neural networks (one per pattern instead of one for all patterns).
CharacterSet | Quoted String | A string containing all possible characters that could be used in the CAPTCHA system.
Width | Whole Number | The width that will be used for each input image.
Height | Whole Number | The height that will be used for each input image.



### Overload 6
    SETUPSOLVER, MNN, CharacterSet, Width, Height, Characters

#### Description
Set up the solver to use a set of neural networks (one per pattern instead of one for all patterns).

#### Parameters
Name | Type | Description
----- | ----- | -----
*MNN* | *Literal Value* | Set up the solver to use a set of neural networks (one per pattern instead of one for all patterns).
CharacterSet | Quoted String | A string containing all possible characters that could be used in the CAPTCHA system.
Width | Whole Number | The width that will be used for each input image.
Height | Whole Number | The height that will be used for each input image.
Characters | Whole Number | The fixed number of characters that are in every CAPTCHA for this system. If the number of characters varies, then use a different overload of this function.



### Overload 7
    SETUPSOLVER, MNN, CharacterSet, Width, Height, HiddenNeurons, Characters

#### Description
Set up the solver to use a set of neural networks (one per pattern instead of one for all patterns).

#### Parameters
Name | Type | Description
----- | ----- | -----
*MNN* | *Literal Value* | Set up the solver to use a set of neural networks (one per pattern instead of one for all patterns).
CharacterSet | Quoted String | A string containing all possible characters that could be used in the CAPTCHA system.
Width | Whole Number | The width that will be used for each input image.
Height | Whole Number | The height that will be used for each input image.
HiddenNeurons | Whole Number | The number of neurons to put in the middle (hidden) layer of the neural network.
Characters | Whole Number | The fixed number of characters that are in every CAPTCHA for this system. If the number of characters varies, then use a different overload of this function.



### Overload 8
    SETUPSOLVER, MNN, CharacterSet, Width, Height, HiddenNeurons, Characters, LearnRate

#### Description
Set up the solver to use a set of neural networks (one per pattern instead of one for all patterns).

#### Parameters
Name | Type | Description
----- | ----- | -----
*MNN* | *Literal Value* | Set up the solver to use a set of neural networks (one per pattern instead of one for all patterns).
CharacterSet | Quoted String | A string containing all possible characters that could be used in the CAPTCHA system.
Width | Whole Number | The width that will be used for each input image.
Height | Whole Number | The height that will be used for each input image.
HiddenNeurons | Whole Number | The number of neurons to put in the middle (hidden) layer of the neural network.
Characters | Whole Number | The fixed number of characters that are in every CAPTCHA for this system. If the number of characters varies, then use a different overload of this function.
LearnRate | Decimal Value | The learning rate of descent for training the neural network. The value should be between 0.0 and 1.0, however anything below 0.9 will descend way too quickly.



### Overload 9
    SETUPSOLVER, BVS, CharacterSet, Width, Height

#### Description
Set up the solver to use bitmap vector subtraction (which matches patterns by finding the root-mean-square distance between two images).

#### Parameters
Name | Type | Description
----- | ----- | -----
*BVS* | *Literal Value* | Set up the solver to use bitmap vector subtraction (which matches patterns by finding the root-mean-square distance between two images).
CharacterSet | Quoted String | A string containing all possible characters that could be used in the CAPTCHA system.
Width | Whole Number | The width that will be used for each input image.
Height | Whole Number | The height that will be used for each input image.



### Overload 10
    SETUPSOLVER, BVS, CharacterSet, Width, Height, MergePatterns

#### Description
Set up the solver to use bitmap vector subtraction (which matches patterns by finding the root-mean-square distance between two images).

#### Parameters
Name | Type | Description
----- | ----- | -----
*BVS* | *Literal Value* | Set up the solver to use bitmap vector subtraction (which matches patterns by finding the root-mean-square distance between two images).
CharacterSet | Quoted String | A string containing all possible characters that could be used in the CAPTCHA system.
Width | Whole Number | The width that will be used for each input image.
Height | Whole Number | The height that will be used for each input image.
MergePatterns | Boolean (Y/N) | Whether or not to group all patterns with the same solution. If you do not, then a separate pattern will be created for every input (not recommended usually) and it will take a lot of time and resources.



### Overload 11
    SETUPSOLVER, HS, CharacterSet, Width, Height

#### Description
Set up the solver to use a histogram solver that compares the histograms of patterns to samples.

#### Parameters
Name | Type | Description
----- | ----- | -----
*HS* | *Literal Value* | Set up the solver to use a histogram solver that compares the histograms of patterns to samples.
CharacterSet | Quoted String | A string containing all possible characters that could be used in the CAPTCHA system.
Width | Whole Number | The width that will be used for each input image.
Height | Whole Number | The height that will be used for each input image.



### Overload 12
    SETUPSOLVER, CV, CharacterSet, Width, Height

#### Description
Set up the solver to use contour vector analysis. Contour analysis has the advantage on being invariant to scale, rotation, and translation which makes it ideal for some (but not all) situations.

#### Parameters
Name | Type | Description
----- | ----- | -----
*CV* | *Literal Value* | Set up the solver to use contour vector analysis.
CharacterSet | Quoted String | A string containing all possible characters that could be used in the CAPTCHA system.
Width | Whole Number | The width that will be used for each input image.
Height | Whole Number | The height that will be used for each input image.



----
# *Preprocess* Section
These are the functions that are placed between the *DEFINEPRECONDITIONS* and *ENDPRECONDITIONS* commands. They are run for each image in the set to precondition the image before trying to segment out individual letters.

----
## BILATERALSMOOTH
Performs a bilateral smoothing (edge preserving smoothing) and noise reduction filter on an image. 

### Overload 1
    BILATERALSMOOTH

#### Description
Perfrom an edge preserving smoothing algorithm.


----
## BINARIZE
Convert the image to black and white, where anything above a certain threshold is turned white. 

### Overload 1
    BINARIZE, Threshold

#### Description
Convert the image to black and white, where anything above a given threshold is turned white.

#### Parameters
Name | Type | Description
----- | ----- | -----
Threshold | Whole Number | A threshold value between 0 and 255 that determines what colors turn black and which turn white.


----
## BLACKANDWHITE
Convert the image to black and white, where anything not white turns black (even the color #FEFEFE). If you need to choose the threshold yourself, then see BINARIZE. 

### Overload 1
    BLACKANDWHITE

#### Description
Flatten an image to black and white.


----
## COLORFILLBLOBS
Fill each unique blob in an image with a random color. A group of adjacent pixels is considered a single blob when they are all similar to each other in the L`*`a`*`b`*` color space below a given threshold. In the L`*`a`*`b`*` color space, a threshold of 2.3 is considered to be a change "just noticible to the human eye." 

### Overload 1
    COLORFILLBLOBS

#### Description
Fill all blobs within a 1.0 distance in the L`*`a`*`b`*` colorspace with a random color.



### Overload 2
    COLORFILLBLOBS, ColorTolerance, BackgroundTolerance

#### Description
Fill all blobs within a given distance in the L`*`a`*`b`*` colorspace with a random color.

#### Parameters
Name | Type | Description
----- | ----- | -----
ColorTolerance | Decimal Value | The maximum Delta E difference between two (L`*`a`*`b`*`) colors to allow when filling a blob. I.E., the colors have to be at most this close together to be considered to be in the same blob.
BackgroundTolerance | Decimal Value | The maximum Delta E difference between a pixel (L`*`a`*`b`*`) and the background to allow when filling.


----
## CONVOLUTE
Perform a convolutional filter on the image. 

### Overload 1
    CONVOLUTE, A1, A2, A3, B1, B2, B3, C1, C2, C3

#### Description
Perform a convolutional filter on the image with a 3x3 kernel.

#### Parameters
Name | Type | Description
----- | ----- | -----
A1 | Whole Number | The upper-left value of the 3x3 kernel.
A2 | Whole Number | The upper-middle value of the 3x3 kernel.
A3 | Whole Number | The upper-right value of the 3x3 kernel.
B1 | Whole Number | The middle-left value of the 3x3 kernel.
B2 | Whole Number | The center value of the 3x3 kernel.
B3 | Whole Number | The middle-right value of the 3x3 kernel.
C1 | Whole Number | The lower-left value of the 3x3 kernel.
C2 | Whole Number | The lower-middle value of the 3x3 kernel.
C3 | Whole Number | The lower-right value of the 3x3 kernel.


----
## CROP
Crop the image. 

### Overload 1
    CROP, X, Width, Height

#### Description
Crop the image to a given rectangle.

#### Parameters
Name | Type | Description
----- | ----- | -----
X | Whole Number | The left side of the rectangle.
Width | Whole Number | The width of the rectangle.
Height | Whole Number | The height of the rectangle.


----
## ERODE
Erodes the edges of blobs within an image. 

### Overload 1
    ERODE

#### Description
Erode the edges of all blobs, where a blob is defined as any pixel grouping completely surrounded by White.



### Overload 2
    ERODE, Times

#### Description
Erode the edges of all blobs, where a blob is defined as any pixel grouping completely surrounded by a given color.

#### Parameters
Name | Type | Description
----- | ----- | -----
Times | Whole Number | Number of times to erode the edges.


----
## FILLWHITE
Fill a color into a region of an image. 

### Overload 1
    FILLWHITE, X, Y

#### Description
Fill the background color into a region of an image.

#### Parameters
Name | Type | Description
----- | ----- | -----
X | Whole Number | The X location of the region to start filling from.
Y | Whole Number | The Y location of the region to start filling from.


----
## GROW
Grow the size of all blobs in the image by one pixel. 

### Overload 1
    GROW

#### Description
Grow the edges of all blobs, where a blob is defined as any pixel grouping completely surrounded by White.



### Overload 2
    GROW, Times

#### Description
Grow the edges of all blobs, where a blob is defined as any pixel grouping completely surrounded by a given color.

#### Parameters
Name | Type | Description
----- | ----- | -----
Times | Whole Number | Number of times to grow the edges.


----
## HISTOGRAMROTATE
Rotate an image using trial and error until a best angle is found (measured by a vertical histogram).  Use this when an image has slanted letters and you want them to be right side up. 

### Overload 1
    HISTOGRAMROTATE

#### Description
Rotate an image using trial and error until a best angle is found (measured by a vertical histogram). 



### Overload 2
    HISTOGRAMROTATE, TRUE

#### Description
Rotate an image using trial and error until a best angle is found (measured by a vertical histogram). 

#### Parameters
Name | Type | Description
----- | ----- | -----
*TRUE* | *Literal Value* | Overlay the resulting image with a completely useless (albeit cool to look at) histogram graph.


----
## INVERT
Invert the colors in the image. 

### Overload 1
    INVERT

#### Description
Invert the colors in the image.


----
## KEEPONLYMAINCOLOR
Finds the color that occurrs most often in the image and removes all other colors that are not the most common color.  This is great if the main CAPTCHA text is all one color and that text always represents the most common color in the image (in which case this function single-handedly segments the letters from the background). 

### Overload 1
    KEEPONLYMAINCOLOR, Threshold

#### Description
Find the color that occurrs most often in the image within a certain threshold and remove all other colors that are not withing a given threshold from that color.

#### Parameters
Name | Type | Description
----- | ----- | -----
Threshold | Whole Number | The threshold value which determines how close a color has to be to be kept.


----
## MEANSHIFT
Apply a mean shift filter to the image. This will effectively flatten out color groups within a certain tolerance. 

### Overload 1
    MEANSHIFT

#### Description
Apply a 1 iteration mean shift filter with a radius of 1 and a tolerance of 1.



### Overload 2
    MEANSHIFT, Iterations, Radius, Tolerance

#### Description
Apply a mean shift filter a given number of times with a given radius and a given tolerance.

#### Parameters
Name | Type | Description
----- | ----- | -----
Iterations | Whole Number | The number of times to repeat the filter on the image.
Radius | Whole Number | The radius of the filter.
Tolerance | Decimal Value | The tolerance that determines how close in color pixels have to be if they are to be considered in the same group.


----
## MEDIAN
Perform a convolutional median filter on the image. 

### Overload 1
    MEDIAN

#### Description
Perform a convolutional median filter on the image one time.



### Overload 2
    MEDIAN, NumTimes

#### Description
Perform a convolutional median filter on the image several times.

#### Parameters
Name | Type | Description
----- | ----- | -----
NumTimes | Whole Number | The number of times to apply the Median filter to the image.


----
## OUTLINE
Performs a convolutional filter on the image that outlines edges. 

### Overload 1
    OUTLINE

#### Description
Outline all edges in the image using a convolutional filter.


----
## REMOVENONCOLOR
White out all pixels that are not a color (any shade of grey). (Useful when a CAPTCHA only colors the letters and not the background.) 

### Overload 1
    REMOVENONCOLOR

#### Description
Remove all grayscale colors from the image leaving only colors.



### Overload 2
    REMOVENONCOLOR, Distance

#### Description
Remove all colors withing a certain threshold of a shade of gray from the image leaving only colors.

#### Parameters
Name | Type | Description
----- | ----- | -----
Distance | Whole Number | The threshold value which determines how close a color has to be to gray to be removed.


----
## REMOVESMALLBLOBS
Remove blobs (by filling them with the background color) from an image that are too small. 

### Overload 1
    REMOVESMALLBLOBS, MinPixelCount, MinWidth, MinHeight

#### Description
Remove blobs from an image that are too small by either pixel count or X and Y dimensions.

#### Parameters
Name | Type | Description
----- | ----- | -----
MinPixelCount | Whole Number | The smallest number of pixels a blob can be made of.
MinWidth | Whole Number | The smallest width a blob can be.
MinHeight | Whole Number | The smallest height a blob can be.



### Overload 2
    REMOVESMALLBLOBS, MinPixelCount, MinWidth, MinHeight, ColorTolerance

#### Description
Fill all blobs within a given distance in the L`*`a`*`b`*` colorspace with a random color.

#### Parameters
Name | Type | Description
----- | ----- | -----
MinPixelCount | Whole Number | The smallest number of pixels a blob can be made of.
MinWidth | Whole Number | The smallest width a blob can be.
MinHeight | Whole Number | The smallest height a blob can be.
ColorTolerance | Whole Number | The RGB tolerance in color when flood filling


----
## RESIZE
Resize the image to a specified width and height. 

### Overload 1
    RESIZE, Width, Height

#### Description
Resize each image to a specified width and height.

#### Parameters
Name | Type | Description
----- | ----- | -----
Width | Whole Number | The width to resize image to.
Height | Whole Number | The height to resize image to.


----
## SAVESAMPLE
Save a sample of the working image for debugging purposes. This is helpful when writing a script, as you can see every step along the way if you wish. 

### Overload 1
    SAVESAMPLE, FileLocation

#### Description
Save a sample of the working image.

#### Parameters
Name | Type | Description
----- | ----- | -----
FileLocation | Quoted String | The name and location of where to save the image to.


----
## SUBTRACT
Perform a pixel-by-pixel subtraction of a given image from the working image and set each pixel value as the difference between the two. 

### Overload 1
    SUBTRACT, ImageLocation

#### Description
Subtract one image from another.

#### Parameters
Name | Type | Description
----- | ----- | -----
ImageLocation | Quoted String | The absolute or relative location to the image to subtract from the working image.



### Overload 2
    SUBTRACT, Base64, String

#### Description
Subtract one image from another.

#### Parameters
Name | Type | Description
----- | ----- | -----
*Base64* | *Literal Value* | Denotes that the image data is encoded and listed here in base64.
String | Quoted String | The Base64 string that contains the bitmap image data.


----
## WAIT
Wait for a key press from the user to continue. 

### Overload 1
    WAIT

#### Description
Wait for a key press from the user to continue.



----
# *Working* Section
These are the functions that are used towards the end of CBL scripts. Most of the functions in this group are used temporarily while developing, testing, or measuring the effectiveness of the script.

----
## FULLTEST
Perform a full test (completely solving a CAPTCHA) and give the actual percentage of CAPTCHAs that were completely and correctly solved. 

### Overload 1
    FULLTEST, Folder, ReportFile

#### Description
Perform a full test (completely solving a CAPTCHA) and give the actual percentage of CAPTCHAs that were completely and correctly solved.

#### Parameters
Name | Type | Description
----- | ----- | -----
Folder | Quoted String | The folder that contains a collection of sample CAPTCHA images for testing. The CAPTCHA images need to labeled (named) with the correct solution to CAPTCHA so that there is something to compare the predicted output to and get a percentage correct.
ReportFile | Quoted String | The file to save the report to.



### Overload 2
    FULLTEST, Folder, ReportFile, ImageFilter

#### Description
Perform a full test (completely solving a CAPTCHA) and give the actual percentage of CAPTCHAs that were completely and correctly solved.

#### Parameters
Name | Type | Description
----- | ----- | -----
Folder | Quoted String | The folder that contains a collection of sample CAPTCHA images for testing. The CAPTCHA images need to labeled (named) with the correct solution to CAPTCHA so that there is something to compare the predicted output to and get a percentage correct.
ReportFile | Quoted String | The file to save the report to.
ImageFilter | Quoted String | The filter (e.g., *.bmp) to use to find images.


----
## LOAD
Load a pattern database. The database you load needs to have been saved under the same setup conditions as the script is being loaded under. 

### Overload 1
    LOAD

#### Description
Load a pattern database from the default "captcha.db" file.



### Overload 2
    LOAD, Location

#### Description
Load a pattern database from a specified file.

#### Parameters
Name | Type | Description
----- | ----- | -----
Location | Quoted String | The name of the pattern database file to load.



### Overload 3
    LOAD, Base64, String

#### Description
Load a pattern database from a base64 representation of the database file.

#### Parameters
Name | Type | Description
----- | ----- | -----
*Base64* | *Literal Value* | Load the database file from a base64 encoded string.
String | Quoted String | The Base64 encoded database to load.


----
## SAVE
Save the DataBase of trained patterns to a file so that it can be loaded later. The idea is to distribute the database file with your finished script (the finished script shouldn't do any training, only efficient solving). 

### Overload 1
    SAVE

#### Description
Save the DataBase of trained patterns to the default "captcha.db" file.



### Overload 2
    SAVE, Location

#### Description
Save the DataBase of trained patterns to a given file.

#### Parameters
Name | Type | Description
----- | ----- | -----
Location | Quoted String | The file name to save the pattern database to.


----
## SAY
Print out a line of debug text to the console. 

### Overload 1
    SAY, Text

#### Description
Print out a line of debug text to the console.

#### Parameters
Name | Type | Description
----- | ----- | -----
Text | Quoted String | The text to print.


----
## SOLVE
Solve a given image using the logic developed and trained for in the CBL script and output the solution. 

### Overload 1
    SOLVE, ImageLocation

#### Description
Solve a CAPTCHA using the logic developed in the current CBL script.

#### Parameters
Name | Type | Description
----- | ----- | -----
ImageLocation | Quoted String | The image file to load and solve.



### Overload 2
    SOLVE, %IMAGE%

#### Description
Solve a CAPTCHA using the logic developed in the current CBL script.

#### Parameters
Name | Type | Description
----- | ----- | -----
*%IMAGE%* | *Literal Value* | This placeholder will be replaced with the first command line value when run from the command line or, if being run from the CBL-GUI script runner app, will be replaced with the image that was dragged and dropped or loaded by the GUI.


----
## TEST
Test the solver's ability to produce correct predictions on the patterns acquired or loaded. (Use patterns that were not used in training or you will get skewed results.) 

### Overload 1
    TEST, Folder

#### Description
Test the solver's ability to produce correct predictions on the patterns acquired or loaded.

#### Parameters
Name | Type | Description
----- | ----- | -----
Folder | Quoted String | The folder that contains the set of labeled patterns to test on. (Use patterns that were not used in training or you will get skewed results.)


----
## TESTSEGMENT
Test the preprocessing and segmentation setup on a test image and save the segmented parts to a folder. 

### Overload 1
    TESTSEGMENT, ImageLocation, OutputFolder

#### Description
Test the preprocessing and segmentation setup on a test image and save the segmented parts to a folder.

#### Parameters
Name | Type | Description
----- | ----- | -----
ImageLocation | Quoted String | The location of the image to test the segmentation on.
OutputFolder | Quoted String | The folder to output the segmented test symbols to.


----
## TRAIN
Train the solver on the patterns acquired or loaded. 

### Overload 1
    TRAIN, Folder

#### Description
Start training on a folder of patterns that have already been segmented and labeled for training.

#### Parameters
Name | Type | Description
----- | ----- | -----
Folder | Quoted String | The folder that contains the generated testing set of labeled patterns.



### Overload 2
    TRAIN, Folder, Iterations

#### Description
Start training on a folder of patterns that have already been segmented and labeled for training.

#### Parameters
Name | Type | Description
----- | ----- | -----
Folder | Quoted String | The folder that contains the generated testing set of labeled patterns.
Iterations | Whole Number | Complete this many iterations of training on the given training set.


----
## WAIT
Wait for the user to press a key. 

### Overload 1
    WAIT

#### Description
Wait for the user to press a key.



----
Generated by SKOTDOC on Sunday, April 05, 2015 at 3:15:59 PM
