using System;
using System.Collections.Generic;
using RaftCore;
using RaftCore.Connections;
using RaftCore.Components;

namespace RaftCore.Connections.Implementations {
    public class APIRaftConnector : IRaftConnector {
        // Connects to a web API given by a base URL
        // TODO: Document endpoints

        public uint NodeId { get; private set; }
        private string baseURL { get; set; }

        public APIRaftConnector(uint nodeId, string baseURL) {
            this.NodeId = nodeId;
            this.baseURL = baseURL;
        }

        public void MakeRequest(String command) {
            // convert params to json
            // make POST <baseurl>/makerequest
        }
        
        public Result<bool> RequestVote(int term, uint candidateId, int lastLogIndex, int lastLogTerm) {
            // convert params to json
            // make POST <baseurl>/requestvote
            // parse response into a Result object
            return new Result<bool>(true, 1);
        }

        public Result<bool> AppendEntries(int term, uint leaderId, int prevLogIndex, int prevLogTerm, List<LogEntry> entries, int leaderCommit) {
            // convert params to json
            // make POST <baseurl>/appendentries
            // parse response into a Result object
            return new Result<bool>(true, 1);
        }

        public void TestConnection() {
            // make POST <baseurl>/test
        }
    }
}
