using EasyRaft.StateMachine;
using EasyRaft.Connections;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;

namespace EasyRaft {
    public enum NodeState { Leader, Follower, Candidate, Stopped };

    public class RaftNode {
    	// TODO: Some of these should be stored in non-volatile storage
        public uint NodeId { get; }

        public EasyRaftStateMachine StateMachine { get; private set; }
        public RaftCluster Cluster { get; private set; }
        public List<LogEntry> Log { get; private set; }

        public NodeState NodeState { get; private set; } = NodeState.Stopped;

        public uint? LeaderId { get; private set; } = null;
        public uint? VotedFor { get; private set; } = null;
        public int VoteCount { get; private set; } = 0;
        public int CommitIndex { get; private set; } = -1;
        public int LastApplied { get; private set; } = 0;

        public int ElectionTimeoutMS { get; private set; } // 150-300ms
        private Timer electionTimer;
        private Timer heartbeatTimer;

        // Leaders' state
        public Dictionary<uint, int> NextIndex { get; }
        public Dictionary<uint, int> MatchIndex { get; }

        private int currentTerm = 0;
        public int CurrentTerm {
            get {
                return currentTerm;
            }
            // Resets LeaderId, VoteCount and VotedFor when it's a term increase
            // Updates current term to the given value
            private set {
                if (value > currentTerm) {
                    currentTerm = value;
                    LeaderId = null;
                    VoteCount = 0;
                    VotedFor = null;
                    NodeState = NodeState.Follower;
                }
            }
        }

        // ******************************************
        // *  Initialization/configuration methods  *
        // ******************************************

        public RaftNode(uint nodeId, EasyRaftStateMachine stateMachine) {
            this.NodeId = nodeId;
            this.StateMachine = stateMachine;
            this.Log = new List<LogEntry>();

            electionTimer = new Timer(StartElection);
            heartbeatTimer = new Timer(SendHeartbeats);

            NextIndex = new Dictionary<uint, int>();
            MatchIndex = new Dictionary<uint, int>();
        }

        public void Configure(RaftCluster cluster) {
            this.Cluster = cluster;
            this.ElectionTimeoutMS = Cluster.CalculateElectionTimeoutMS();
            this.NodeState = NodeState.Follower;
        }


        public void Run() {
            switch(this.NodeState) {
                case NodeState.Candidate:
                    CandidateLoop();
                    break;
                case NodeState.Leader:
                    LeaderLoop();
                    break;
                case NodeState.Follower:
                    FollowerLoop();
                    break;
                case NodeState.Stopped:
                    StoppedLoop();
                    break;
            }
        }

        
        // Invoked by leader to replicate log entries; also used as heartbeat.
        // Receiver implementation
        // should term be out param?
        public bool AppendEntries(int term, uint leaderId, int prevLogIndex, int prevLogTerm, 
                                  List<LogEntry> entries, int leaderCommit) {
            if (NodeState == NodeState.Stopped) return false;
            if (term < this.CurrentTerm) return false;

            if (Log.Count > 0 && prevLogIndex >= Log.Count) return false; // So it doesn't throw an exception right below
            if (entries != null && Log.Count > 0 && Log[prevLogIndex].TermNumber != prevLogTerm) return false;

            // If we get to here it means the one sending us a message is leader
            StopHeartbeatTimer();
            ResetElectionTimer();
            CurrentTerm = term;

            NodeState = NodeState.Follower;
            LeaderId = leaderId;

            if (entries != null)  {
                // If an existing entry conflicts with a new one (same index
                // but different terms), delete the existing entry and all that
                // follow it (§5.3)
                Log = Log.Take(entries[0].Index).ToList();

                // Append any new entries not already in the log
                Log.AddRange(entries);

                Console.WriteLine("Node " + NodeId + " appending new entry");
            }
            else { // HEARTBEAT
                Console.WriteLine("Node " + NodeId + " received heartbeat from " + leaderId);
            }


            if (leaderCommit > CommitIndex) {
                Console.WriteLine("Node " + NodeId + " applying entries");
                // Instead of doing maths with leaderCommit and CommitIndex, could:
                // If commitIndex > lastApplied:
                // increment lastApplied, apply log[lastApplied] to state machine
                var toApply = Log.Skip(CommitIndex + 1).Take(leaderCommit - CommitIndex).ToList();
                toApply.ForEach(x => Console.WriteLine(x.Command));
                toApply.ForEach(x => StateMachine.ExecuteCommand(x.Command));

                CommitIndex = Math.Min(leaderCommit, Log[Log.Count - 1].Index);
                
                LastApplied = CommitIndex;
            }

            return true;
        }

        // Invoked by candidates to request a vote.
        // Return value of true means candidate received vote
        public bool RequestVote(int term, uint candidateId, int lastLogIndex, int lastLogTerm) {
            if (NodeState == NodeState.Stopped) return false;
            Console.WriteLine("Node " + candidateId + " is requesting vote from node " + NodeId);

            bool voteGranted = false;
            if (term < CurrentTerm) {
                return voteGranted;
            }

            StopHeartbeatTimer();
            ResetElectionTimer();
            CurrentTerm = term;

            if ((VotedFor == null || VotedFor == candidateId)
                && lastLogIndex >= Log.Count - 1
                && lastLogTerm >= GetLastLogTerm()) {
                voteGranted = true;
            }

            if (voteGranted) {
                VotedFor = candidateId;
            }

            return voteGranted;
        }


