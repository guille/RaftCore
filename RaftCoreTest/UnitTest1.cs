using System;
using Xunit;
using RaftCore;
using RaftCore.Connections;
using RaftCore.Connections.Implementations;
using RaftCore.StateMachine.Implementations;
using System.Threading;

namespace RaftCoreTest {
    public class UnitTest1 {

        // TODO: Move next function to new package (test.util)

        internal enum SM { Numeral, Dictionary };
        
        // Creates and returns a configured array of raftnodes using the test cluster
        private RaftNode[] ConfigureRaftCluster(int numberOfNodes, SM sm) {
            RaftNode[] nodes = new RaftNode[numberOfNodes];
            
            // Create nodes
            for (uint i = 0; i < numberOfNodes; i++) {
                if (sm == SM.Numeral){
                    nodes[i] = new RaftNode(i, new NumeralStateMachine());
                }
                else {
                    nodes[i] = new RaftNode(i, new DictionaryStateMachine());
                }

            }

            // Adding them to a cluster and configuring them
            foreach (RaftNode node in nodes) {
                var c = new RaftCluster();
                Array.ForEach(nodes, x => c.AddNode(new ObjectRaftConnector(x.NodeId, x)));
                node.Configure(c);
            }

            return nodes;
        }


        /*********************************
        *    Dictionary State Machine    *
        *                                *
        **********************************/


        [Fact (Skip =   "")]
        public void TestLogReplicationDictionary() {
            // Only test log replication works
            int nodeCount = 3;
            RaftNode[] nodes = ConfigureRaftCluster(nodeCount, SM.Dictionary);

            foreach (RaftNode node in nodes) {
                node.Run();
            }

            Thread.Sleep(1500);

            nodes[0].MakeRequest("SET X 10");
            nodes[1].MakeRequest("SET Y 22");

            Thread.Sleep(1500);

            foreach (var node in nodes) {
                Assert.Equal("SET X 10", node.Log[0].Command);
                Assert.Equal(0, node.Log[0].Index);
                Assert.Equal("SET Y 22", node.Log[1].Command);
                Assert.Equal(1, node.Log[1].Index);
                Assert.Equal(2, node.GetCommittedEntries().Count);
            }

            Array.ForEach(nodes, x => x.Stop());
        }

        [Fact (Skip =   "")]
        public void TestDictionaryStateMachineReplication() {
            // Goes further and tests state machine
            int nodeCount = 3;
            RaftNode[] nodes = ConfigureRaftCluster(nodeCount, SM.Dictionary);

            foreach (RaftNode node in nodes) {
                node.Run();
            }

            Thread.Sleep(1000);

            nodes[0].MakeRequest("SET X 10");

            Thread.Sleep(1500);

            Array.ForEach(nodes, x => x.Stop());

            foreach (var node in nodes) {
                Assert.Equal("10", node.StateMachine.RequestStatus("X"));
            }

            Array.ForEach(nodes, x => x.Stop());
        }

        /*********************************
        *      Numeral State Machine     *
        *                                *
        **********************************/

        [Fact (Skip =   "")]
        public void TestNumeralStateMachineReplication() {
            // Goes further and tests state machine
            int nodeCount = 3;
            RaftNode[] nodes = ConfigureRaftCluster(nodeCount, SM.Numeral);

            foreach (RaftNode node in nodes) {
                node.Run();
            }

            Thread.Sleep(1500);

            nodes[0].MakeRequest("+10");
            nodes[1].MakeRequest("-21");

            Thread.Sleep(1500);

            Array.ForEach(nodes, x => x.Stop());

            foreach (var node in nodes) {
                Assert.Equal("-11", node.StateMachine.RequestStatus(""));
            }

            Array.ForEach(nodes, x => x.Stop());
        }


        /*********************************
        *  Independent of State Machine  *
        *                                *
        **********************************/

        [Fact (Skip =   "")]
        public void TestLeaderElection() {
            int nodeCount = 3;
            RaftNode[] nodes = ConfigureRaftCluster(nodeCount, SM.Dictionary);

            foreach (RaftNode node in nodes) {
                node.Run();
            }

            Thread.Sleep(1500);

            int candidates = 0;
            int leaders = 0;
            int followers = 0;
            foreach (var node in nodes) {
                if (node.NodeState == NodeState.Candidate) candidates++;
                else if (node.NodeState == NodeState.Leader) leaders++;
                else if (node.NodeState == NodeState.Follower) followers++;

            }

            // Array.ForEach(nodes, x => Console.WriteLine(x.ToString()));
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


        // Running the test by leaving the reason empty doesn't seem to work for theories(?)
        // [Theory (Skip =  "")]
        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(5)]
        public void TestDifferentElectionTimeouts(int nodeCount) {
            RaftNode[] nodes = ConfigureRaftCluster(nodeCount, SM.Dictionary);
            
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

        [Fact (Skip =  "")]
        public void TestStopRestartNode() {
            int nodeCount = 3;
            RaftNode[] nodes = ConfigureRaftCluster(nodeCount, SM.Dictionary);
            
            foreach (RaftNode node in nodes) {
                node.Run();
            }

            Thread.Sleep(1500);

            nodes[2].Stop();

            nodes[0].MakeRequest("SET X 10");
            nodes[1].MakeRequest("SET Y 22");

            Thread.Sleep(2500);

            nodes[2].Restart();

            Thread.Sleep(1500);

            foreach (var node in nodes) {
                // Console.WriteLine(node.ToString());
                Assert.Equal("SET X 10", node.Log[0].Command);
                Assert.Equal(0, node.Log[0].Index);
                Assert.Equal("SET Y 22", node.Log[1].Command);
                Assert.Equal(1, node.Log[1].Index);
                Assert.Equal(2, node.GetCommittedEntries().Count);
            }

            Array.ForEach(nodes, x => x.Stop());
        }

        [Fact (Skip =  "")]
        public void TestStartLotsOfNodes() {
            RaftNode[] nodes = ConfigureRaftCluster(3000, SM.Dictionary);
            foreach (RaftNode node in nodes) {
                node.Run();
            }

            Thread.Sleep(2500);

            nodes[0].MakeRequest("SET X 10");

            Thread.Sleep(2500);

            Array.ForEach(nodes, x => Assert.NotNull(x.LeaderId));

            Array.ForEach(nodes, x => x.Stop());

            foreach (var node in nodes) {
                Assert.Equal("10", node.StateMachine.RequestStatus("X"));
            }
        }
    }
}
