using MyTwitchBot.Ads;
using System;
using System.Collections.Generic;
using System.Text;

// AdVoteManagerTests.cs
namespace MyTwitchBot.Tests
{
    public class AdVoteManagerTests
    {
        [Fact]
        public void VoteOpen_IsFalse_OnCreation()
        {
            var manager = new AdVoteManager();
            Assert.False(manager.VoteOpen);
        }

        [Fact]
        public void OpenVote_SetsVoteOpenToTrue()
        {
            var manager = new AdVoteManager();
            manager.OpenVote();
            Assert.True(manager.VoteOpen);
        }

        [Fact]
        public void CloseVote_SetsVoteOpenToFalse()
        {
            var manager = new AdVoteManager();
            manager.OpenVote();
            manager.CloseVote();
            Assert.False(manager.VoteOpen);
        }

        [Fact]
        public void CastVote_IncrementsVoteCount()
        {
            var manager = new AdVoteManager();
            manager.OpenVote();
            manager.CastVote("ViewerOne");
            Assert.Equal(1, manager.VoteCount);
        }

        [Fact]
        public void CastVote_Deduplicates_SameViewer()
        {
            var manager = new AdVoteManager();
            manager.OpenVote();
            manager.CastVote("ViewerOne");
            manager.CastVote("ViewerOne");
            Assert.Equal(1, manager.VoteCount);
        }

        [Fact]
        public void CastVote_AcceptsMultipleUniqueViewers()
        {
            var manager = new AdVoteManager();
            manager.OpenVote();
            manager.CastVote("ViewerOne");
            manager.CastVote("ViewerTwo");
            manager.CastVote("ViewerThree");
            Assert.Equal(3, manager.VoteCount);
        }

        [Fact]
        public void CastVote_DoesNotCount_WhenVoteClosed()
        {
            var manager = new AdVoteManager();
            manager.CastVote("ViewerOne"); // vote never opened
            Assert.Equal(0, manager.VoteCount);
        }

        [Fact]
        public void OpenVote_ClearsPreviousVotes()
        {
            var manager = new AdVoteManager();
            manager.OpenVote();
            manager.CastVote("ViewerOne");
            manager.CastVote("ViewerTwo");

            // Second vote round opens
            manager.OpenVote();
            Assert.Equal(0, manager.VoteCount);
        }

        [Fact]
        public void CastVote_DoesNotCount_AfterVoteClosed()
        {
            var manager = new AdVoteManager();
            manager.OpenVote();
            manager.CastVote("ViewerOne");
            manager.CloseVote();
            manager.CastVote("ViewerTwo"); // too late!
            Assert.Equal(1, manager.VoteCount);
        }
    }
}
