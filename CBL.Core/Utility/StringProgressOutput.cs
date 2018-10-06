namespace ScottClayton.CAPTCHA.Utility
{
    public class StringProgressOutput : IProgressOutput
    {
        public string ProgressOutput { get; private set; }

        public StringProgressOutput()
        {
            ProgressOutput = "";
        }

        public void Backspace(int characters)
        {
            ProgressOutput = ProgressOutput.Substring(0, ProgressOutput.Length - characters);
        }

        public void Write(string text)
        {
            ProgressOutput += text;
        }
    }
}