using System;
using Xunit;
using EasyRaft;
using EasyRaft.Connections;
using EasyRaft.Connections.Implementations;
using EasyRaftTest.util;
using System.Threading;

namespace EasyRaftTest {
    public class UnitTest1 {

        // Creates and returns a configured array of raftnodes using the test cluster
        private RaftNode[] ConfigureRaftCluster(int numberOfNodes) {
            RaftNode[] nodes = new RaftNode[numberOfNodes];

            // Create nodes
            for (uint i = 0; i < numberOfNodes; i++) {
                nodes[i] = new RaftNode(i, new TestStateMachine());
            }

            // Adding them to a cluster and configuring them
            foreach (RaftNode node in nodes) {
                var c = new RaftCluster();
                Array.ForEach(nodes, x => c.AddNode(new ObjectRaftConnector(x.NodeId, x)));
                node.Configure(c);
            }

            return nodes;
        }


        [Fact (Skip = "")]
        public void TestLogReplication() {
            // Only test log replication works
            int nodeCount = 3;
            RaftNode[] nodes = ConfigureRaftCluster(nodeCount);
            // Thread[] threads = new Thread[nodeCount];
            // int i = 0;

            foreach (RaftNode node in nodes) {
                node.Run();
                // threads[i] = new Thread(node.Run);
                // threads[i].Start();
                // i++;
            }

            // foreach (Thread thread in threads) {
            //     thread.Join();
            // }

            Thread.Sleep(3500);

            nodes[0].MakeRequest("SET X 10");
            nodes[1].MakeRequest("SET Y 22");

            Thread.Sleep(3500);

            // Array.ForEach(nodes, x => Console.WriteLine(x.ToString()));

            foreach (var node in nodes) {
                Assert.Equal("SET X 10", node.Log[0].Command);
                Assert.Equal(0, node.Log[0].Index);
                Assert.Equal("SET Y 22", node.Log[1].Command);
                Assert.Equal(1, node.Log[1].Index);
                Assert.Equal(2, node.GetCommittedEntries().Count);
            }

            Array.ForEach(nodes, x => x.Stop());
        }

        [Fact (Skip = "time")]
        public void TestStateMachineReplication() {
            // Goes further and tests state machine
            int nodeCount = 3;
            RaftNode[] nodes = ConfigureRaftCluster(nodeCount);

            foreach (RaftNode node in nodes) {
                node.Run();
            }

            Thread.Sleep(3500);

            nodes[0].MakeRequest("SET X 10");

            Thread.Sleep(3500);

            Array.ForEach(nodes, x => x.Stop());

            foreach (var node in nodes) {
                Assert.Equal(10, node.StateMachine.RequestStatus("X"));
            }

            Array.ForEach(nodes, x => x.Stop());
        }

        [Fact (Skip = "time")]
        public void TestLeaderElection() {
            int nodeCount = 3;
            RaftNode[] nodes = ConfigureRaftCluster(nodeCount);

            foreach (RaftNode node in nodes) {
                node.Run();
            }

            Thread.Sleep(7000);

            int candidates = 0;
            int leaders = 0;
            int followers = 0;
            foreach (var node in nodes) {
                if (node.NodeState == NodeState.Candidate) candidates++;
                else if (node.NodeState == NodeState.Leader) leaders++;
                else if (node.NodeState == NodeState.Follower) followers++;

            }

            Array.ForEach(nodes, x => Console.WriteLine(x.ToString()));
            Array.ForEach(nodes, x => x.Stop());

            if (followers == nodeCount) {
                // Valid state 2
                Assert.Equal(0, candidates);
                Assert.Equal(nodeCount, followers);
                Assert.Equal(0, candidates);
                foreach (var node in nodes)
                    Assert.Null(node.LeaderId);
                // all the LeaderId are null?
            }
            else if (candidates > 0) {
                // Valid state 2
                ; // TODO
            }
            else {
                // Valid state 3
                Assert.Equal(0, candidates);
                Assert.Equal(nodeCount - 1, followers);
                Assert.Equal(1, leaders);
            }

        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(5)]
        public void TestDifferentElectionTimeouts(int nodeCount) {
            RaftNode[] nodes = ConfigureRaftCluster(nodeCount);
            
            // Tests all the election timeouts are different
            // Could fail

            foreach (var node in nodes) {
                int matches = 0;
                foreach (var node2 in nodes) {
                    if (node2.ElectionTimeoutMS == node.ElectionTimeoutMS) matches++;
                }
                Assert.Equal(1, matches);
            }

        }
    }
}
