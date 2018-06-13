using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRaft.StateMachine {
    public interface EasyRaftStateMachine {
        void ExecuteCommand(String command);
        int RequestStatus(string param);
        void TestConnection();
    }
}
