using System;
using System.Collections.Generic;
using RaftCore;
using RaftCore.Connections;
using RaftCore.Components;
using System.Net.Http;
using System.Threading.Tasks;

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
        private readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Initializes a connector through a base URL.
        /// </summary>
        /// <param name="nodeId">ID that represents this connector's node</param>
        /// <param name="baseURL">Base URL to make requests on</param>
        public APIRaftConnector(uint nodeId, string baseURL) {
            this.NodeId = nodeId;
            if (!baseURL.EndsWith("/")) {
                baseURL += "/";
            }
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
            SendMakeRequest(command);
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
            var res = SendRequestVote(term, candidateId, lastLogIndex, lastLogTerm).Result;
            
            return ParseResultFromJSON(res);
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
            var res = SendAppendEntries(term, leaderId, prevLogIndex, prevLogTerm, entries, leaderCommit).Result;

            return ParseResultFromJSON(res);
        }

        /// <summary>
        /// Calls the TestConnection method on the node.
        /// For this, it makes a GET request to the endpoint baseURL/test
        /// </summary>
        public void TestConnection() {
            SendTestConnection();
        }

        // **************************************
        // **************************************

        private async void SendMakeRequest(string command) {
            var req = new Dictionary<string, string> { { "request", command } };

            var content = new FormUrlEncodedContent(req);

            var response = await client.PostAsync(baseURL + "makerequest", content);
        }

        private async Task<string> SendRequestVote(int term, uint candidateId, int lastLogIndex, int lastLogTerm) {
            var req = new Dictionary<string, string>
            {
               { "term", term.ToString() },
               { "candidateId", candidateId.ToString() },
               { "lastLogIndex", lastLogIndex.ToString() },
               { "lastLogTerm", lastLogTerm.ToString() }
            };

            var content = new FormUrlEncodedContent(req);

            var response = await client.PostAsync(baseURL + "requestvote", content);

            var responseString = await response.Content.ReadAsStringAsync();

            return responseString;
        }

        private async Task<string> SendAppendEntries(int term, uint leaderId, int prevLogIndex, int prevLogTerm, List<LogEntry> entries, int leaderCommit) {
            var entriesDict = new object[entries.Count];

            for (int i = 0; i < entries.Count; i++) {
                entriesDict[i] = new { term = entries[i].TermNumber, index = entries[i].Index, command = entries[i].Command};
            }

            var req = new Dictionary<string, string>
            {
               { "term", term.ToString() },
               { "leaderId", leaderId.ToString() },
               { "prevLogIndex", prevLogIndex.ToString() },
               { "prevLogTerm", prevLogTerm.ToString() },
               { "entries", entriesDict.ToString() },
               { "leaderCommit", leaderCommit.ToString() }
            };

            var content = new FormUrlEncodedContent(req);

            var response = await client.PostAsync(baseURL + "appendentries", content);

            var responseString = await response.Content.ReadAsStringAsync();

            return responseString;
        }

        private async void SendTestConnection() {
            var response = await client.GetAsync(baseURL + "test");
        }

        private Result<bool> ParseResultFromJSON(string res) {
        	// Assumes valid input. One of:
        	// {"term":22,"value":false}
        	// {"value":true,"term":1}
            var sep = res.Split(",");
            string strTerm;
            string strValue;
            
            if (sep[0][sep[0].Length - 1] == 'e') {
            	strValue = sep[0];
            	strTerm = sep[1];
            }
            else {
	            strTerm = sep[0];
	            strValue = sep[1];
            }
            strTerm.Trim('{');
            strTerm.Trim('}');
            strValue.Trim('{');
            strValue.Trim('}');

            int resultTerm;
            bool resultValue;
			
			if (strValue[8] == 'f') {
				resultValue = false;
			}
			else {
				resultValue = true;
			}

			resultTerm = int.Parse(strTerm.Substring(6));

			return new Result<bool>(resultValue, resultTerm);
        }
    }
}
