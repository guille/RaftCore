using System;
using System.Collections.Generic;
using RaftCore.Components;

namespace RaftCore.Connections {
    /// <summary>
    /// Defines how to connect and interact with a node.
    /// </summary>
    public interface IRaftConnector {
        /// <summary>
        /// ID uniquely identifying the node this <see cref="IRaftConnector"/> connects to.
        /// </summary>
        uint NodeId { get; }

        /// <summary>
        /// Calls the MakeRequest method on the node.
        /// </summary>
        /// <param name="command">String containing the request to send to the node</param>
        void MakeRequest(String command);

        /// <summary>
        /// Calls the RequestVote method on the node.
        /// </summary>
        /// <param name="term">Term of the candidate</param>
        /// <param name="candidateId">Node ID of the candidate</param>
        /// <param name="lastLogIndex">Index of candidate's last log entry</param>
        /// <param name="lastLogTerm">Term of candidate's last log entry</param>
        /// <returns>Returns a Result object containing the current term of the node and whether it grants the requested vote</returns>
        Result<bool> RequestVote(int term, uint candidateId, int lastLogIndex, int lastLogTerm);

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
        Result<bool> AppendEntries(int term, uint leaderId, int prevLogIndex, int prevLogTerm, List<LogEntry> entries, int leaderCommit);

        /// <summary>
        /// Calls the TestConnection method on the node.
        /// </summary>
        void TestConnection();
    }
}
