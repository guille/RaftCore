using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace EasyRaft.Connections {
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

        // Returns votes obtained
        public int RequestVotesFromAll(int term, uint candidateId, int lastLogIndex, int lastLogTerm) {
            return nodes.AsParallel().Where(x => x.NodeId != candidateId).Count(x => x.RequestVote(term, candidateId, lastLogIndex, lastLogTerm));
        }

        public void SendHeartbeats(int term, uint leaderId, int commitIndex) {
            // IMPORTANT: No heartbeats to self
            Parallel.ForEach(nodes, x =>
                {
                    if (x.NodeId != leaderId)
                        x.AppendEntries(term, leaderId, 0, 0, null, commitIndex);
                }
            );
        }
        
        public int CalculateElectionTimeoutMS() {
            // TODO: move to a constructor?
            int broadcastTime = Math.Max(50, CalculateBroadcastTimeMS());
            Random rand = new Random(); // TODO: Change seed? broadcast * current time?
            // Ensures the election timeout is one order of magnitude bigger than the broadcast time
            return rand.Next(broadcastTime * 12, broadcastTime * 69);
        }

        // TODO: Broadcast time depends on the state machine chosen.
        // A memory hashmap will perform faster than a relational database.
        // This method doesn't take that time into account, just the network travel time
        // TODO: It's always 0 now
        public int CalculateBroadcastTimeMS() {
            long[] times = new long[nodes.Count];
            int i = 0;
            Stopwatch stopWatch = new Stopwatch();
            // Takes an average of clusterSize measures
            foreach (var node in nodes) {
                stopWatch.Start();
                stopWatch.Reset();
                
                // -1 in term assures fast reply and no state change
                Parallel.ForEach(nodes, x => x.AppendEntries(-1, 0, 0, 0, null, 0));
                
                stopWatch.Stop();

                times[i] = stopWatch.ElapsedMilliseconds;
                i++;

            }
            // measure how much it takes to send and receive a request
            // to each node,& return the average
            return (int) times.Average();
        }

        public List<uint> GetNodeIdsExcept(uint nodeId) {
            return nodes.Where(x => x.NodeId != nodeId).Select(x => x.NodeId).ToList();
        }
        
        public bool SendAppendEntriesTo(uint nodeId, int term, uint leaderId, int prevLogIndex, 
                                       int prevLogTerm, List<LogEntry> entries, int leaderCommit) {
            

            // use await somewhere here?
            // do await on majority so the execution continues, but keep the other appendrequests retrying
            return nodes.Find(x => x.NodeId == nodeId).AppendEntries(term, leaderId, prevLogIndex, 
                                       prevLogTerm, entries, leaderCommit);

            // new method in raftnode, called from makerequest --> SendAppendEntries
            // calls this method to send append entries
            // depending on what it returns:
            // if this method returns true, add 1 to some counter (careful race conditions)
            // if this method returns false, decrement nextindex (careful race conditions) and try again
            // when said counter hits GetMajority, apply stuff
            // this new method should be called synchronously??
            // if it's asynchronous, a new request could get applied before?
            
        }
    }
}
