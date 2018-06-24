using System;
using Xunit;
using RaftCore;
using RaftCore.Connections;
using RaftCore.Connections.Implementations;
using RaftCore.StateMachine.Implementations;
using System.Threading;
using RaftCoreTest.Util;

namespace RaftCoreTest {
    public class IntegrationTests {

        /*********************************
        *    Dictionary State Machine    *
        *                                *
        **********************************/

        [Fact (Skip =   "")]
        public void TestLogReplicationDictionary() {
            // Only test log replication works
            int nodeCount = 3;
            RaftNode[] nodes = TestMethods.ConfigureAndRunRaftCluster(nodeCount, SM.Dictionary);

            Thread.Sleep(TestMethods.WAIT_MS);

            nodes[0].MakeRequest("SET X 10");
            nodes[1].MakeRequest("SET Y 22");

            Thread.Sleep(TestMethods.WAIT_MS);

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
            RaftNode[] nodes = TestMethods.ConfigureAndRunRaftCluster(nodeCount, SM.Dictionary);

            Thread.Sleep(TestMethods.WAIT_MS);

            nodes[0].MakeRequest("SET X 10");

            Thread.Sleep(TestMethods.WAIT_MS);

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
            RaftNode[] nodes = TestMethods.ConfigureAndRunRaftCluster(nodeCount, SM.Numeral);

            Thread.Sleep(TestMethods.WAIT_MS);

            nodes[0].MakeRequest("+10");
            nodes[1].MakeRequest("-21");

            Thread.Sleep(TestMethods.WAIT_MS);

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
            RaftNode[] nodes = TestMethods.ConfigureAndRunRaftCluster(nodeCount, SM.Dictionary);

            Thread.Sleep(TestMethods.WAIT_MS);

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

        [Fact (Skip =  "")]
        public void TestStopRestartNode() {
            int nodeCount = 3;
            RaftNode[] nodes = TestMethods.ConfigureAndRunRaftCluster(nodeCount, SM.Dictionary);

            Thread.Sleep(TestMethods.WAIT_MS);

            nodes[2].Stop();

            Thread.Sleep(TestMethods.WAIT_MS);

            nodes[0].MakeRequest("SET X 10");
            nodes[1].MakeRequest("SET Y 22");

            Thread.Sleep(TestMethods.WAIT_MS);

            nodes[2].Restart();

            Thread.Sleep(TestMethods.WAIT_MS);

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

        [Fact (Skip =  "time")]
        public void TestStartLotsOfNodes() {
            RaftNode[] nodes = TestMethods.ConfigureAndRunRaftCluster(5000, SM.Dictionary);
            
            Thread.Sleep(2000);

            nodes[0].MakeRequest("SET X 10");

            Thread.Sleep(1000);

            Array.ForEach(nodes, x => Assert.NotNull(x.LeaderId));

            Array.ForEach(nodes, x => x.Stop());

            foreach (var node in nodes) {
                Assert.Equal("10", node.StateMachine.RequestStatus("X"));
            }
        }
    }
}
