using System;
using System.Collections.Generic;
using EasyRaft;
using EasyRaft.Connections;

namespace EasyRaft.Connections.Implementations {
    public class ObjectRaftConnector : IRaftConnector {
        // Simple object cluster, no IPs/RPCs

        public uint NodeId { get; private set; }
        private RaftNode Node { get; set; }

        public ObjectRaftConnector(uint nodeId, RaftNode node) {
            this.NodeId = nodeId;
            this.Node = node;
        }

        public void SendRequestToNode(String command) {
            // TODO: Return?
            Node.MakeRequest(command);
        }
        
        public bool RequestVote(int term, uint candidateId, int lastLogIndex, int lastLogTerm) {
            return Node.RequestVote(term, candidateId, lastLogIndex, lastLogTerm);
        }

        
        public bool AppendEntries(int term, uint leaderId, int prevLogIndex, int prevLogTerm, List<LogEntry> entries, int leaderCommit) {
            return Node.AppendEntries(term, leaderId, prevLogIndex, prevLogTerm, entries, leaderCommit);
        }

        // TODO: Internal?
        public void TestConnection() {
            Node.TestConnection();
        }

    }
}
