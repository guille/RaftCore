namespace RaftCore.Components {
    /// <summary>
    /// Represents a node's log entry.
    /// </summary>
    public class LogEntry {
        /// <summary>
        /// Term in which the entry was received by the leader
        /// </summary>
        public int TermNumber { get; }

        /// <summary>
        /// Index in the log of the entry.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Command associated with the entry.
        /// </summary>
        public string Command { get; }

        /// <summary>
        /// Initializes a log entry given a term number, index and a command
        /// </summary>
        /// <param name="termNumber">Parameter representing the entry's term</param>
        /// <param name="index">Parameter representing the entry's index</param>
        /// <param name="command">Parameter representing the entry's command</param>
        public LogEntry(int termNumber, int index, string command) {
            this.TermNumber = termNumber;
            this.Index = index;
            this.Command = command;
        }
    }
}
