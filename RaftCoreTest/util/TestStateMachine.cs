using System;
using System.Collections.Generic;
using EasyRaft.StateMachine;

namespace EasyRaftTest.util {
    class TestStateMachine : EasyRaftStateMachine {
        Dictionary<string, int> state =
            new Dictionary<string, int>();

        public void ExecuteCommand(String command) {
            command = command.ToUpper();
            var commands = command.Split(" ");
            switch(commands[0]) {
                // SET X Y
                case "SET":
                    state[commands[1]] = int.Parse(commands[2]);
                    break;
                // CLEAR X
                case "CLEAR":
                    state.Remove(commands[1]);
                    break;
                // TODO: unknown command
                default:
                    break;
            }
        }

        public int RequestStatus(string param) {
            if (state.ContainsKey(param)) {
                return state[param];
            }
            else {
                return -1;
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
