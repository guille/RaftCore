using System;
using System.Collections.Generic;
using RaftCore.Components;

namespace RaftCore.Connections.Implementations {
    /// <summary>
    /// Simple object connector, directly communicates to nodes in memory
    /// </summary>
    public class ObjectRaftConnector : IRaftConnector {
        /// <summary>
        /// ID matching an existing node's ID.
        /// </summary>
        public uint NodeId { get; private set; }
        private RaftNode Node { get; set; }

        /// <summary>
        /// Initializes a connector with nodes in memory.
        /// </summary>
        /// <param name="nodeId">ID that represents this connector's node</param>
        /// <param name="node"><see cref="RaftNode"/> object</param>
        public ObjectRaftConnector(uint nodeId, RaftNode node) {
            this.NodeId = nodeId;
            this.Node = node;
        }

        /// <summary>
        /// Calls the MakeRequest method on the node.
        /// </summary>
        /// <param name="command">String containing the request to send to the node</param>
        public void MakeRequest(String command) {
            Node.MakeRequest(command);
        }

        /// <summary>
        /// Calls the RequestVote method on the node.
        /// </summary>
        /// <param name="term">Term of the candidate</param>
        /// <param name="candidateId">Node ID of the candidate</param>
        /// <param name="lastLogIndex">Index of candidate's last log entry</param>
        /// <param name="lastLogTerm">Term of candidate's last log entry</param>
        /// <returns>Returns a Result object containing the current term of the node and whether it grants the requested vote</returns>
        public Result<bool> RequestVote(int term, uint candidateId, int lastLogIndex, int lastLogTerm) {
            return Node.RequestVote(term, candidateId, lastLogIndex, lastLogTerm);
        }

        /// <summary>
        /// Calls the AppendEntries method on the node.
        /// </summary>
        /// <param name="term">Leader's current term number</param>
        /// <param name="leaderId">ID of the node invoking this method</param>
        /// <param name="prevLogIndex">Index of log immediately preceding new ones</param>
        /// <param name="prevLogTerm">Term of prevLogIndex entry</param>
        /// <param name="entries">List of entries sent to be replicated. null for heartbeat</param>
        /// <param name="leaderCommit">Leader's CommitIndex</param>
        /// <returns>Returns a Result object containing the current term of the node and whether the request worked</returns>
        public Result<bool> AppendEntries(int term, uint leaderId, int prevLogIndex, int prevLogTerm, List<LogEntry> entries, int leaderCommit) {
            return Node.AppendEntries(term, leaderId, prevLogIndex, prevLogTerm, entries, leaderCommit);
        }

        /// <summary>
        /// Calls the TestConnection method on the node.
        /// </summary>
        public void TestConnection() {
            Node.TestConnection();
        }
    }
}
