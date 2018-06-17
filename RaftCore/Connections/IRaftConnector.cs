using System;
using System.Collections.Generic;
using System.Text;

namespace RaftCore.Connections {
    public interface IRaftConnector {
        // Defines how to connect and interact with node NodeId
        
        uint NodeId { get; }
        void SendRequestToNode(String command);
        Result<bool> RequestVote(int term, uint candidateId, int lastLogIndex, int lastLogTerm);
        Result<bool> AppendEntries(int term, uint leaderId, int prevLogIndex, int prevLogTerm, List<LogEntry> entries, int leaderCommit);
        void TestConnection();
    }
}
