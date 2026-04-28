namespace MyTwitchBot.Tests
{
    public class StreakKeeperTests
    {
        [Fact]
        public void Win_IncrementsWinCount()
        {
            var sk = new StreakKeeper();
            sk.Win();
            Assert.Equal(1, sk.Wins);
        }

        [Fact]
        public void Lose_IncrementsLossCount()
        {
            var sk = new StreakKeeper();
            sk.Lose();
            Assert.Equal(1, sk.Losses);
        }

        [Fact]
        public void Win_StartsWinStreak()
        {
            var sk = new StreakKeeper();
            sk.Win();
            Assert.Equal("W", sk.StreakName);
            Assert.Equal(1, sk.StreakCount);
        }

        [Fact]
        public void ConsecutiveWins_IncrementsStreak()
        {
            var sk = new StreakKeeper();
            sk.Win();
            sk.Win();
            sk.Win();
            Assert.Equal(3, sk.StreakCount);
            Assert.Equal("W", sk.StreakName);
        }

        [Fact]
        public void LoseAfterWins_ResetsStreakToOne()
        {
            var sk = new StreakKeeper();
            sk.Win();
            sk.Win();
            sk.Lose();
            Assert.Equal(1, sk.StreakCount);
            Assert.Equal("L", sk.StreakName);
        }

        [Fact]
        public void GetStreakMessage_ReturnsEmpty_WhenStreakLessThanTwo()
        {
            var sk = new StreakKeeper();
            sk.Win();
            Assert.Equal(string.Empty, sk.GetStreakMessage());
        }

        [Fact]
        public void GetStreakMessage_ReturnsMessage_WhenStreakTwoOrMore()
        {
            var sk = new StreakKeeper();
            sk.Win();
            sk.Win();
            Assert.Equal("Streak: Win 2", sk.GetStreakMessage());
        }

        [Fact]
        public void GetScoreLine_FormatsCorrectly()
        {
            var sk = new StreakKeeper();
            sk.Win();
            sk.Win();
            sk.Lose();
            Assert.Equal("2W - 1L", sk.GetScoreLine());
        }

        [Fact]
        public void Clone_CreatesIndependentCopy()
        {
            var sk = new StreakKeeper();
            sk.Win();
            sk.Win();

            var backup = sk.Clone();
            sk.Lose(); // modify original

            // Backup should be unaffected
            Assert.Equal(2, backup.Wins);
            Assert.Equal(0, backup.Losses);
            Assert.Equal("W", backup.StreakName);
        }

        [Fact]
        public void Clone_RestoredState_MatchesOriginal()
        {
            var sk = new StreakKeeper();
            sk.Win();
            sk.Win();

            var backup = sk.Clone();
            sk.Lose();

            // Restore from backup
            sk = backup;
            Assert.Equal(2, sk.Wins);
            Assert.Equal(0, sk.Losses);
            Assert.Equal(2, sk.StreakCount);
        }
    }
}
