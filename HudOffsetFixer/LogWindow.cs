using System;
using System.Windows.Controls;

namespace HudOffsetFixer
{
    public class LogWindow
    {
        private static TextBox _textBox;
        public static void SetBox(TextBox textBox)
        {
            _textBox = textBox;
        }

        public static void LogMessage(string message)
        {
            _textBox.Text += $"Message: {message}{Environment.NewLine}";
        }

        public static void LogError(string message)
        {
            _textBox.Text += $"Error: {message}{Environment.NewLine}";
        }
    }
}
