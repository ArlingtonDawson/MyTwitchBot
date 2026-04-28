using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTwitchBot
{
    public static class TwitchIrcMessageParser
    {
        public static string GetUsername(string message)
        {
            int exclamation = message.IndexOf('!');
            return exclamation > 0 ? message.Substring(1, exclamation - 1) : "unknown";
        }

        public static string GetMessageText(string message)
        {
            int colonIndex = message.IndexOf(":", 1);
            return colonIndex >= 0 ? message.Substring(colonIndex + 1).Trim() : "";
        }

        public static bool IsModerator(string message)
        {
            if (!message.StartsWith("@")) return false;

            string tagSection = message.Split(' ')[0].TrimStart('@');
            string[] tags = tagSection.Split(';');

            foreach (string tag in tags)
            {
                if (tag.StartsWith("badges=") && (tag.Contains("moderator") || tag.Contains("broadcaster")))
                    return true;

                if (tag.StartsWith("mod=") && tag.EndsWith("1"))
                    return true;
            }

            return false;
        }
    }
}
