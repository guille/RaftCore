using System;
using Xunit;
using System.Collections.Generic;
using System.Threading;
using RaftCore;
using RaftCore.StateMachine.Implementations;
using RaftCore.Connections;
using RaftCore.Connections.Implementations;
using RaftCore.Components;
using RaftCoreTest.Util;

namespace RaftCoreTest {
    public class RaftNodeTest {

        [Fact]
        public void TestGetCommittedEntries() {
            var node = TestMethods.CreateNode();

            // Empty when log is empty
            Assert.Empty(node.GetCommittedEntries());
            var entry1 = new LogEntry(1, 1, "2");
            var entry2 = new LogEntry(1, 2, "4");

            node.AppendEntries(1, 2, 0, 0, new List<LogEntry>() { entry1, entry2 }, 0);
            var committed = node.GetCommittedEntries();
            Assert.NotEmpty(committed);
            Assert.Single(committed);
            Assert.Equal(2, node.Log.Count);
            Assert.Equal(entry1, committed[0]);

            node.AppendEntries(5, 2, 0, 0, null, 1);
            committed = node.GetCommittedEntries();
            Assert.NotEmpty(committed);
            Assert.Equal(2, committed.Count);
            Assert.Equal(entry2, committed[1]);
        }

        [Fact]
        public void TestAppendEntriesTermValues() {
            var node = TestMethods.CreateNode();

            Assert.Equal(0, node.CurrentTerm);

            // It updates the term when it receives a RPC with a higher term
            node.AppendEntries(2, 2, 0, 0, null, 0);
            Assert.Equal(2, node.CurrentTerm);
            
            // It drops RPCs with lower terms and sends back current term
            var res = node.AppendEntries(0, 2, 0, 0, null, 0);
            Assert.False(res.Value);
            Assert.Equal(node.CurrentTerm, res.Term);
        }

        [Fact]
        public void TestRequestVoteTermValues() {
            var node = TestMethods.CreateNode();

            Assert.Equal(0, node.CurrentTerm);

            // It updates the term when it receives a RPC with a higher term
            node.RequestVote(2, 2, 0, 0);
            Assert.Equal(2, node.CurrentTerm);
            
            // It drops RPCs with lower terms and sends back current term
            var res = node.RequestVote(0, 2, 0, 0);
            Assert.False(res.Value);
            Assert.Equal(node.CurrentTerm, res.Term);
        }

        [Fact]
        public void TestNodeStopAndRestart() {
            var node = TestMethods.CreateNode();

            Assert.Equal(NodeState.Follower, node.NodeState);

            node.Stop();

            Assert.Equal(NodeState.Stopped, node.NodeState);

            node.Restart();

            Assert.NotEqual(NodeState.Stopped, node.NodeState);

            node.Stop();
        }

        [Fact]
        public void TestNoRPCWhenStopped() {
            var node = TestMethods.CreateNode();

            node.Stop();

            var res = node.AppendEntries(1, 2, 0, 0, null, 0);

            Assert.False(res.Value);

            res = node.RequestVote(1, 2, 0, 0);

            Assert.False(res.Value);
        }


        [Fact]
        public void TestAppendEntriesConsistencyCheck() {
            var node = TestMethods.CreateNode();

            // Append dummy valid entry, log is initially empty
            var entry1 = new LogEntry(1, 0, "2");
            var res = node.AppendEntries(1, 1, 0, 0, new List<LogEntry>() { entry1 }, 0);
            Assert.True(res.Value);
            Assert.Equal("2", node.Log[0].Command);

            // Append dummy valid entry, passes the consistency check:
            // Log[0].TermNumber == 1
            var entry2 = new LogEntry(1, 1, "3");
            res = node.AppendEntries(1, 1, 0, 1, new List<LogEntry>() { entry2 }, 0);
            Assert.True(res.Value);
            Assert.Equal("3", node.Log[1].Command);

            // Send new AppendEntries and fail the consistency check:
            // Log[0].TermNumber != 0
            // 1 != 0
            var entry3 = new LogEntry(1, 2, "4");
            res = node.AppendEntries(3, 1, 0, 0, new List<LogEntry>() { entry3 }, 0);
            Assert.False(res.Value);
        }

