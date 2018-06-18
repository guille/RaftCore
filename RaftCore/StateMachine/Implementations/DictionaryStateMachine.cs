using System;
using System.Collections.Generic;
using RaftCore.StateMachine;

namespace RaftCore.StateMachine.Implementations {
    public class DictionaryStateMachine : RaftCoreStateMachine {
        Dictionary<string, int> state =
            new Dictionary<string, int>();

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

        public string RequestStatus(string param) {
            if (state.ContainsKey(param)) {
                return state[param].ToString();
            }
            else {
                return "";
            }
        }

        public override String ToString() {
            return state.ToString();
        }

        public void TestConnection() {
            var testState = new Dictionary<string, int>();
            testState["X"] = 0;
            testState.Clear();
        }

    }
}
