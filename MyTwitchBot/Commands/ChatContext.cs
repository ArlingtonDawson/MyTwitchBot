using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTwitchBot.Commands
{
    public class ChatContext
    {
        public TwitchIrcClient IrcClient { get; set; }
        public string ChannelName { get; set; }
        public string Username { get; set; }
        public StreakKeeper StreakKeeper { get; set; }
        public StreakKeeper BackupStreakKeeper { get; set; }
    }
}
