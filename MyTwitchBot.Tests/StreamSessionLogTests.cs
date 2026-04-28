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
        }

        [Fact]
        public void AddFollower_DoesNotAffectSubscribers()
        {
            var log = new StreamSessionLog();
            log.AddFollower("ViewerOne");
            Assert.Empty(log.NewSubscribers);
        }
    }
}