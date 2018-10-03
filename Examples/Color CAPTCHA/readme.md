# CBLI Walkthrough
This guide will show you how to solve a CAPTCHA using the CBL scripting language and the CBL Interpreter (which may be downloaded from the releases page).
All of the scripts below can be found in the "Examples/Color CAPTCHA" folder.

## Segmentation
First of all, create a new script with the captcha suffix. Here's an example.

```
** Show all messages
SetMode, all

** Use the blob segmentation method
SetupSegmenter, BLOB, 15, 15, 6

** Use the bitmap vector subtraction classifier, set the character set to just numbers,
** and specify the dimentions to which each extracted character should be resized
SetupSolver, BVS, "0123456789", 20, 20, Y

** This block contains a set of image manipulation instructions for preprocessing the image
** so that it can be more easily segmented into individual characters
DefinePreconditions
    ** It helps to save a copy of the image after each operation for debugging
    SAVESAMPLE, "step1.bmp"
    REMOVENONCOLOR, 60
    SAVESAMPLE, "step2.bmp"
    MEDIAN, 1
    SAVESAMPLE, "step3.bmp"
    BINARIZE, 80
    SAVESAMPLE, "step4.bmp"
EndPreconditions

** Test the segmentation steps on an image and save the individual characters to a folder
TESTSEGMENT, "images/194224.bmp", "segmented"
```

You'll want to collect as many sample images from the CAPTCHA as you can and place them in an "images" folder right next to your script.
Each image should be named with the solution of that CAPTCHA (for training).

Run the segmentation script from the command line in training mode.

```
cbli -s colorcap.segment.captcha -t
```

Which should run and successfully output the segmented characters.

```
Mode set to ALL
Segmenter Setup Complete
Bitmap Vector Solver Setup Complete
Preconditions loaded
Image saved as "step1.bmp"
All grayscale colors removed from image
Image saved as "step2.bmp"
Median Filter Applied
Image saved as "step3.bmp"
Image binarized to black and white
Image saved as "step4.bmp"
Test segmentation of "images/194224.bmp" complete
```

![Segments](https://cdn.rawgit.com/skotz/captcha-breaking-library/62660e0f/Examples/Color%20CAPTCHA/readme/segments.png)

Continue to play around with the image manipulation methods until you're able to cleanly segment as many images as possible.

## Training

Once your happy with the segmentation quality, you can make a copy of your script and remove any TestSegment or SaveSample commands.
Also add a Train and Save command at the bottom.

```
** Show all messages
SetMode, all

** Use the blob segmentation method
SetupSegmenter, BLOB, 15, 15, 6

** Use the bitmap vector subtraction classifier, set the character set to just numbers,
** and specify the dimentions to which each extracted character should be resized
SetupSolver, BVS, "0123456789", 20, 20, Y

** This block contains a set of image manipulation instructions for preprocessing the image
** so that it can be more easily segmented into individual characters
DefinePreconditions
    REMOVENONCOLOR, 60
    MEDIAN, 1
    BINARIZE, 80
EndPreconditions

** Train the model on a folder full of images
TRAIN, "images"

** Run some tests to see just how accurate our model is
FULLTEST, "images", "fulltest.txt"

** Save the trained model to a database file
SAVE, "colorcap.db"
```

Run this new script to train your model.

```
cbli -s colorcap.train.captcha -t
```

This will create a folder named "trainers" within the images folder.
In the trainers folder there will be another folder for each character in your chosen character set.
Go through every single image and delete or move any image that is classified incorrectly.
For instance, if you see an image of a 3 in the 8's folder, delete it.
This will help the trainer learn only correct patterns.

Once you've cleaned up the samples, run the training script again.

```
cbli -s colorcap.train.captcha -t
```

Since the trainers folder already exists, it will not segment the images again but will instead use the existing images (which you've cleaned up).

Note that if you change any steps in the segmentation block, you'll need to delete the trainers folder to regenerate the segments based on the new logic.

The FullTest command will run the trained model (in our case a BVS classifier) on all of the images in the folder and give us an accuracy score.

```
[?] GUESS  - IMAGE
[+] 974018 - images\974018.bmp
[+] 292197 - images\292197.bmp
[+] 393186 - images\393186.bmp
[-] 987023 - images\987623.bmp
[-] 719000 - images\719043.bmp
[-] 996008 - images\996338.bmp

TOTAL CORRECT: 37 (37/107 = 34.58%)
```

In our case about 34% of the CAPTCHAs were correctly solved. 
You can also see what it guessed for each image to help you debug. 
In many cases the model might be off by just one character.

Note that typically you want to run FullTest against a folder of images you did not use in training.

The last command in our script saved the trained model to a file, so now we can create our final solver script!

## Solving

Duplicate the script again and rename it. 
Remove the Train, FullTest, and Save commands, and add Load and Solve commands like this.
You can also optionally set the mode to Quiet if you don't want verbose message output.

```
** This is the final solver script, so we can disable debug mode
SetMode, quiet
SetupSegmenter, BLOB, 15, 15, 6
SetupSolver, BVS, "0123456789", 20, 20, Y

** Load the trained classifier
Load, "colorcap.db"

DefinePreconditions
    REMOVENONCOLOR, 60
    MEDIAN, 1
    BINARIZE, 80
EndPreconditions

** Solve an image passed to the script via the command line or the GUI tool
SOLVE, %image%
```

Run the script to solve a specific CAPTCHA like so.

```
cbli -s colorcap.solve.captcha -i images/965057.bmp
```

And it returns it's best guess!

```
CAPTCHA Solution: 965057
```

You can also load this script with the CBL GUI.

![GUI](https://cdn.rawgit.com/skotz/captcha-breaking-library/62660e0f/Examples/Color%20CAPTCHA/readme/gui.png)
