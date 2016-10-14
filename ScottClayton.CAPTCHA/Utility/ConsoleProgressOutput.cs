using System;

namespace ScottClayton.CAPTCHA.Utility
{
    internal class ConsoleProgressOutput : IProgressOutput
    {
        public void Backspace(int characters)
        {
            for (int i = 0; i < characters; i++)
            {
                Console.Write("\b");
            }
        }

        public void Write(string text)
        {
            Console.Write(text);
        }
    }
}