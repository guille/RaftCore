using System;
using Xunit;
using System.Collections.Generic;
using RaftCore;
using RaftCore.StateMachine.Implementations;
using RaftCore.Connections;
using RaftCore.Connections.Implementations;

namespace RaftCoreTest.Util {
    internal enum SM { Numeral, Dictionary };
    
    public static class TestMethods {
        internal const int WAIT_MS = 1000;

        // Creates a single-node cluster with a numeral state machine and returns the node
        static internal RaftNode CreateNode() {
            RaftNode node = new RaftNode(1, new NumeralStateMachine());
            var c = new RaftCluster();
            c.AddNode(new ObjectRaftConnector(node.NodeId, node));
            node.Configure(c);
            return node;
        }
        
        // Creates and returns a configured array of raftnodes using the test cluster
        static internal RaftNode[] ConfigureRaftCluster(int numberOfNodes, SM sm) {
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

        static internal RaftNode[] ConfigureAndRunRaftCluster(int numberOfNodes, SM sm) {
            RaftNode[] nodes = ConfigureRaftCluster(numberOfNodes, sm);
            Array.ForEach(nodes, node => node.Run());
            return nodes;
        }
    }
}