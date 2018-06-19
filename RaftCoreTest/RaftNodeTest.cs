using System;
using Xunit;
using System.Collections.Generic;
using RaftCore;
using RaftCore.StateMachine.Implementations;
using RaftCore.Connections;
using RaftCore.Connections.Implementations;

namespace RaftCoreTest {
    public class RaftNodeTest {
        private RaftNode CreateNode() {
            RaftNode node = new RaftNode(1, new NumeralStateMachine());
            var c = new RaftCluster();
            c.AddNode(new ObjectRaftConnector(node.NodeId, node));
            node.Configure(c);
            return node;
        }

        [Fact]
        public void TestGetCommittedEntries() {
            var node = CreateNode();

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
        public void TestAppendEntriesTermChanges() {
            var node = CreateNode();

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
        public void TestRequestVoteTermChanges() {
            var node = CreateNode();

            Assert.Equal(0, node.CurrentTerm);

            // It updates the term when it receives a RPC with a higher term
            node.RequestVote(2, 2, 0, 0);
            Assert.Equal(2, node.CurrentTerm);
            
            // It drops RPCs with lower terms and sends back current term
            var res = node.RequestVote(0, 2, 0, 0);
            Assert.False(res.Value);
            Assert.Equal(node.CurrentTerm, res.Term);
        }
    }
}


