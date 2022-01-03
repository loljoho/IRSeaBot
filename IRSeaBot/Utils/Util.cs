using System;
using System.Text;

namespace IRSeaBot.Utils
{
    public class Util
    {
        public static string GetRestOfMessage(string[] msg)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i < msg.Length; i++)
            {
                sb.Append(msg[i] + " ");
            }
            return sb.ToString();
        }

        public static string GetFirstWordOfMessage(string[] msg)
        {
            string word = String.Empty;
            if (msg.Length >= 1)
            {
                word = msg[1];
            }
            return word;
        }

    }
}
