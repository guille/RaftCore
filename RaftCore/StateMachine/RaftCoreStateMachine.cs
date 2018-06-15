using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRaft.StateMachine {
    public interface RaftCoreStateMachine {
        void Apply(String command);
        int RequestStatus(string param);
        void TestConnection();
    }
}
