using System;
using System.Collections.Generic;
using EasyRaft.StateMachine;

namespace EasyRaft.StateMachine.Implementations {
    public class NumeralStateMachine : RaftCoreStateMachine {
        int state = 0;

        public void Apply(String command) {
            var delta = int.Parse(command);
            state += delta;
            //TODO: Rescue parse error
        }

        public int RequestStatus(string param) {
            return state;
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
