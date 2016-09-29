using System;

using Windows.UI.Xaml.Controls;

namespace WindowsPhone_SaveThePass.Model
{
    static class HelperFunctions
    {
        public static ContentDialog FactoryDialog(string content, string primaryButtonText, string secondaryButtonText)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Content = content;
            dialog.PrimaryButtonText = primaryButtonText;
            dialog.SecondaryButtonText = secondaryButtonText;
            return dialog;
        }

        public static void Log(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        public static string Crop(this string word)
        {
            string cropped = "";

            string[] splited = word.Split(' ');

            for (int i = 0; i < splited.Length; i++)
            {
                cropped += splited[i];
            }

            return cropped;
        }

        public static string GeneratePassword()
        {
            Random random = new Random();
            const int ASCII_ZERO = 48;
            const int ASCII_NINE = 57;
            const int ASCII_A_LOWER = 65;
            const int ASCII_Z_LOWER = 90;
            const int ASCII_A_UPPER = 97;
            const int ASCII_Z_UPPER = 122;


            char[] assebledPassword = new char[10];

            for (int i = 0; i < assebledPassword.Length; i++)
            {
                if (i < 3)
                    assebledPassword[i] = (char)random.Next(ASCII_ZERO, ASCII_NINE);
                else if (i < 6)
                    assebledPassword[i] = (char)random.Next(ASCII_A_LOWER, ASCII_Z_LOWER);
                else if (i < 8)
                    assebledPassword[i] = (char)random.Next(ASCII_ZERO, ASCII_NINE);
                else
                    assebledPassword[i] = (char)random.Next(ASCII_A_UPPER, ASCII_Z_UPPER);
            }

            string password = string.Empty;
            foreach (char ch in assebledPassword)
            {
                password += ch;
            }

            if (!string.IsNullOrEmpty(password))
                return password;
            else
                return "00000";
        }
    }
}
