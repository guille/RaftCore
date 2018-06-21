using System;

namespace RaftCore.StateMachine.Implementations {
    /// <summary>
    /// Represents a number as a state machine, applying deltas (additions and substractions to it).
    /// </summary>
    public class NumeralStateMachine : IRaftStateMachine {
        private int state = 0;

        /// <summary>
        /// Transforms the given command into an integer and applies it to the state machine.
        /// For example, a command of "10" or "+10" would add 10 to the current state
        /// while a command of "-5" would substract 5 from it.
        /// Silently ignores bad requests
        /// </summary>
        /// <param name="command">String containing the number to apply</param>
        public void Apply(String command) {
            try {
                var delta = int.Parse(command);
                state += delta;
            } catch (System.FormatException) {
                ; // Don't apply bad requests
            }

        }

        /// <summary>
        /// Returns the current state of this state machine as a string.
        /// </summary>
        /// <param name="param">This parameter is ignored in this implementation.</param>
        /// <returns>The current state, i.e. the number it represents</returns>
        public string RequestStatus(string param) {
            return state.ToString();
        }
        
        /// <summary>
        /// Used to determine the broadcast time.
        /// The method parses a string into an integer and adds it to a variable.
        /// </summary>
        public void TestConnection() {
            int testState = 0;
            testState += int.Parse("-1");
        }

    }
}
