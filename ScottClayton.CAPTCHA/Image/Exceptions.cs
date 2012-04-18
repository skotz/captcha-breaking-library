using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScottClayton.Image
{
    /// <summary>
    /// Exception raised when you try to segment an image without defining how to segment by subscribing to the OnBeforeSegmentation event.
    /// </summary>
    public class SegmentationEventNotSubscribedToException : Exception
    {
        public SegmentationEventNotSubscribedToException()
            : base("Segmentation failed because OnBeforeSegmentation was not subscribed to. You must define how to segment each image.")
        {
        }

        public SegmentationEventNotSubscribedToException(string message)
            : base(message)
        {
        }

        public SegmentationEventNotSubscribedToException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception raised when there is some problem in the preprocessing phase of image segmentation.
    /// </summary>
    public class ImageProcessingException : Exception
    {
        public ImageProcessingException()
            : base("Failed to perform some operation on an Image.")
        {
        }

        public ImageProcessingException(string message)
            : base(message)
        {
        }

        public ImageProcessingException(string message, Exception innerException)
            : base(message + " (See inner exception for more detail.)", innerException)
        {
        }
    }

    /// <summary>
    /// A useless class for the sake of a lame joke...
    /// </summary>
    class Cookie : Exception { }
}
