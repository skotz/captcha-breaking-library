using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using ScottClayton.Image;

namespace ScottClayton.CAPTCHA.Utility
{
    /// <summary>
    /// Send a global message to anyone who subscribes to a static event and wants to get messages.
    /// This class kinda violates the whole encapsulation idea...
    /// </summary>
    public static class GlobalMessage
    {
        /// <summary>
        /// Whether or not to send global messages.
        /// </summary>
        public static bool ALLOW_MESSAGES = false;

        public delegate void MessageHandler(string message);
        
        /// <summary>
        /// Raised whenever a new message is sent
        /// </summary>
        public static event MessageHandler OnGlobalMessage;

        public delegate void BitmapMessageHandler(List<Bitmap> image, string tag);

        /// <summary>
        /// Raised whenever a class wants to send a Bitmap to everyone
        /// </summary>
        public static event BitmapMessageHandler OnGlobalBitmapMessage;

        /// <summary>
        /// Send a string to all subscribers
        /// </summary>
        /// <param name="message">The message</param>
        public static void SendMessage(string message)
        {
            if (ALLOW_MESSAGES && OnGlobalMessage != null)
            {
                OnGlobalMessage(message);
            }
        }

        /// <summary>
        /// Send a bitmap to all subscribers
        /// </summary>
        /// <param name="image">The image to send</param>
        /// <param name="tag">A value indicating what this message is for</param>
        public static void SendMessage(Bitmap image, string tag = "")
        {
            if (ALLOW_MESSAGES && OnGlobalBitmapMessage != null)
            {
                OnGlobalBitmapMessage(new List<Bitmap>() { image.CloneFull() }, tag);
            }
        }

        /// <summary>
        /// Send a bitmap list to all subscribers
        /// </summary>
        /// <param name="image">The images to send</param>
        /// <param name="tag">A value indicating what this message is for</param>
        public static void SendMessage(List<Bitmap> image, string tag = "")
        {
            if (ALLOW_MESSAGES && OnGlobalBitmapMessage != null)
            {
                OnGlobalBitmapMessage(image.Select(i => i.CloneFull()).ToList(), tag);
            }
        }

        /// <summary>
        /// Get a string representation of all the properties and values in an object for debugging.
        /// </summary>
        /// <typeparam name="T">The type to get a property listing from</typeparam>
        /// <param name="item">The item to get a property listing from</param>
        /// <param name="newline">The string to separate each property line with</param>
        /// <returns></returns>
        public static string GetPropertiesList<T>(this T item, string newline = "\n")
        {
            try
            {
                // It's ugly, but it's for testing so deal with it.
                return typeof(T).GetProperties().ToList().Select(i => i.Name + " = " + (i.GetValue(item, null) ?? "null")).Aggregate((c, n) => c + newline + n);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}