        // **************************
        // *  Called by the client  *
        // **************************
        public void MakeRequest(String command) {
            if (NodeState == NodeState.Leader) {
                Console.WriteLine("This node is the leader");
                
                var entry = new LogEntry(CurrentTerm, Log.Count, command);
                Log.Add(entry);

                // entry has been replicated: (i.e. replicated to majority of nodes)
                if (SendAppendEntries(entry)) {
                    StateMachine.ExecuteCommand(command);
                    LastApplied++;
                    CommitIndex++;
                }

                // TODO: return result of the execution
            }
            else {
                // Wait until there is a leader (maybe itself)
                // Then redirect them the request
                do {
                    Thread.Sleep(500);
                } while (!LeaderId.HasValue);
                uint leader = LeaderId.Value;
                Console.WriteLine("Redirecting to leader " + leader);
                Cluster.RedirectRequestToNode(command, leader);
            }
        }

        public List<LogEntry> GetCommittedEntries() {
            return Log.Take(CommitIndex + 1).ToList();
        }

        // **********************
        // *  INTERNAL METHODS  *
        // **********************

        private void CandidateLoop() {
            ResetElectionTimer();

            CurrentTerm++;
            LeaderId = null;

            // Vote for self
            VoteCount = 1;
            VotedFor = NodeId;

            // start election
            Console.Out.WriteLine("A node has started an election: " + NodeId + " (term " + CurrentTerm + ")");
            VoteCount += Cluster.RequestVotesFromAll(CurrentTerm, NodeId, Log.Count - 1, GetLastLogTerm());
            Console.Out.WriteLine(VoteCount);

            if (VoteCount >= GetMajority()) {
                Console.Out.WriteLine("Leader!! : " + NodeId);
                LeaderId = NodeId;
                NodeState = NodeState.Leader;
                Run();
            }
        }

        private void LeaderLoop() {
            StopElectionTimer();
            // Send heartbeats periodically
            ResetHeartbeatTimer();

            // reset nextIndex, matchIndex
            NextIndex.Clear();
            // Initialize all nextIndex to the index just after the last one in the log
            Cluster.GetNodeIdsExcept(NodeId).ForEach(x => NextIndex[x] = Log.Count);
            // MatchIndex.Clear();
        }

        private void FollowerLoop() {
            StopHeartbeatTimer();
            ResetElectionTimer();
        }

        private void StoppedLoop() {
            StopHeartbeatTimer();
            StopElectionTimer();
        }

        public void Start() {
            NodeState = NodeState.Follower;
            Run();
        }

        public void Stop() {
            NodeState = NodeState.Stopped;
            Run();
        }

        private int GetMajority() {
            double n = (Cluster.Size + 1) / 2;
            return (int) Math.Ceiling(n);
        }

        private void StartElection(object arg) {
            NodeState = NodeState.Candidate;
            Run();
        }

        private void SendHeartbeats(object arg) {
            Cluster.SendHeartbeats(CurrentTerm, NodeId, CommitIndex);
        }

        private void StopHeartbeatTimer() {
            heartbeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void StopElectionTimer() {
            electionTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void ResetElectionTimer() {
            if (NodeState != NodeState.Leader) {
                electionTimer.Change(ElectionTimeoutMS, ElectionTimeoutMS);
            }
        }

        private void ResetHeartbeatTimer() {
            if (NodeState == NodeState.Leader) {
                heartbeatTimer.Change(0, ElectionTimeoutMS/3);
            }
        }

        private int GetLastLogTerm() {
            int thisLastLogTerm = 0; // default 0
            if (Log.Count > 0) {
                thisLastLogTerm = Log[Log.Count - 1].TermNumber;
            }

            return thisLastLogTerm;
        }

        private bool SendAppendEntries(LogEntry entry) {
            var nodes = Cluster.GetNodeIdsExcept(NodeId);
            int successfulAppends = 0;
            // in parallel, and asynchronously:

            // If last log index ≥ nextIndex for a follower: send
            // AppendEntries RPC with log entries starting at nextIndex
            // If successful: update nextIndex and matchIndex for follower (§5.3)
            // If AppendEntries fails because of log inconsistency:
            // decrement nextIndex and retry (§5.3)

            nodes.ForEach( x => 
            {
                var entries = new List<LogEntry>() { entry };
                var prevLogIndex = Math.Max(0, NextIndex[x] - 1);
                if (Cluster.SendAppendEntriesTo(x, CurrentTerm, NodeId, prevLogIndex, Log[prevLogIndex].TermNumber, entries, CommitIndex))
                    Interlocked.Increment(ref successfulAppends);
                // TODO: Change into a do-while
                // if any return false, decrement nextindex and try again
                
            });

            return successfulAppends >= GetMajority();

        }

        public override string ToString() {
            string state;
            if (NodeState == NodeState.Follower)
                state = "Follower (of " + LeaderId + ")";
            else
                state = NodeState.ToString();
            return "Node (" + NodeId + ") -- " + state;
        }

    }
}
