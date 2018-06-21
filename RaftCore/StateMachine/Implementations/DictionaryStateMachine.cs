using System;
using System.Collections.Generic;
using RaftCore.StateMachine;

namespace RaftCore.StateMachine.Implementations {
    /// <summary>
    /// Represents a dictionary of string to ints as a state machine.
    /// </summary>
    public class DictionaryStateMachine : IRaftStateMachine {
        Dictionary<string, int> state =
            new Dictionary<string, int>();

        /// <summary>
        /// Parses the command and applies it to the state machine.
        /// This implementation recognises two commands:
        /// <list type="bullet">
        /// <item>
        /// <description>SET X Y: Adds a key-value pair (X,Y) to the dictionary</description>
        /// </item>
        /// <item>
        /// <description>CLEAR X: Removes the element with key X from the dictionary.</description>
        /// </item>
        /// </list>
        /// The commands (set/clear) are case insensitive.
        /// The keys represented with X above are case sensitive.
        /// It silently ignores bad requests.
        /// </summary>
        /// <param name="command">String containing the command to apply</param>
        public void Apply(String command) {
            var commands = command.Split(" ");
            try {
                switch(commands[0].ToUpper()) {
                    // SET X Y
                    case "SET":
                        state[commands[1]] = int.Parse(commands[2]);
                        break;
                    // CLEAR X
                    case "CLEAR":
                        if (state.ContainsKey(commands[1])) {
                            state.Remove(commands[1]);
                        }
                        break;
                    default:
                        break;
                }
            } catch (System.FormatException) {
                ; // Don't apply bad requests
            }
        }

        /// <summary>
        /// Queries the dictionary for a given key.
        /// </summary>
        /// <param name="param">Key to lookup</param>
        /// <returns>Returns the value associated with the given key or the empty string if there are none.</returns>
        public string RequestStatus(string param) {
            if (state.ContainsKey(param)) {
                return state[param].ToString();
            }
            else {
                return "";
            }
        }
        
        /// <summary>
        /// Used to determine the broadcast time.
        /// The method creates a dictionary and adds a key-value pair.
        /// </summary>
        public void TestConnection() {
            var testState = new Dictionary<string, int>();
            testState["X"] = int.Parse("0");
            testState.Clear();
        }

    }
}
