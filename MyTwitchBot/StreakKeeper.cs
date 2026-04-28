using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTwitchBot
{
    public class StreakKeeper
    {
        public int Wins { get; private set; } = 0;
        public int Losses { get; private set; } = 0;
        public int StreakCount { get; private set; } = 0;
        public string StreakName { get; private set; } = string.Empty;



        public void Lose()
        {
            Losses++;
            if(StreakName == "L")
            {  StreakCount++; }
            else { StreakCount=1;
                StreakName = "L";
            }
        }

        public void Win()
        {
            Wins++;
            if (StreakName == "W")
            { StreakCount++; }
            else
            {
                StreakCount = 1;
                StreakName = "W";
            }
        }

        public string GetStreakMessage()
        {
            if (StreakCount < 2)
                return string.Empty;  // ← was returning spaces before, tests expect Empty

            string streakType = StreakName == "W" ? "Win" : "Lose";
            return $"Streak: {streakType} {StreakCount}";
        }

        public string GetScoreLine()
        {
            return $"{Wins}W - {Losses}L {GetStreakMessage()}".Trim();
        }

        public StreakKeeper Clone()
        {
            return new StreakKeeper
            {
                Wins = this.Wins,
                Losses=this.Losses,
                StreakCount=this.StreakCount,
                StreakName=this.StreakName
            };
        }
    }
}
