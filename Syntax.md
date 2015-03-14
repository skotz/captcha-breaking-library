# CBL Syntax Documentation #

Here is the basic documentation for each CBL (CAPTCHA Breaking Language) command currently available within the scripting language (there is more you can access when using the .NET library directly).

The plan is to have a programming guide complete with examples and a program structure guide in the near future, but that is dependent on outside forces at the moment.




<table><tr><td><font size='4'><i>Setup</font></td></i><td><b>These are the functions that are placed towards the top of CBL scripts. They set up the methods that will be used to segment and untimately solve the CAPTCHA images.</b><i></td></tr></i><tr><td width='25'> </td><td width='200px'><a href='Syntax#ENDPRECONDITIONS.md'>ENDPRECONDITIONS</a></td><td>End the preconditioning loop block. </td></tr>
<tr><td width='25'> </td><td width='200px'><a href='Syntax#SETMODE.md'>SETMODE</a></td><td>Set the level of debugger output to the screen when the script is run in a console. </td></tr>
<tr><td width='25'> </td><td width='200px'><a href='Syntax#SETUPSOLVER.md'>SETUPSOLVER</a></td><td>Set up the solver which is responsible for determining what letter an individual picture represents. </td></tr>
<tr><td width='25'> </td><td width='200px'><a href='Syntax#DEFINEPRECONDITIONS.md'>DEFINEPRECONDITIONS</a></td><td>Start the preconditioning loop block ("loop" because it's run for each image being processed). </td></tr>
<tr><td width='25'> </td><td width='200px'><a href='Syntax#SETUPSEGMENTER.md'>SETUPSEGMENTER</a></td><td>Set up the segmenter which is responsible for extracting individual letters from an image after preprocessing. </td></tr>
</table>


<table><tr><td><font size='4'><i>Preprocess</font></td></i><td><b>These are the functions that are placed between the <code>DEFINEPRECONDITIONS</code> and <code>ENDPRECONDITIONS</code> commands. They are run for each image in the set to precondition the image before trying to segment out individual letters.</b><i></td></tr></i><tr><td width='25'> </td><td width='200px'><a href='Syntax#RESIZE.md'>RESIZE</a></td><td>Resize the image to a specified width and height. </td></tr>
<tr><td width='25'> </td><td width='200px'><a href='Syntax#ERODE.md'>ERODE</a></td><td>Erodes the edges of blobs within an image. </td></tr>
<tr><td width='25'> </td><td width='200px'><a href='Syntax#GROW.md'>GROW</a></td><td>Grow the size of all blobs in the image by one pixel. </td></tr>
<tr><td width='25'> </td><td width='200px'><a href='Syntax#OUTLINE.md'>OUTLINE</a></td><td>Performs a convolutional filter on the image that outlines edges. </td></tr>
<tr><td width='25'> </td><td width='200px'><a href='Syntax#SUBTRACT.md'>SUBTRACT</a></td><td>Perform a pixel-by-pixel subtraction of a given image from the working image and set each pixel value as the difference between the two. </td></tr>
<tr><td width='25'> </td><td width='200px'><a href='Syntax#MEDIAN.md'>MEDIAN</a></td><td>Perform a convolutional median filter on the image. </td></tr>
<tr><td width='25'> </td><td width='200px'><a href='Syntax#INVERT.md'>INVERT</a></td><td>Invert the colors in the image. </td></tr>
<tr><td width='25'> </td><td width='200px'><a href='Syntax#CROP.md'>CROP</a></td><td>Crop the image. </td></tr>
<tr><td width='25'> </td><td width='200px'><a href='Syntax#BILATERALSMOOTH.md'>BILATERALSMOOTH</a></td><td>Performs a bilateral smoothing (edge preserving smoothing) and noise reduction filter on an image. </td></tr>
<tr><td width='25'> </td><td width='200px'><a href='Syntax#COLORFILLBLOBS.md'>COLORFILLBLOBS</a></td><td>Fill each unique blob in an image with a random color. A group of adjacent pixels is considered a single blob when they are all similar to each other in the L<code>*</code>a<code>*</code>b<code>*</code> color space below a given threshold. In the L<code>*</code>a<code>*</code>b<code>*</code> color space, a threshold of 2.3 is considered to be a change "just noticible to the human eye." </td></tr>
<tr><td width='25'> </td><td width='200px'><a href='Syntax#REMOVESMALLBLOBS.md'>REMOVESMALLBLOBS</a></td><td>Remove blobs (by filling them with the background color) from an image that are too small. </td></tr>
<tr><td width='25'> </td><td width='200px'><a href='Syntax#BLACKANDWHITE.md'>BLACKANDWHITE</a></td><td>Convert the image to black and white, where anything not white turns black (even the color #FEFEFE). If you need to choose the threshold yourself, then see BINARIZE. </td></tr>
<tr><td width='25'> </td><td width='200px'><a href='Syntax#BINARIZE.md'>BINARIZE</a></td><td>Convert the image to black and white, where anything above a certain threshold is turned white. </td></tr>
<tr><td width='25'> </td><td width='200px'><a href='Syntax#REMOVENONCOLOR.md'>REMOVENONCOLOR</a></td><td>White out all pixels that are not a color (any shade of grey). (Useful when a CAPTCHA only colors the letters and not the background.) </td></tr>
<tr><td width='25'> </td><td width='200px'><a href='Syntax#KEEPONLYMAINCOLOR.md'>KEEPONLYMAINCOLOR</a></td><td>Finds the color that occurrs most often in the image and removes all other colors that are not the most common color.  This is great if the main CAPTCHA text is all one color and that text always represents the most common color in the image (in which case this function single-handedly segments the letters from the background). </td></tr>
<tr><td width='25'> </td><td width='200px'><a href='Syntax#SAVESAMPLE.md'>SAVESAMPLE</a></td><td>Save a sample of the working image for debugging purposes. This is helpful when writing a script, as you can see every step along the way if you wish. </td></tr>
<tr><td width='25'> </td><td width='200px'><a href='Syntax#MEANSHIFT.md'>MEANSHIFT</a></td><td>Apply a mean shift filter to the image. This will effectively flatten out color groups within a certain tolerance. </td></tr>
<tr><td width='25'> </td><td width='200px'><a href='Syntax#FILLWHITE.md'>FILLWHITE</a></td><td>Fill a color into a region of an image. </td></tr>
<tr><td width='25'> </td><td width='200px'><a href='Syntax#CONVOLUTE.md'>CONVOLUTE</a></td><td>Perform a convolutional filter on the image. </td></tr>
<tr><td width='25'> </td><td width='200px'><a href='Syntax#HISTOGRAMROTATE.md'>HISTOGRAMROTATE</a></td><td>Rotate an image using trial and error until a best angle is found (measured by a vertical histogram).  Use this when an image has slanted letters and you want them to be right side up. </td></tr>
<tr><td width='25'> </td><td width='200px'><a href='Syntax#WAIT.md'>WAIT</a></td><td>Wait for a key press from the user to continue. </td></tr>
</table>


<table><tr><td><font size='4'><i>Working</font></td></i><td><b>These are the functions that are used towards the end of CBL scripts. Most of the functions in this group are used temporarily while developing, testing, or measuring the effectiveness of the script.</b><i></td></tr></i><tr><td width='25'> </td><td width='200px'><a href='Syntax#TESTSEGMENT.md'>TESTSEGMENT</a></td><td>Test the preprocessing and segmentation setup on a test image and save the segmented parts to a folder. </td></tr>
<tr><td width='25'> </td><td width='200px'><a href='Syntax#TRAIN.md'>TRAIN</a></td><td>Train the solver on the patterns acquired or loaded. </td></tr>
<tr><td width='25'> </td><td width='200px'><a href='Syntax#TEST.md'>TEST</a></td><td>Test the solver's ability to produce correct predictions on the patterns acquired or loaded. (Use patterns that were not used in training or you will get skewed results.) </td></tr>
<tr><td width='25'> </td><td width='200px'><a href='Syntax#FULLTEST.md'>FULLTEST</a></td><td>Perform a full test (completely solving a CAPTCHA) and give the actual percentage of CAPTCHAs that were completely and correctly solved. </td></tr>
<tr><td width='25'> </td><td width='200px'><a href='Syntax#SOLVE.md'>SOLVE</a></td><td>Solve a given image using the logic developed and trained for in the CBL script and output the solution. </td></tr>
<tr><td width='25'> </td><td width='200px'><a href='Syntax#SAVE.md'>SAVE</a></td><td>Save the DataBase of trained patterns to a file so that it can be loaded later. The idea is to distribute the database file with your finished script (the finished script shouldn't do any training, only efficient solving). </td></tr>
<tr><td width='25'> </td><td width='200px'><a href='Syntax#LOAD.md'>LOAD</a></td><td>Load a pattern database. The database you load needs to have been saved under the same setup conditions as the script is being loaded under. </td></tr>
<tr><td width='25'> </td><td width='200px'><a href='Syntax#SAY.md'>SAY</a></td><td>Print out a line of debug text to the console. </td></tr>
<tr><td width='25'> </td><td width='200px'><a href='Syntax#WAIT.md'>WAIT</a></td><td>Wait for the user to press a key. </td></tr>
</table>



---

# _Setup_ Section #
These are the functions that are placed towards the top of CBL scripts. They set up the methods that will be used to segment and untimately solve the CAPTCHA images.


---

## DEFINEPRECONDITIONS ##
Start the preconditioning loop block ("loop" because it's run for each image being processed).
```cobol
DEFINEPRECONDITIONS```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Start the preconditioning loop block.</i></td></tr>
</table>


---

## ENDPRECONDITIONS ##
End the preconditioning loop block.
```cobol
ENDPRECONDITIONS```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>End the preconditioning loop block.</i></td></tr>
</table>


---

## SETMODE ##
Set the level of debugger output to the screen when the script is run in a console.
```cobol
SETMODE, WARN```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Set the level of debugger output to the screen when the script is run in a console.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <i><code>WARN</code></i> </th><th> <i>Literal Value</i> </th><th> Only output error or warning messages. </th></thead><tbody>
</td></tr></table></tbody></table>

```cobol
SETMODE, QUIET```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Set the level of debugger output to the screen when the script is run in a console.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <i><code>QUIET</code></i> </th><th> <i>Literal Value</i> </th><th> Do not print any information to the screen unless something fatal happened. </th></thead><tbody>
</td></tr></table></tbody></table>

```cobol
SETMODE, ALL```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Set the level of debugger output to the screen when the script is run in a console.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <i><code>ALL</code></i> </th><th> <i>Literal Value</i> </th><th> Output all messages including errors, warnings, and normal informational messages.  </th></thead><tbody>
</td></tr></table></tbody></table>


---

## SETUPSEGMENTER ##
Set up the segmenter which is responsible for extracting individual letters from an image after preprocessing.
```cobol
SETUPSEGMENTER, BLOB, MinWidth, MinHeight```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Use the blob segmenter, where each extracted image is represented as a separate, uniquely colored block of pixels in the source image.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <i><code>BLOB</code></i> </th><th> <i>Literal Value</i> </th><th> Use the blob segmenter to extract individual symbols. </th></thead><tbody>
<tr><td> <code>MinWidth</code> </td><td> Whole Number </td><td> The minimum width a blob must be to be considered a blob worthy of extraction. </td></tr>
<tr><td> <code>MinHeight</code> </td><td> Whole Number </td><td> The minimum height a blob must be to be considered a blob worthy of extraction. </td></tr>
</td></tr></table></tbody></table>

```cobol
SETUPSEGMENTER, BLOB, MinWidth, MinHeight, NumBlobs```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Use the blob segmenter, where each extracted image is represented as a separate, uniquely colored block of pixels in the source image.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <i><code>BLOB</code></i> </th><th> <i>Literal Value</i> </th><th> Use the blob segmenter to extract individual symbols. </th></thead><tbody>
<tr><td> <code>MinWidth</code> </td><td> Whole Number </td><td> The minimum width a blob must be to be considered a blob worthy of extraction. </td></tr>
<tr><td> <code>MinHeight</code> </td><td> Whole Number </td><td> The minimum height a blob must be to be considered a blob worthy of extraction. </td></tr>
<tr><td> <code>NumBlobs</code> </td><td> Whole Number </td><td> The fixed number of blobs to extract from the image. If fewer than this number are found, then the largest blobs will be split up until there are this many blobs. If there are too many, then the smallest will be ignored. </td></tr>
</td></tr></table></tbody></table>

```cobol
SETUPSEGMENTER, HIST, Tolerance```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Use histograms to determine where the best place in the image is to slice between letters.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <i><code>HIST</code></i> </th><th> <i>Literal Value</i> </th><th> Use histograms to divide up the image. </th></thead><tbody>
<tr><td> <code>Tolerance</code> </td><td> Whole Number </td><td> Any number of non-background pixels below this number (on any given vertical slice of the image) will be considered a valid split point. </td></tr>
</td></tr></table></tbody></table>

```cobol
SETUPSEGMENTER, HIST, Tolerance, NumberOfChars```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Use histograms to determine where the best place in the image is to slice between letters.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <i><code>HIST</code></i> </th><th> <i>Literal Value</i> </th><th> Use histograms to divide up the image. </th></thead><tbody>
<tr><td> <code>Tolerance</code> </td><td> Whole Number </td><td> Any number of non-background pixels below this number (on any given vertical slice of the image) will be considered a valid split point. </td></tr>
<tr><td> <code>NumberOfChars</code> </td><td> Whole Number </td><td> The number of characters you expect to have extracted from the image. If there are more than this, then the least likely matches will be discarded. If there are fewer than this, then the largest ones will be subdivided. </td></tr>
</td></tr></table></tbody></table>


---

## SETUPSOLVER ##
Set up the solver which is responsible for determining what letter an individual picture represents.
```cobol
SETUPSOLVER, SNN, CharacterSet, Width, Height```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Set up the solver to use a fully connected, backpropagation neural network.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <i><code>SNN</code></i> </th><th> <i>Literal Value</i> </th><th> Set up the solver to use a Simple Neural Network (fully connected with backpropagation error correction). </th></thead><tbody>
<tr><td> <code>CharacterSet</code> </td><td> Quoted String </td><td> A string containing all possible characters that could be used in the CAPTCHA system. </td></tr>
<tr><td> <code>Width</code> </td><td> Whole Number </td><td> The width that will be used for each input image. </td></tr>
<tr><td> <code>Height</code> </td><td> Whole Number </td><td> The height that will be used for each input image. </td></tr>
</td></tr></table></tbody></table>

```cobol
SETUPSOLVER, SNN, CharacterSet, Width, Height, Characters```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Set up the solver to use a fully connected, backpropagation neural network.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <i><code>SNN</code></i> </th><th> <i>Literal Value</i> </th><th> Set up the solver to use a Simple Neural Network (fully connected with backpropagation error correction). </th></thead><tbody>
<tr><td> <code>CharacterSet</code> </td><td> Quoted String </td><td> A string containing all possible characters that could be used in the CAPTCHA system. </td></tr>
<tr><td> <code>Width</code> </td><td> Whole Number </td><td> The width that will be used for each input image. </td></tr>
<tr><td> <code>Height</code> </td><td> Whole Number </td><td> The height that will be used for each input image. </td></tr>
<tr><td> <code>Characters</code> </td><td> Whole Number </td><td> The fixed number of characters that are in every CAPTCHA for this system. If the number of characters varies, then use a different overload of this function. </td></tr>
</td></tr></table></tbody></table>

```cobol
SETUPSOLVER, SNN, CharacterSet, Width, Height, HiddenNeurons, Characters```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Set up the solver to use a fully connected, backpropagation neural network.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <i><code>SNN</code></i> </th><th> <i>Literal Value</i> </th><th> Set up the solver to use a Simple Neural Network (fully connected with backpropagation error correction). </th></thead><tbody>
<tr><td> <code>CharacterSet</code> </td><td> Quoted String </td><td> A string containing all possible characters that could be used in the CAPTCHA system. </td></tr>
<tr><td> <code>Width</code> </td><td> Whole Number </td><td> The width that will be used for each input image. </td></tr>
<tr><td> <code>Height</code> </td><td> Whole Number </td><td> The height that will be used for each input image. </td></tr>
<tr><td> <code>HiddenNeurons</code> </td><td> Whole Number </td><td> The number of neurons to put in the middle (hidden) layer of the neural network. </td></tr>
<tr><td> <code>Characters</code> </td><td> Whole Number </td><td> The fixed number of characters that are in every CAPTCHA for this system. If the number of characters varies, then use a different overload of this function. </td></tr>
</td></tr></table></tbody></table>

```cobol
SETUPSOLVER, SNN, CharacterSet, Width, Height, HiddenNeurons, Characters, LearnRate```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Set up the solver to use a fully connected, backpropagation neural network.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <i><code>SNN</code></i> </th><th> <i>Literal Value</i> </th><th> Set up the solver to use a Simple Neural Network (fully connected with backpropagation error correction). </th></thead><tbody>
<tr><td> <code>CharacterSet</code> </td><td> Quoted String </td><td> A string containing all possible characters that could be used in the CAPTCHA system. </td></tr>
<tr><td> <code>Width</code> </td><td> Whole Number </td><td> The width that will be used for each input image. </td></tr>
<tr><td> <code>Height</code> </td><td> Whole Number </td><td> The height that will be used for each input image. </td></tr>
<tr><td> <code>HiddenNeurons</code> </td><td> Whole Number </td><td> The number of neurons to put in the middle (hidden) layer of the neural network. </td></tr>
<tr><td> <code>Characters</code> </td><td> Whole Number </td><td> The fixed number of characters that are in every CAPTCHA for this system. If the number of characters varies, then use a different overload of this function. </td></tr>
<tr><td> <code>LearnRate</code> </td><td> Decimal Value </td><td> The learning rate of descent for training the neural network. The value should be between 0.0 and 1.0, however anything below 0.9 will descend way too quickly. </td></tr>
</td></tr></table></tbody></table>

```cobol
SETUPSOLVER, MNN, CharacterSet, Width, Height```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Set up the solver to use a set of neural networks (one per pattern instead of one for all patterns).</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <i><code>MNN</code></i> </th><th> <i>Literal Value</i> </th><th> Set up the solver to use a set of neural networks (one per pattern instead of one for all patterns). </th></thead><tbody>
<tr><td> <code>CharacterSet</code> </td><td> Quoted String </td><td> A string containing all possible characters that could be used in the CAPTCHA system. </td></tr>
<tr><td> <code>Width</code> </td><td> Whole Number </td><td> The width that will be used for each input image. </td></tr>
<tr><td> <code>Height</code> </td><td> Whole Number </td><td> The height that will be used for each input image. </td></tr>
</td></tr></table></tbody></table>

```cobol
SETUPSOLVER, MNN, CharacterSet, Width, Height, Characters```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Set up the solver to use a set of neural networks (one per pattern instead of one for all patterns).</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <i><code>MNN</code></i> </th><th> <i>Literal Value</i> </th><th> Set up the solver to use a set of neural networks (one per pattern instead of one for all patterns). </th></thead><tbody>
<tr><td> <code>CharacterSet</code> </td><td> Quoted String </td><td> A string containing all possible characters that could be used in the CAPTCHA system. </td></tr>
<tr><td> <code>Width</code> </td><td> Whole Number </td><td> The width that will be used for each input image. </td></tr>
<tr><td> <code>Height</code> </td><td> Whole Number </td><td> The height that will be used for each input image. </td></tr>
<tr><td> <code>Characters</code> </td><td> Whole Number </td><td> The fixed number of characters that are in every CAPTCHA for this system. If the number of characters varies, then use a different overload of this function. </td></tr>
</td></tr></table></tbody></table>

```cobol
SETUPSOLVER, MNN, CharacterSet, Width, Height, HiddenNeurons, Characters```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Set up the solver to use a set of neural networks (one per pattern instead of one for all patterns).</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <i><code>MNN</code></i> </th><th> <i>Literal Value</i> </th><th> Set up the solver to use a set of neural networks (one per pattern instead of one for all patterns). </th></thead><tbody>
<tr><td> <code>CharacterSet</code> </td><td> Quoted String </td><td> A string containing all possible characters that could be used in the CAPTCHA system. </td></tr>
<tr><td> <code>Width</code> </td><td> Whole Number </td><td> The width that will be used for each input image. </td></tr>
<tr><td> <code>Height</code> </td><td> Whole Number </td><td> The height that will be used for each input image. </td></tr>
<tr><td> <code>HiddenNeurons</code> </td><td> Whole Number </td><td> The number of neurons to put in the middle (hidden) layer of the neural network. </td></tr>
<tr><td> <code>Characters</code> </td><td> Whole Number </td><td> The fixed number of characters that are in every CAPTCHA for this system. If the number of characters varies, then use a different overload of this function. </td></tr>
</td></tr></table></tbody></table>

```cobol
SETUPSOLVER, MNN, CharacterSet, Width, Height, HiddenNeurons, Characters, LearnRate```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Set up the solver to use a set of neural networks (one per pattern instead of one for all patterns).</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <i><code>MNN</code></i> </th><th> <i>Literal Value</i> </th><th> Set up the solver to use a set of neural networks (one per pattern instead of one for all patterns). </th></thead><tbody>
<tr><td> <code>CharacterSet</code> </td><td> Quoted String </td><td> A string containing all possible characters that could be used in the CAPTCHA system. </td></tr>
<tr><td> <code>Width</code> </td><td> Whole Number </td><td> The width that will be used for each input image. </td></tr>
<tr><td> <code>Height</code> </td><td> Whole Number </td><td> The height that will be used for each input image. </td></tr>
<tr><td> <code>HiddenNeurons</code> </td><td> Whole Number </td><td> The number of neurons to put in the middle (hidden) layer of the neural network. </td></tr>
<tr><td> <code>Characters</code> </td><td> Whole Number </td><td> The fixed number of characters that are in every CAPTCHA for this system. If the number of characters varies, then use a different overload of this function. </td></tr>
<tr><td> <code>LearnRate</code> </td><td> Decimal Value </td><td> The learning rate of descent for training the neural network. The value should be between 0.0 and 1.0, however anything below 0.9 will descend way too quickly. </td></tr>
</td></tr></table></tbody></table>

```cobol
SETUPSOLVER, BVS, CharacterSet, Width, Height```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Set up the solver to use bitmap vector subtraction (which matches patterns by finding the root-mean-square distance between two images).</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <i><code>BVS</code></i> </th><th> <i>Literal Value</i> </th><th> Set up the solver to use bitmap vector subtraction (which matches patterns by finding the root-mean-square distance between two images). </th></thead><tbody>
<tr><td> <code>CharacterSet</code> </td><td> Quoted String </td><td> A string containing all possible characters that could be used in the CAPTCHA system. </td></tr>
<tr><td> <code>Width</code> </td><td> Whole Number </td><td> The width that will be used for each input image. </td></tr>
<tr><td> <code>Height</code> </td><td> Whole Number </td><td> The height that will be used for each input image. </td></tr>
</td></tr></table></tbody></table>

```cobol
SETUPSOLVER, BVS, CharacterSet, Width, Height, MergePatterns```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Set up the solver to use bitmap vector subtraction (which matches patterns by finding the root-mean-square distance between two images).</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <i><code>BVS</code></i> </th><th> <i>Literal Value</i> </th><th> Set up the solver to use bitmap vector subtraction (which matches patterns by finding the root-mean-square distance between two images). </th></thead><tbody>
<tr><td> <code>CharacterSet</code> </td><td> Quoted String </td><td> A string containing all possible characters that could be used in the CAPTCHA system. </td></tr>
<tr><td> <code>Width</code> </td><td> Whole Number </td><td> The width that will be used for each input image. </td></tr>
<tr><td> <code>Height</code> </td><td> Whole Number </td><td> The height that will be used for each input image. </td></tr>
<tr><td> <code>MergePatterns</code> </td><td> Boolean (Y/N) </td><td> Whether or not to group all patterns with the same solution. If you do not, then a separate pattern will be created for every input (not recommended usually) and it will take a lot of time and resources. </td></tr>
</td></tr></table></tbody></table>

```cobol
SETUPSOLVER, HS, CharacterSet, Width, Height```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Set up the solver to use a histogram solver that compares the histograms of patterns to samples.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <i><code>HS</code></i> </th><th> <i>Literal Value</i> </th><th> Set up the solver to use a histogram solver that compares the histograms of patterns to samples. </th></thead><tbody>
<tr><td> <code>CharacterSet</code> </td><td> Quoted String </td><td> A string containing all possible characters that could be used in the CAPTCHA system. </td></tr>
<tr><td> <code>Width</code> </td><td> Whole Number </td><td> The width that will be used for each input image. </td></tr>
<tr><td> <code>Height</code> </td><td> Whole Number </td><td> The height that will be used for each input image. </td></tr>
</td></tr></table></tbody></table>

```cobol
SETUPSOLVER, CV, CharacterSet, Width, Height```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Set up the solver to use contour vector analysis. Contour analysis has the advantage on being invariant to scale, rotation, and translation which makes it ideal for some (but not all) situations.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <i><code>CV</code></i> </th><th> <i>Literal Value</i> </th><th> Set up the solver to use contour vector analysis. </th></thead><tbody>
<tr><td> <code>CharacterSet</code> </td><td> Quoted String </td><td> A string containing all possible characters that could be used in the CAPTCHA system. </td></tr>
<tr><td> <code>Width</code> </td><td> Whole Number </td><td> The width that will be used for each input image. </td></tr>
<tr><td> <code>Height</code> </td><td> Whole Number </td><td> The height that will be used for each input image. </td></tr>
</td></tr></table></tbody></table>



---

# _Preprocess_ Section #
These are the functions that are placed between the `DEFINEPRECONDITIONS` and `ENDPRECONDITIONS` commands. They are run for each image in the set to precondition the image before trying to segment out individual letters.


---

## BILATERALSMOOTH ##
Performs a bilateral smoothing (edge preserving smoothing) and noise reduction filter on an image.
```cobol
BILATERALSMOOTH```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Perfrom an edge preserving smoothing algorithm.</i></td></tr>
</table>


---

## BINARIZE ##
Convert the image to black and white, where anything above a certain threshold is turned white.
```cobol
BINARIZE, Threshold```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Convert the image to black and white, where anything above a given threshold is turned white.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <code>Threshold</code> </th><th> Whole Number </th><th> A threshold value between 0 and 255 that determines what colors turn black and which turn white. </th></thead><tbody>
</td></tr></table></tbody></table>


---

## BLACKANDWHITE ##
Convert the image to black and white, where anything not white turns black (even the color #FEFEFE). If you need to choose the threshold yourself, then see BINARIZE.
```cobol
BLACKANDWHITE```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Flatten an image to black and white.</i></td></tr>
</table>


---

## COLORFILLBLOBS ##
Fill each unique blob in an image with a random color. A group of adjacent pixels is considered a single blob when they are all similar to each other in the L`*`a`*`b`*` color space below a given threshold. In the L`*`a`*`b`*` color space, a threshold of 2.3 is considered to be a change "just noticible to the human eye."
```cobol
COLORFILLBLOBS```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Fill all blobs within a 1.0 distance in the L<code>*</code>a<code>*</code>b<code>*</code> colorspace with a random color.</i></td></tr>
</table>

```cobol
COLORFILLBLOBS, ColorTolerance, BackgroundTolerance```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Fill all blobs within a given distance in the L<code>*</code>a<code>*</code>b<code>*</code> colorspace with a random color.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <code>ColorTolerance</code> </th><th> Decimal Value </th><th> The maximum Delta E difference between two (L<code>*</code>a<code>*</code>b<code>*</code>) colors to allow when filling a blob. I.E., the colors have to be at most this close together to be considered to be in the same blob. </th></thead><tbody>
<tr><td> <code>BackgroundTolerance</code> </td><td> Decimal Value </td><td> The maximum Delta E difference between a pixel (L<code>*</code>a<code>*</code>b<code>*</code>) and the background to allow when filling. </td></tr>
</td></tr></table></tbody></table>


---

## CONVOLUTE ##
Perform a convolutional filter on the image.
```cobol
CONVOLUTE, A1, A2, A3, B1, B2, B3, C1, C2, C3```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Perform a convolutional filter on the image with a 3x3 kernel.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <code>A1</code> </th><th> Whole Number </th><th> The upper-left value of the 3x3 kernel. </th></thead><tbody>
<tr><td> <code>A2</code> </td><td> Whole Number </td><td> The upper-middle value of the 3x3 kernel. </td></tr>
<tr><td> <code>A3</code> </td><td> Whole Number </td><td> The upper-right value of the 3x3 kernel. </td></tr>
<tr><td> <code>B1</code> </td><td> Whole Number </td><td> The middle-left value of the 3x3 kernel. </td></tr>
<tr><td> <code>B2</code> </td><td> Whole Number </td><td> The center value of the 3x3 kernel. </td></tr>
<tr><td> <code>B3</code> </td><td> Whole Number </td><td> The middle-right value of the 3x3 kernel. </td></tr>
<tr><td> <code>C1</code> </td><td> Whole Number </td><td> The lower-left value of the 3x3 kernel. </td></tr>
<tr><td> <code>C2</code> </td><td> Whole Number </td><td> The lower-middle value of the 3x3 kernel. </td></tr>
<tr><td> <code>C3</code> </td><td> Whole Number </td><td> The lower-right value of the 3x3 kernel. </td></tr>
</td></tr></table></tbody></table>


---

## CROP ##
Crop the image.
```cobol
CROP, X, Width, Height```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Crop the image to a given rectangle.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <code>X</code> </th><th> Whole Number </th><th> The left side of the rectangle. </th></thead><tbody>
<tr><td> <code>Width</code> </td><td> Whole Number </td><td> The width of the rectangle. </td></tr>
<tr><td> <code>Height</code> </td><td> Whole Number </td><td> The height of the rectangle. </td></tr>
</td></tr></table></tbody></table>


---

## ERODE ##
Erodes the edges of blobs within an image.
```cobol
ERODE```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Erode the edges of all blobs, where a blob is defined as any pixel grouping completely surrounded by White.</i></td></tr>
</table>

```cobol
ERODE, Times```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Erode the edges of all blobs, where a blob is defined as any pixel grouping completely surrounded by a given color.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <code>Times</code> </th><th> Whole Number </th><th> Number of times to erode the edges. </th></thead><tbody>
</td></tr></table></tbody></table>


---

## FILLWHITE ##
Fill a color into a region of an image.
```cobol
FILLWHITE, X, Y```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Fill the background color into a region of an image.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <code>X</code> </th><th> Whole Number </th><th> The X location of the region to start filling from. </th></thead><tbody>
<tr><td> <code>Y</code> </td><td> Whole Number </td><td> The Y location of the region to start filling from. </td></tr>
</td></tr></table></tbody></table>


---

## GROW ##
Grow the size of all blobs in the image by one pixel.
```cobol
GROW```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Grow the edges of all blobs, where a blob is defined as any pixel grouping completely surrounded by White.</i></td></tr>
</table>

```cobol
GROW, Times```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Grow the edges of all blobs, where a blob is defined as any pixel grouping completely surrounded by a given color.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <code>Times</code> </th><th> Whole Number </th><th> Number of times to grow the edges. </th></thead><tbody>
</td></tr></table></tbody></table>


---

## HISTOGRAMROTATE ##
Rotate an image using trial and error until a best angle is found (measured by a vertical histogram).  Use this when an image has slanted letters and you want them to be right side up.
```cobol
HISTOGRAMROTATE```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Rotate an image using trial and error until a best angle is found (measured by a vertical histogram).</i></td></tr>
</table>

```cobol
HISTOGRAMROTATE, TRUE```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Rotate an image using trial and error until a best angle is found (measured by a vertical histogram).</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <i><code>TRUE</code></i> </th><th> <i>Literal Value</i> </th><th> Overlay the resulting image with a completely useless (albeit cool to look at) histogram graph. </th></thead><tbody>
</td></tr></table></tbody></table>


---

## INVERT ##
Invert the colors in the image.
```cobol
INVERT```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Invert the colors in the image.</i></td></tr>
</table>


---

## KEEPONLYMAINCOLOR ##
Finds the color that occurrs most often in the image and removes all other colors that are not the most common color.  This is great if the main CAPTCHA text is all one color and that text always represents the most common color in the image (in which case this function single-handedly segments the letters from the background).
```cobol
KEEPONLYMAINCOLOR, Threshold```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Find the color that occurrs most often in the image within a certain threshold and remove all other colors that are not withing a given threshold from that color.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <code>Threshold</code> </th><th> Whole Number </th><th> The threshold value which determines how close a color has to be to be kept. </th></thead><tbody>
</td></tr></table></tbody></table>


---

## MEANSHIFT ##
Apply a mean shift filter to the image. This will effectively flatten out color groups within a certain tolerance.
```cobol
MEANSHIFT```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Apply a 1 iteration mean shift filter with a radius of 1 and a tolerance of 1.</i></td></tr>
</table>

```cobol
MEANSHIFT, Iterations, Radius, Tolerance```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Apply a mean shift filter a given number of times with a given radius and a given tolerance.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <code>Iterations</code> </th><th> Whole Number </th><th> The number of times to repeat the filter on the image. </th></thead><tbody>
<tr><td> <code>Radius</code> </td><td> Whole Number </td><td> The radius of the filter. </td></tr>
<tr><td> <code>Tolerance</code> </td><td> Decimal Value </td><td> The tolerance that determines how close in color pixels have to be if they are to be considered in the same group. </td></tr>
</td></tr></table></tbody></table>


---

## MEDIAN ##
Perform a convolutional median filter on the image.
```cobol
MEDIAN```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Perform a convolutional median filter on the image one time.</i></td></tr>
</table>

```cobol
MEDIAN, NumTimes```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Perform a convolutional median filter on the image several times.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <code>NumTimes</code> </th><th> Whole Number </th><th> The number of times to apply the Median filter to the image. </th></thead><tbody>
</td></tr></table></tbody></table>


---

## OUTLINE ##
Performs a convolutional filter on the image that outlines edges.
```cobol
OUTLINE```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Outline all edges in the image using a convolutional filter.</i></td></tr>
</table>


---

## REMOVENONCOLOR ##
White out all pixels that are not a color (any shade of grey). (Useful when a CAPTCHA only colors the letters and not the background.)
```cobol
REMOVENONCOLOR```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Remove all grayscale colors from the image leaving only colors.</i></td></tr>
</table>

```cobol
REMOVENONCOLOR, Distance```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Remove all colors withing a certain threshold of a shade of gray from the image leaving only colors.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <code>Distance</code> </th><th> Whole Number </th><th> The threshold value which determines how close a color has to be to gray to be removed. </th></thead><tbody>
</td></tr></table></tbody></table>


---

## REMOVESMALLBLOBS ##
Remove blobs (by filling them with the background color) from an image that are too small.
```cobol
REMOVESMALLBLOBS, MinPixelCount, MinWidth, MinHeight```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Remove blobs from an image that are too small by either pixel count or X and Y dimensions.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <code>MinPixelCount</code> </th><th> Whole Number </th><th> The smallest number of pixels a blob can be made of. </th></thead><tbody>
<tr><td> <code>MinWidth</code> </td><td> Whole Number </td><td> The smallest width a blob can be. </td></tr>
<tr><td> <code>MinHeight</code> </td><td> Whole Number </td><td> The smallest height a blob can be. </td></tr>
</td></tr></table></tbody></table>

```cobol
REMOVESMALLBLOBS, MinPixelCount, MinWidth, MinHeight, ColorTolerance```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Fill all blobs within a given distance in the L<code>*</code>a<code>*</code>b<code>*</code> colorspace with a random color.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <code>MinPixelCount</code> </th><th> Whole Number </th><th> The smallest number of pixels a blob can be made of. </th></thead><tbody>
<tr><td> <code>MinWidth</code> </td><td> Whole Number </td><td> The smallest width a blob can be. </td></tr>
<tr><td> <code>MinHeight</code> </td><td> Whole Number </td><td> The smallest height a blob can be. </td></tr>
<tr><td> <code>ColorTolerance</code> </td><td> Whole Number </td><td> The RGB tolerance in color when flood filling </td></tr>
</td></tr></table></tbody></table>


---

## RESIZE ##
Resize the image to a specified width and height.
```cobol
RESIZE, Width, Height```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Resize each image to a specified width and height.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <code>Width</code> </th><th> Whole Number </th><th> The width to resize image to. </th></thead><tbody>
<tr><td> <code>Height</code> </td><td> Whole Number </td><td> The height to resize image to. </td></tr>
</td></tr></table></tbody></table>


---

## SAVESAMPLE ##
Save a sample of the working image for debugging purposes. This is helpful when writing a script, as you can see every step along the way if you wish.
```cobol
SAVESAMPLE, FileLocation```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Save a sample of the working image.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <code>FileLocation</code> </th><th> Quoted String </th><th> The name and location of where to save the image to. </th></thead><tbody>
</td></tr></table></tbody></table>


---

## SUBTRACT ##
Perform a pixel-by-pixel subtraction of a given image from the working image and set each pixel value as the difference between the two.
```cobol
SUBTRACT, ImageLocation```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Subtract one image from another.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <code>ImageLocation</code> </th><th> Quoted String </th><th> The absolute or relative location to the image to subtract from the working image. </th></thead><tbody>
</td></tr></table></tbody></table>


---

## WAIT ##
Wait for a key press from the user to continue.
```cobol
WAIT```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Wait for a key press from the user to continue.</i></td></tr>
</table>



---

# _Working_ Section #
These are the functions that are used towards the end of CBL scripts. Most of the functions in this group are used temporarily while developing, testing, or measuring the effectiveness of the script.


---

## FULLTEST ##
Perform a full test (completely solving a CAPTCHA) and give the actual percentage of CAPTCHAs that were completely and correctly solved.
```cobol
FULLTEST, Folder, ReportFile```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Perform a full test (completely solving a CAPTCHA) and give the actual percentage of CAPTCHAs that were completely and correctly solved.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <code>Folder</code> </th><th> Quoted String </th><th> The folder that contains a collection of sample CAPTCHA images for testing. The CAPTCHA images need to labeled (named) with the correct solution to CAPTCHA so that there is something to compare the predicted output to and get a percentage correct. </th></thead><tbody>
<tr><td> <code>ReportFile</code> </td><td> Quoted String </td><td> The file to save the report to. </td></tr>
</td></tr></table></tbody></table>

```cobol
FULLTEST, Folder, ReportFile, ImageFilter```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Perform a full test (completely solving a CAPTCHA) and give the actual percentage of CAPTCHAs that were completely and correctly solved.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <code>Folder</code> </th><th> Quoted String </th><th> The folder that contains a collection of sample CAPTCHA images for testing. The CAPTCHA images need to labeled (named) with the correct solution to CAPTCHA so that there is something to compare the predicted output to and get a percentage correct. </th></thead><tbody>
<tr><td> <code>ReportFile</code> </td><td> Quoted String </td><td> The file to save the report to. </td></tr>
<tr><td> <code>ImageFilter</code> </td><td> Quoted String </td><td> The filter (e.g., <b>.bmp) to use to find images.</b></td></tr>
</td></tr></table></tbody></table>


---

## LOAD ##
Load a pattern database. The database you load needs to have been saved under the same setup conditions as the script is being loaded under.
```cobol
LOAD```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Load a pattern database from the default "captcha.db" file.</i></td></tr>
</table>

```cobol
LOAD, Location```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Load a pattern database from a specified file.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <code>Location</code> </th><th> Quoted String </th><th> The name of the pattern database file to load. </th></thead><tbody>
</td></tr></table></tbody></table>


---

## SAVE ##
Save the DataBase of trained patterns to a file so that it can be loaded later. The idea is to distribute the database file with your finished script (the finished script shouldn't do any training, only efficient solving).
```cobol
SAVE```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Save the DataBase of trained patterns to the default "captcha.db" file.</i></td></tr>
</table>

```cobol
SAVE, Location```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Save the DataBase of trained patterns to a given file.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <code>Location</code> </th><th> Quoted String </th><th> The file name to save the pattern database to. </th></thead><tbody>
</td></tr></table></tbody></table>


---

## SAY ##
Print out a line of debug text to the console.
```cobol
SAY, Text```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Print out a line of debug text to the console.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <code>Text</code> </th><th> Quoted String </th><th> The text to print. </th></thead><tbody>
</td></tr></table></tbody></table>


---

## SOLVE ##
Solve a given image using the logic developed and trained for in the CBL script and output the solution.
```cobol
SOLVE, ImageLocation```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Solve a CAPTCHA using the logic developed in the current CBL script.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <code>ImageLocation</code> </th><th> Quoted String </th><th> The image file to load and solve. </th></thead><tbody>
</td></tr></table></tbody></table>

```cobol
SOLVE, %IMAGE%```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Solve a CAPTCHA using the logic developed in the current CBL script.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <i><code>%IMAGE%</code></i> </th><th> <i>Literal Value</i> </th><th> This placeholder will be replaced with the first command line value when run from the command line or, if being run from the CBL-GUI script runner app, will be replaced with the image that was dragged and dropped or loaded by the GUI. </th></thead><tbody>
</td></tr></table></tbody></table>


---

## TEST ##
Test the solver's ability to produce correct predictions on the patterns acquired or loaded. (Use patterns that were not used in training or you will get skewed results.)
```cobol
TEST, Folder```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Test the solver's ability to produce correct predictions on the patterns acquired or loaded.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <code>Folder</code> </th><th> Quoted String </th><th> The folder that contains the set of labeled patterns to test on. (Use patterns that were not used in training or you will get skewed results.) </th></thead><tbody>
</td></tr></table></tbody></table>


---

## TESTSEGMENT ##
Test the preprocessing and segmentation setup on a test image and save the segmented parts to a folder.
```cobol
TESTSEGMENT, ImageLocation, OutputFolder```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Test the preprocessing and segmentation setup on a test image and save the segmented parts to a folder.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <code>ImageLocation</code> </th><th> Quoted String </th><th> The location of the image to test the segmentation on. </th></thead><tbody>
<tr><td> <code>OutputFolder</code> </td><td> Quoted String </td><td> The folder to output the segmented test symbols to. </td></tr>
</td></tr></table></tbody></table>


---

## TRAIN ##
Train the solver on the patterns acquired or loaded.
```cobol
TRAIN, Folder```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Start training on a folder of patterns that have already been segmented and labeled for training.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <code>Folder</code> </th><th> Quoted String </th><th> The folder that contains the generated testing set of labeled patterns. </th></thead><tbody>
</td></tr></table></tbody></table>

```cobol
TRAIN, Folder, Iterations```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Start training on a folder of patterns that have already been segmented and labeled for training.</i></td></tr>

<tr><td><b>Parameters</b></td></tr>
<tr><td><pre>   </pre></td><td>
<table><thead><th> <code>Folder</code> </th><th> Quoted String </th><th> The folder that contains the generated testing set of labeled patterns. </th></thead><tbody>
<tr><td> <code>Iterations</code> </td><td> Whole Number </td><td> Complete this many iterations of training on the given training set. </td></tr>
</td></tr></table></tbody></table>


---

## WAIT ##
Wait for the user to press a key.
```cobol
WAIT```

<table><tr><td><b>Description</b></td></tr>
<tr><td><pre>   </pre></td><td><i>Wait for the user to press a key.</i></td></tr>
</table>



---

Generated by SKOTDOC on Friday, July 06, 2012 at 7:36:55 PM