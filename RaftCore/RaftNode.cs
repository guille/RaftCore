// #define DEBUG
#undef DEBUG
// #define SIM
#undef SIM

using RaftCore.StateMachine;
using RaftCore.Connections;
using RaftCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace RaftCore {
    /// <summary>
    /// Possible states a node can be in
    /// </summary>
    public enum NodeState {
        /// <summary>
        /// Represents the node is in leader state, as defined by the Raft specification
        /// </summary>
        Leader,
        /// <summary>
        /// Represents the node is in follower state, as defined by the Raft specification
        /// </summary>
        Follower,
        /// <summary>
        /// Represents the node is in candidate state, as defined by the Raft specification
        /// </summary>
        Candidate,
        /// <summary>
        /// Represents the node has been stopped (see <see cref="RaftNode.Stop"/> and won't respond to RPCs.
        /// </summary>
        Stopped
    };

    /// <summary>
    /// Represents a node as defined by Raft's algorithm.
    /// </summary>
    public class RaftNode {
    	// TODO: Some of these should be stored in non-volatile storage
        /// <summary>
        /// Unsigned integer uniquely representing a node.
        /// </summary>
        public uint NodeId { get; }

        /// <summary>
        /// State machine this node replicates.
        /// </summary>
        public IRaftStateMachine StateMachine { get; private set; }
        /// <summary>
        /// Represents the cluster this node is part of.
        /// </summary>
        public RaftCluster Cluster { get; private set; }
        /// <summary>
        /// Log of entries to replicate.
        /// </summary>
        public List<LogEntry> Log { get; private set; }

        /// <summary>
        /// Current state of the node.
        /// </summary>
        public NodeState NodeState { get; private set; } = NodeState.Stopped;

        /// <summary>
        /// ID of the node identified as leader, if any.
        /// </summary>
        public uint? LeaderId { get; private set; } = null;
        /// <summary>
        /// ID of the candidate that received vote in current term, if any.
        /// </summary>
        public uint? VotedFor { get; private set; } = null;
        /// <summary>
        /// Index of highest lof entry known to be committed.
        /// </summary>
        public int CommitIndex { get; private set; } = -1;
        /// <summary>
        /// Index of highest log entry applied to state machine.
        /// </summary>
        public int LastApplied { get; private set; } = -1;

        /// <summary>
        /// The node's election timeout in milliseconds.
        /// </summary>
        public int ElectionTimeoutMS { get; private set; }
        private Timer electionTimer;
        private Timer heartbeatTimer;

        // Leaders' state
        /// <summary>
        /// For each node, index of next log entry to send to that node.
        /// </summary>
        public Dictionary<uint, int> NextIndex { get; }
        /// <summary>
        /// For each node, index of highest log entry known to be replicated on that node.
        /// </summary>
        public Dictionary<uint, int> MatchIndex { get; }

        private int currentTerm = 0;
        /// <summary>
        /// Latest term the node has seen
        /// </summary>
        public int CurrentTerm {
            get {
                return currentTerm;
            }
            private set {
                if (value > currentTerm) {
                    currentTerm = value;
                    LeaderId = null;
                    VotedFor = null;
                    NodeState = NodeState.Follower;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RaftNode"/> class.
        /// </summary>
        /// <param name="nodeId">The node's ID</param>
        /// <param name="stateMachine"><see cref="IRaftStateMachine"/> to replicate</param>
        public RaftNode(uint nodeId, IRaftStateMachine stateMachine) {
            this.NodeId = nodeId;
            this.StateMachine = stateMachine;
            this.Log = new List<LogEntry>();

            electionTimer = new Timer(TriggerElection);
            heartbeatTimer = new Timer(SendHeartbeats);

            NextIndex = new Dictionary<uint, int>();
            MatchIndex = new Dictionary<uint, int>();
        }

        /// <summary>
        /// Configures the node: adds the cluster object, calculates the election timeout and sets the initial state to Follower.
        /// </summary>
        /// <param name="cluster"><see cref="RaftCluster"/> instance containing all the nodes in the cluster</param>
        public void Configure(RaftCluster cluster) {
            this.Cluster = cluster;
            this.ElectionTimeoutMS = Cluster.CalculateElectionTimeoutMS();
            this.NodeState = NodeState.Follower;
        }

        /// <summary>
        /// Invoked after a change of state, or to start the node's execution for the first time.
        /// Initializes the timers and state appropiate to the node's state.
        /// </summary>
        public void Run() {
            if (Cluster == null) {
                throw new InvalidOperationException("You must configure the cluster before you run it.");
            }
            switch(this.NodeState) {
                case NodeState.Candidate:
                    StopHeartbeatTimer();
                    ResetElectionTimer();
                    StartElection();
                    break;
                case NodeState.Leader:
                    StopElectionTimer();
                    ResetHeartbeatTimer();
                    ResetLeaderState();
                    break;
                case NodeState.Follower:
                    StopHeartbeatTimer();
                    ResetElectionTimer();
                    break;
                case NodeState.Stopped:
                    StopHeartbeatTimer();
                    StopElectionTimer();
                    break;
            }
        }


        /// <summary>
        /// Invoked by a leader node to replicate log entries or send heartbeats.
        /// </summary>
        /// <param name="term">Leader's current term number</param>
        /// <param name="leaderId">ID of the node invoking this method</param>
        /// <param name="prevLogIndex">Index of log immediately preceding new ones</param>
        /// <param name="prevLogTerm">Term of prevLogIndex entry</param>
        /// <param name="entries">List of entries sent to be replicated. null for heartbeat</param>
        /// <param name="leaderCommit">Leader's CommitIndex</param>
        /// <returns>Returns a Result object containing the current term of the node and whether the request worked</returns>
        public Result<bool> AppendEntries(int term, uint leaderId, int prevLogIndex, int prevLogTerm, 
                                  List<LogEntry> entries, int leaderCommit) {
            if (NodeState == NodeState.Stopped) {
                return new Result<bool>(false, CurrentTerm);
            }
            if (term < this.CurrentTerm) {
                LogMessage("Received AppendEntries with outdated term. Declining.");
                return new Result<bool>(false, CurrentTerm);
            }

            if (entries != null && Log.Count > 0 && Log[prevLogIndex].TermNumber != prevLogTerm) {
                // log doesn’t contain an entry at prevLogIndex
                // whose term matches prevLogTerm
                return new Result<bool>(false, CurrentTerm);
            }

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

                LogMessage("Node " + NodeId + " appending new entry " + entries[0].Command);
            }
            else { // HEARTBEAT
                LogMessage("Node " + NodeId + " received heartbeat from " + leaderId);
            }


            if (leaderCommit > CommitIndex) {
                //TODO: It gets here on heartbeats
                LogMessage("Node " + NodeId + " applying entries");
                // Instead of doing maths with leaderCommit and CommitIndex, could:
                // If commitIndex > lastApplied:
                // increment lastApplied, apply log[lastApplied] to state machine
                var toApply = Log.Skip(CommitIndex + 1).Take(leaderCommit - CommitIndex).ToList();

                if (toApply.Count == 0) {
                    LogMessage("Node " + NodeId + " failed applying entries");
                    return new Result<bool>(false, CurrentTerm);
                }

                toApply.ForEach(x => StateMachine.Apply(x.Command));

                CommitIndex = Math.Min(leaderCommit, Log[Log.Count - 1].Index);
                
                LastApplied = CommitIndex;
            }

            return new Result<bool>(true, CurrentTerm);
        }

        /// <summary>
        /// Invoked by candidates to request a vote from this node.
        /// It uses the parameters provided to check whether the candidate is more up to date than this node.
        /// </summary>
        /// <param name="term">Term of the candidate</param>
        /// <param name="candidateId">Node ID of the candidate</param>
        /// <param name="lastLogIndex">Index of candidate's last log entry</param>
        /// <param name="lastLogTerm">Term of candidate's last log entry</param>
        /// <returns>Returns a Result object containing the current term of the node and whether it grants the requested vote</returns>
        public Result<bool> RequestVote(int term, uint candidateId, int lastLogIndex, int lastLogTerm) {
            if (NodeState == NodeState.Stopped) return new Result<bool>(false, CurrentTerm);
            LogMessage("Node " + candidateId + " is requesting vote from node " + NodeId);

            bool voteGranted = false;
            if (term < CurrentTerm) {
                return new Result<bool>(voteGranted, CurrentTerm);
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

            return new Result<bool>(voteGranted, CurrentTerm);
        }

        /// <summary>
        /// Called by a client to make a request to the node.
        /// If the node is the leader, appends the entry to its log.
        /// Otherwise, it waits until it finds a leader and redirects the request to them.
        /// The request is dropped if it can't find a leader
        /// </summary>
        /// <param name="command">String forming a command recognisable by the state machine</param>
        public void MakeRequest(String command) {
            if (NodeState == NodeState.Leader) {
                LogMessage("This node is the leader");
                
                var entry = new LogEntry(CurrentTerm, Log.Count, command);
                Log.Add(entry);
            }
            else if (NodeState == NodeState.Follower && LeaderId.HasValue) {
                LogMessage("Redirecting to leader " + LeaderId + " by " + NodeId);
                Cluster.RedirectRequestToNode(command, LeaderId);
            }
            else {
                LogMessage("Couldn't find a leader. Dropping request.");
            }
        }

        /// <summary>
        /// Determines the elements in the log that have been committed, as far as the node knows.
        /// </summary>
        /// <returns>List of entries in the log known to be committed.</returns>
        public List<LogEntry> GetCommittedEntries() {
            return Log.Take(CommitIndex + 1).ToList();
        }

        // **********************
        // *  INTERNAL METHODS  *
        // **********************

        private void StartElection() {
            CurrentTerm++;

            // Vote for self
            var voteCount = 1;
            VotedFor = NodeId;

            // Start election
            LogMessage("A node has started an election: " + NodeId + " (term " + CurrentTerm + ")");

            var nodes = Cluster.GetNodeIdsExcept(NodeId);
            int votes = 0;

            Parallel.ForEach(nodes, nodeId => 
            {
                var res = Cluster.RequestVoteFrom(nodeId, CurrentTerm, NodeId, Log.Count - 1, GetLastLogTerm());

                CurrentTerm = res.Term;

                if (res.Value) {
                    Interlocked.Increment(ref votes);
                }
            });
            voteCount += votes;

            if (voteCount >= GetMajority()) {
                LogMessage("New leader!! : " + NodeId + " with " + voteCount + " votes");
                LeaderId = NodeId;
                NodeState = NodeState.Leader;
                Run();
            }
        }

        private void ResetLeaderState() {
            NextIndex.Clear();
            MatchIndex.Clear();

            Cluster.GetNodeIdsExcept(NodeId).ForEach(x => {
                NextIndex[x] = Log.Count;
                MatchIndex[x] = 0;
            });
        }

        /// <summary>
        /// Changes the node's state to Follower if it was stopped. It does nothing otherwise.
        /// </summary>
        public void Restart() {
            if (NodeState == NodeState.Stopped) {
                LogMessage("Restarting node " + NodeId);
                NodeState = NodeState.Follower;
                Run();
            }
        }

        /// <summary>
        /// Changes the node's state to Stopped. A node won't accept RPCs while in this state,
        /// </summary>
        public void Stop() {
            if (NodeState != NodeState.Stopped) {
                LogMessage("Bringing node " + NodeId + " down");
                NodeState = NodeState.Stopped;
                Run();
            }
        }

        /// <summary>
        /// Calculates the minimum number of nodes that form a majority.
        /// </summary>
        /// <returns>Number of nodes in the cluster representing a quorum</returns>
        private int GetMajority() {
            double n = (Cluster.Size + 1) / 2;
            return (int) Math.Ceiling(n);
        }

        private void TriggerElection(object arg) {
            NodeState = NodeState.Candidate;
            #if (SIM)
            Thread.Sleep(150);
            #endif
            Run();
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
                heartbeatTimer.Change(0, ElectionTimeoutMS/2);
            }
        }

        /// <summary>
        /// Calculates the term of the last log entry.
        /// </summary>
        /// <returns>Term of node's last log entry. If the log is empty, returns 0</returns>
        private int GetLastLogTerm() {
            return (Log.Count > 0) ? Log[Log.Count - 1].TermNumber : 0;
        }

        /// <summary>
        /// Triggered by the heartbeatTimer. Sends AppendEntries requests in parallel to all the nodes in the cluster.
        /// If there are unreplicated entries, sends them in the request. Sends a simple heartbeat otherwise.
        /// It also checks MatchIndex for any entries replicated in the majority of nodes, and commits them.
        /// </summary>
        /// <param name="arg">Sent by System.Threading.Timer</param>
        private void SendHeartbeats(object arg) {
            var nodes = Cluster.GetNodeIdsExcept(NodeId);

            Parallel.ForEach(nodes, nodeId => 
            {
                if (!NextIndex.ContainsKey(nodeId)) return; // Prevents errors when testing
                var prevLogIndex = Math.Max(0, NextIndex[nodeId] - 1);
                int prevLogTerm = (Log.Count > 0) ? prevLogTerm = Log[prevLogIndex].TermNumber : 0;

                List<LogEntry> entries;

                if (Log.Count > NextIndex[nodeId]) {
                    LogMessage("Log Count: " + Log.Count + " -- Target node[nextIndex]: " + nodeId + " [" + NextIndex[nodeId] + "]");
                    entries = Log.Skip(NextIndex[nodeId]).ToList();
                }
                else {
                    // covers Log is empty or no new entries to replicate
                    entries = null;
                }

                var res = Cluster.SendAppendEntriesTo(nodeId, CurrentTerm, NodeId, prevLogIndex, prevLogTerm, entries, CommitIndex);

                CurrentTerm = res.Term;

                if (res.Value) {
                    if (entries != null) {
                        // Entry appended
                        LogMessage("Successful AE to " + nodeId + ". Setting nextIndex to " + NextIndex[nodeId]);
                        NextIndex[nodeId] = Log.Count;
                        MatchIndex[nodeId] = Log.Count - 1;
                    }
                }
                else {
                    LogMessage("Failed AE to " + nodeId + ". Setting nextIndex to " + NextIndex[nodeId]);
                    // Entry failed to be appended
                    if (NextIndex[nodeId] > 0) {
                        NextIndex[nodeId]--;
                    }

                }
            });

            // TODO: Do this as new task?
            // Iterate over all uncommitted entries
            for(int i = CommitIndex + 1; i < Log.Count; i++) {
                // We add 1 because we know the entry is replicated in this node
                var replicatedIn = MatchIndex.Values.Count(x => x >= i) + 1;
                if (Log[i].TermNumber == CurrentTerm && replicatedIn >= GetMajority()) {
                    CommitIndex = i;
                    StateMachine.Apply(Log[i].Command);
                    LastApplied = i;
                }
            }
            // (responder a client request)

        }

        /// <summary>
        /// Used by the cluster to calculate the broadcast time.
        /// Makes a dummy request to the state machine.
        /// </summary>
        internal void TestConnection() {
            StateMachine.TestConnection();
        }

        /// <summary>
        /// Returns a readable representation of the node, containing its state and id..
        /// </summary>
        /// <returns>String representation of the node.</returns>
        public override string ToString() {
            string state;
            if (NodeState == NodeState.Follower)
                state = "Follower (of " + LeaderId + ")";
            else
                state = NodeState.ToString();
            return "Node (" + NodeId + ") -- " + state;
        }

        private void LogMessage(string msg) {
            #if (DEBUG)
            Console.WriteLine(msg);
            #endif  
        }

    }
}
