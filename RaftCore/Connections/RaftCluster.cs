using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace RaftCore.Connections {
    public class RaftCluster {
        List<IRaftConnector> nodes = new List<IRaftConnector>();
        public int Size { 
            get {
                return nodes.Count;
            }
        }

        public void AddNode(IRaftConnector node) {
            this.nodes.Add(node);
        }

        public void RedirectRequestToNode(string command, uint? leaderId) {
            nodes.Find(x => x.NodeId == leaderId.Value).SendRequestToNode(command);
        }

        public int CalculateElectionTimeoutMS() {
            int broadcastTime = Math.Max(25, CalculateBroadcastTimeMS());
            Random rand = new Random(); // TODO: Change seed? broadcast * current time?
            // Ensures the election timeout is one order of magnitude bigger than the broadcast time
            return rand.Next(broadcastTime * 12, broadcastTime * 32);
        }

        // TODO: Broadcast time depends on the state machine chosen.
        // TODO: It's always 0 now
        // measure how much it takes to send and receive a request
        // to each node,& return the average
        public int CalculateBroadcastTimeMS() {
            long[] times = new long[nodes.Count];
            int i = 0;
            Stopwatch stopWatch = new Stopwatch();
            // Takes an average of clusterSize measures
            foreach (var node in nodes) {
                stopWatch.Restart();
                
                Parallel.ForEach(nodes, x => x.TestConnection());
                
                stopWatch.Stop();

                times[i] = stopWatch.ElapsedMilliseconds;

                i++;

            }
            return (int) times.Average();
        }

        public List<uint> GetNodeIdsExcept(uint nodeId) {
            return nodes.Where(x => x.NodeId != nodeId).Select(x => x.NodeId).ToList();
        }
        
        public Result<bool> SendAppendEntriesTo(uint nodeId, int term, uint leaderId, int prevLogIndex, 
                                       int prevLogTerm, List<LogEntry> entries, int leaderCommit) {
            return nodes.Find(x => x.NodeId == nodeId).AppendEntries(term, leaderId, prevLogIndex, 
                                       prevLogTerm, entries, leaderCommit);
        }

        public Result<bool> RequestVoteFrom(uint nodeId, int term, uint candidateId, int lastLogIndex, int lastLogTerm) {
            return nodes.Find(x => x.NodeId == nodeId).RequestVote(term, candidateId, lastLogIndex, lastLogTerm);
        }
    }
}
