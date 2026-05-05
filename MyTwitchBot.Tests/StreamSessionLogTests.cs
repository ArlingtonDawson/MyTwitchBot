using MyTwitchBot.EventSub;
using System;
using System.Collections.Generic;
using System.Text;

// StreamSessionLogTests.cs
namespace MyTwitchBot.Tests
{
    public class StreamSessionLogTests
    {
        [Fact]
        public void AddFollower_AddsFollowerToList()
        {
            var log = new StreamSessionLog();
            log.AddFollower("ViewerOne");
            Assert.Contains("ViewerOne", log.NewFollowers);
        }

        [Fact]
        public void AddFollower_Deduplicates()
        {
            var log = new StreamSessionLog();
            log.AddFollower("ViewerOne");
            log.AddFollower("ViewerOne");
            Assert.Single(log.NewFollowers);
        }

        [Fact]
        public void AddSubscriber_AddsSubscriberToList()
        {
            var log = new StreamSessionLog();
            log.AddSubscriber("SubOne");
            Assert.Contains("SubOne", log.NewSubscribers);
        }

        [Fact]
        public void AddSubscriber_Deduplicates()
        {
            var log = new StreamSessionLog();
            log.AddSubscriber("SubOne");
            log.AddSubscriber("SubOne");
            Assert.Single(log.NewSubscribers);
        }

        [Fact]
        public void AddGifter_AddsGifterWithCount()
        {
            var log = new StreamSessionLog();
            log.AddGifter("GenerousGuy", 5);
            Assert.True(log.Gifters.ContainsKey("GenerousGuy"));
            Assert.Equal(5, log.Gifters["GenerousGuy"]);
        }

        [Fact]
        public void AddGifter_AccumulatesCount_WhenGifterAlreadyExists()
        {
            var log = new StreamSessionLog();
            log.AddGifter("GenerousGuy", 3);
            log.AddGifter("GenerousGuy", 2);
            Assert.Equal(5, log.Gifters["GenerousGuy"]);
        }

        [Fact]
        public void AddGifter_TracksMultipleGifters_Independently()
        {
            var log = new StreamSessionLog();
            log.AddGifter("GenerousGuy", 5);
            log.AddGifter("KindViewer", 2);
            Assert.Equal(5, log.Gifters["GenerousGuy"]);
            Assert.Equal(2, log.Gifters["KindViewer"]);
        }

        [Fact]
        public void Collections_AreEmpty_OnCreation()
        {
            var log = new StreamSessionLog();
            Assert.Empty(log.NewFollowers);
            Assert.Empty(log.NewSubscribers);
            Assert.Empty(log.Gifters);
            Assert.Empty(log.ReturningViewers);
        }

        [Fact]
        public void AddFollower_DoesNotAffectSubscribers()
        {
            var log = new StreamSessionLog();
            log.AddFollower("ViewerOne");
            Assert.Empty(log.NewSubscribers);
        }

        [Fact]
        public void BanUser_RemovesFromFollowers()
        {
            var log = new StreamSessionLog();
            log.AddFollower("spambot");
            log.BanUser("spambot");
            Assert.DoesNotContain("spambot", log.NewFollowers);
        }

        [Fact]
        public void BanUser_RemovesFromSubscribers()
        {
            var log = new StreamSessionLog();
            log.AddSubscriber("spambot");
            log.BanUser("spambot");
            Assert.DoesNotContain("spambot", log.NewSubscribers);
        }

        [Fact]
        public void BanUser_RemovesFromGifters()
        {
            var log = new StreamSessionLog();
            log.AddGifter("spambot", 5);
            log.BanUser("spambot");
            Assert.DoesNotContain("spambot", log.Gifters.Keys);
        }

        [Fact]
        public void BanUser_IsCaseInsensitive()
        {
            var log = new StreamSessionLog();
            log.AddFollower("SpamBot");
            log.BanUser("spambot");
            Assert.DoesNotContain("SpamBot", log.NewFollowers);
        }

        [Fact]
        public void AddFollower_IgnoresBannedUser()
        {
            var log = new StreamSessionLog();
            log.BanUser("spambot");
            log.AddFollower("spambot");
            Assert.Empty(log.NewFollowers);
        }

        // Returning Viewers
        [Fact]
        public void AddReturningViewer_AddsToList()
        {
            var log = new StreamSessionLog();
            log.AddReturningViewer("RegularViewer");
            Assert.Contains("RegularViewer", log.ReturningViewers);
        }

        [Fact]
        public void AddReturningViewer_Deduplicates()
        {
            var log = new StreamSessionLog();
            log.AddReturningViewer("RegularViewer");
            log.AddReturningViewer("RegularViewer");
            Assert.Single(log.ReturningViewers);
        }

        [Fact]
        public void AddReturningViewer_IsCaseInsensitive()
        {
            var log = new StreamSessionLog();
            log.AddReturningViewer("RegularViewer");
            log.AddReturningViewer("regularviewer");
            Assert.Single(log.ReturningViewers);
        }

        [Fact]
        public void AddReturningViewer_DoesNotAdd_IfAlreadyNewFollower()
        {
            var log = new StreamSessionLog();
            log.AddFollower("NewFan");
            log.AddReturningViewer("NewFan");
            Assert.Empty(log.ReturningViewers);
            Assert.Single(log.NewFollowers);
        }
    }
}