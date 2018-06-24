using System;
using System.Collections.Generic;
using System.Linq;
using RaftCore;
using RaftCore.StateMachine.Implementations;
using RaftCore.Connections;
using RaftCore.Connections.Implementations;

namespace RaftCoreWeb.Services {
    public class ClusterService : ICluster {
        private RaftCluster cluster;
        private List<RaftNode> nodes;

        public ClusterService() {
            var numberOfNodes = 5;
            var _nodes = new List<RaftNode>();

            // Create nodes
            for (uint i = 0; i < numberOfNodes; i++) {
                var node = new RaftNode(i, new NumeralStateMachine());
                _nodes.Add(node);
            }

            // Adding them to a cluster and configuring them
            foreach (RaftNode node in _nodes) {
                var c = new RaftCluster();
                _nodes.ForEach(x => c.AddNode(new ObjectRaftConnector(x.NodeId, x)));
                node.Configure(c);
            }

            this.cluster = _nodes[0].Cluster;

            _nodes.ForEach(node => node.Run());

            this.nodes = _nodes;
        }

        public RaftNode GetNode(uint id) {
            return nodes.Find(x => x.NodeId == id);
        }

        public List<RaftNode> GetNodes() {
            return nodes;
        }

        public RaftCluster GetCluster() {
            return cluster;
        }
    }
}