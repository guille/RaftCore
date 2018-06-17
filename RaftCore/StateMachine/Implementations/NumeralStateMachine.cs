using System;

namespace RaftCore.StateMachine.Implementations {
    public class NumeralStateMachine : RaftCoreStateMachine {
        int state = 0;

        public void Apply(String command) {
            var delta = int.Parse(command);
            state += delta;
            //TODO: Rescue parse error
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
