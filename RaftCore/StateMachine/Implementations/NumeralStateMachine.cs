using System;

namespace RaftCore.StateMachine.Implementations {
    public class NumeralStateMachine : IRaftStateMachine {
        int state = 0;

        public void Apply(String command) {
            try {
                var delta = int.Parse(command);
                state += delta;
            } catch (System.FormatException) {
                ; // Don't apply bad requests
            }
            
        }

        public string RequestStatus(string param) {
            return state.ToString();
        }

        public override String ToString() {
            return state.ToString();
        }

        public void TestConnection() {
            int testState = 0;
            testState += int.Parse("-1");
        }

    }
}
