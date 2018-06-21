namespace RaftCore.StateMachine {
    /// <summary>
    /// Represents a state machine for replication.
    /// </summary>
    public interface IRaftStateMachine {
        /// <summary>
        /// Applies a command to the state machine. The State Machine will silently ignore bad commands.
        /// </summary>
        /// <param name="command">Command to apply</param>
        void Apply(string command);

        /// <summary>
        /// Finds the string representation of the state machine, limited by the given command.
        /// </summary>
        /// <param name="param">Limits, filters or specifies the information to retrieve.</param>
        /// <returns>A string containing the specified representation of the state machine</returns>
        string RequestStatus(string param);

        /// <summary>
        /// Used to determine the broadcast time.
        /// The method must implement a dummy request to the state machine, in order to correctly approximate the broadcast time.
        /// </summary>
        void TestConnection();
    }
}
