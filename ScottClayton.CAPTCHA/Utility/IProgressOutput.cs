namespace ScottClayton.CAPTCHA.Utility
{
    public interface IProgressOutput
    {
        void Write(string text);

        void Backspace(int characters);
    }
}