        [Fact]
        public void TestRequestVoteConsistencyCheck() {
            var node = TestMethods.CreateNode();

            // Append dummy entry with term 1
            var entry1 = new LogEntry(1, 0, "2");
            var entry2 = new LogEntry(2, 1, "3");
            node.AppendEntries(1, 1, 0, 0, new List<LogEntry>() { entry1, entry2 }, 0);
            // node.Log.Count - 1 --> 1
            Assert.Equal(1, node.Log.Count - 1);
            // node.LastLogTerm   --> 2
            Assert.Equal(2, node.Log[node.Log.Count - 1].TermNumber);

            // pass consistency check
            var res = node.RequestVote(2, 2, 1, 2);
            Assert.True(res.Value);

            // outdated lastlogindex
            // if lastLogIndex >= Log.Count - 1 is false, no vote
            res = node.RequestVote(2, 2, 0, 2);
            Assert.False(res.Value);

            // outdated lastlogterm
            // if lastLogTerm >= GetLastLogTerm() is false, no vote
            res = node.RequestVote(2, 2, 1, 1);
            Assert.False(res.Value);
        }

        [Fact]
        public void TestMakeRequestToLeader() {
            var node = TestMethods.CreateNode();
            node.Run();
            Thread.Sleep(TestMethods.WAIT_MS);
            Assert.Equal(NodeState.Leader, node.NodeState);
            node.MakeRequest("2");
            Assert.Equal("2", node.Log[0].Command);
        }

        [Fact]
        public void TestMakeRequestToFollower() {
            // Start two nodes in a cluster
            RaftNode[] nodes = TestMethods.ConfigureAndRunRaftCluster(2, SM.Dictionary);
            
            Thread.Sleep(TestMethods.WAIT_MS);

            if (nodes[0].NodeState == NodeState.Leader) {
                nodes[1].MakeRequest("2");
                Assert.Equal("2", nodes[0].Log[0].Command);
            }
            else if (nodes[1].NodeState == NodeState.Leader) {
                nodes[0].MakeRequest("2");
                Assert.Equal("2", nodes[1].Log[0].Command);
            }
        }

        [Fact]
        public void TestApplyEntrySingleNode() {
            var node = TestMethods.CreateNode();
            node.Run();
            Thread.Sleep(TestMethods.WAIT_MS);
            Assert.Equal(NodeState.Leader, node.NodeState);

            node.MakeRequest("2");
            Thread.Sleep(TestMethods.WAIT_MS);
            // Since the single node forms a majority, it will apply the request to its state machine.
            Assert.Equal("2", node.StateMachine.RequestStatus(""));
            Assert.Equal(0, node.CommitIndex);
            Assert.Equal(0, node.LastApplied);
        }

        [Fact]
        public void TestApplyEntry() {
            var node = TestMethods.CreateNode();
            var entry1 = new LogEntry(1, 1, "2");
            var entry2 = new LogEntry(1, 2, "4");
            var entry3 = new LogEntry(1, 3, "-1");

            node.AppendEntries(1, 2, 0, 0, new List<LogEntry>() { entry1, entry2, entry3 }, 1);

            // Since leaderCommit is 1, it will apply entry1 and entry2
            Assert.Equal("6", node.StateMachine.RequestStatus(""));
            Assert.Equal(1, node.CommitIndex);
            Assert.Equal(1, node.LastApplied);
        }

        [Fact]
        public void TestApplyEntryFail() {
            var node = TestMethods.CreateNode();
            var entry = new LogEntry(1, 1, "2");
            var res = node.AppendEntries(1, 2, 0, 0, null, 14);
            // The node doesn't have entries in its log matching the commit index
            Assert.False(res.Value);
        }

        [Fact]
        public void TestSendHeartbeats() {
            // Start two nodes in a cluster
            RaftNode[] nodes = TestMethods.ConfigureAndRunRaftCluster(2, SM.Dictionary);
            
            Thread.Sleep(TestMethods.WAIT_MS);

            var leader = (nodes[0].NodeState == NodeState.Leader) ? nodes[0] : nodes[1];
            var term = leader.CurrentTerm;

            Thread.Sleep(TestMethods.WAIT_MS);

            // The elected leader will still be the leader with the same term number, since it has been sending heartbeats. 

            Assert.Equal(NodeState.Leader, leader.NodeState);
            Assert.Equal(term, leader.CurrentTerm);
        }
    }
}
