using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using RaftCore.Components;

namespace RaftCore.Connections {
    /// <summary>
    /// Represents a cluster of Raft nodes. Contains one <see cref="IRaftConnector"/> for each node.
    /// </summary>
    public class RaftCluster {
        private List<IRaftConnector> nodes = new List<IRaftConnector>();

        /// <summary>
        /// Number of nodes in the cluster.
        /// </summary>
        public int Size { 
            get {
                return nodes.Count;
            }
        }

        /// <summary>
        /// Adds a node's <see cref="IRaftConnector"/> to the cluster.
        /// </summary>
        /// <param name="node"></param>
        public void AddNode(IRaftConnector node) {
            this.nodes.Add(node);
        }

        /// <summary>
        /// Redirects a user request to a specified node.
        /// </summary>
        /// <param name="command">User request to redirect</param>
        /// <param name="leaderId">Node to redirect the request to</param>
        public void RedirectRequestToNode(string command, uint? leaderId) {
            nodes.Find(x => x.NodeId == leaderId.Value).MakeRequest(command);
        }

        /// <summary>
        /// Randomly calculates the election timeout for a node in the cluster.
        /// This calculation is based on the return value of <see cref="CalculateBroadcastTimeMS"/>
        /// The election timeout will be a random number between 12 and 24 times that of the broadcast time.
        /// </summary>
        /// <returns>A randomized election timeout appropiate to the cluster size and characteristics</returns>
        public int CalculateElectionTimeoutMS() {
            int broadcastTime = CalculateBroadcastTimeMS();
            Random rand = new Random();
            // Ensures the election timeout is one order of magnitude bigger than the broadcast time
            return rand.Next(broadcastTime * 12, broadcastTime * 24);
        }

        /// <summary>
        /// Calculates the broadcast time: That is, the average time it takes a server to:
        /// <list type="number">
        /// <item>
        /// <description>send RPCs in parallel to every server in the cluster</description>
        /// </item>
        /// <item>
        /// <description>receive their responses.</description>
        /// </item>
        /// </list>
        /// The method uses the node's <see cref="RaftNode.TestConnection"/>
        /// The broadcast time will always be at least 25 ms.
        /// </summary>
        /// <returns>The estimated broadcast time of the cluster, or 25. Whichever is higher.</returns>
        public int CalculateBroadcastTimeMS() {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Restart();
                
            Parallel.ForEach(nodes, x => x.TestConnection());
                
            stopWatch.Stop();

            int elapsedMS = (int) stopWatch.ElapsedMilliseconds;

            return Math.Max(25, elapsedMS);
        }

        /// <summary>
        /// Returns the ids of all the nodes in the cluster except for the specified one.
        /// </summary>
        /// <param name="nodeId">Node ID to exclude</param>
        /// <returns>List of node IDs without the specified one</returns>
        public List<uint> GetNodeIdsExcept(uint nodeId) {
            return nodes.Where(x => x.NodeId != nodeId).Select(x => x.NodeId).ToList();
        }

        /// <summary>
        /// Sends the AppendEntry message to the specified <see cref="IRaftConnector"/>
        /// </summary>
        /// <param name="nodeId">Node that will receive the request</param>
        /// <param name="term">Leader's current term number</param>
        /// <param name="leaderId">ID of the node invoking this method</param>
        /// <param name="prevLogIndex">Index of log immediately preceding new ones</param>
        /// <param name="prevLogTerm">Term of prevLogIndex entry</param>
        /// <param name="entries">List of entries sent to be replicated. null for heartbeat</param>
        /// <param name="leaderCommit">Leader's CommitIndex</param>
        /// <returns>Returns a Result object containing the current term of the node and whether the request worked</returns>
        public Result<bool> SendAppendEntriesTo(uint nodeId, int term, uint leaderId, int prevLogIndex, 
                                       int prevLogTerm, List<LogEntry> entries, int leaderCommit) {
            return nodes.Find(x => x.NodeId == nodeId).AppendEntries(term, leaderId, prevLogIndex, 
                                       prevLogTerm, entries, leaderCommit);
        }

        /// <summary>
        /// Sends the RequestVote message to the specified <see cref="IRaftConnector"/>
        /// </summary>
        /// <param name="nodeId">Node that will receive the request</param>
        /// <param name="term">Term of the candidate</param>
        /// <param name="candidateId">Node ID of the candidate</param>
        /// <param name="lastLogIndex">Index of candidate's last log entry</param>
        /// <param name="lastLogTerm">Term of candidate's last log entry</param>
        /// <returns>Returns a Result object containing the current term of the node and whether it grants the requested vote</returns>
        public Result<bool> RequestVoteFrom(uint nodeId, int term, uint candidateId, int lastLogIndex, int lastLogTerm) {
            return nodes.Find(x => x.NodeId == nodeId).RequestVote(term, candidateId, lastLogIndex, lastLogTerm);
        }
    }
}
