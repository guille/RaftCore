using System;
using System.Collections.Generic;
using RaftCore;
using RaftCore.Connections;
using RaftCore.Components;

namespace RaftCore.Connections.Implementations {
    /// <summary>
    /// Connects to nodes through a defined WEB API
    /// </summary>
    public class APIRaftConnector : IRaftConnector {
        /// <summary>
        /// ID matching an existing node's ID.
        /// </summary>
        public uint NodeId { get; private set; }
        private string baseURL { get; set; }

        /// <summary>
        /// Initializes a connector through a base URL.
        /// </summary>
        /// <param name="nodeId">ID that represents this connector's node</param>
        /// <param name="baseURL">Base URL to make requests on</param>
        public APIRaftConnector(uint nodeId, string baseURL) {
            this.NodeId = nodeId;
            this.baseURL = baseURL;
        }

        /// <summary>
        /// Calls the MakeRequest method on the node.
        /// For this, it makes a POST request to the endpoint baseURL/makerequest
        /// </summary>
        /// <param name="command">String containing the request to send to the node</param>
        public void MakeRequest(String command) {
            // convert params to json
            // make POST <baseurl>/makerequest
        }

        /// <summary>
        /// Calls the RequestVote method on the node.
        /// For this, it makes a POST request to the endpoint baseURL/requestvote
        /// And interprets the resulting JSON as a result.
        /// </summary>
        /// <param name="term">Term of the candidate</param>
        /// <param name="candidateId">Node ID of the candidate</param>
        /// <param name="lastLogIndex">Index of candidate's last log entry</param>
        /// <param name="lastLogTerm">Term of candidate's last log entry</param>
        /// <returns>Returns a Result object containing the current term of the node and whether it grants the requested vote</returns>
        public Result<bool> RequestVote(int term, uint candidateId, int lastLogIndex, int lastLogTerm) {
            // convert params to json
            // make POST <baseurl>/requestvote
            // parse response into a Result object
            return new Result<bool>(true, 1);
        }

        /// <summary>
        /// Calls the AppendEntries method on the node.
        /// For this, it makes a POST request to the endpoint baseURL/appendentries
        /// And interprets the resulting JSON as a result.
        /// </summary>
        /// <param name="term">Leader's current term number</param>
        /// <param name="leaderId">ID of the node invoking this method</param>
        /// <param name="prevLogIndex">Index of log immediately preceding new ones</param>
        /// <param name="prevLogTerm">Term of prevLogIndex entry</param>
        /// <param name="entries">List of entries sent to be replicated. null for heartbeat</param>
        /// <param name="leaderCommit">Leader's CommitIndex</param>
        /// <returns>Returns a Result object containing the current term of the node and whether the request worked</returns>
        public Result<bool> AppendEntries(int term, uint leaderId, int prevLogIndex, int prevLogTerm, List<LogEntry> entries, int leaderCommit) {
            // convert params to json
            // make POST <baseurl>/appendentries
            // parse response into a Result object
            return new Result<bool>(true, 1);
        }

        /// <summary>
        /// Calls the TestConnection method on the node.
        /// For this, it makes a POST request to the endpoint baseURL/test
        /// </summary>
        public void TestConnection() {
            // make POST <baseurl>/test
        }
    }
}
