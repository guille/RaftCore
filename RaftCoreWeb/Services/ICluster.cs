using RaftCore;
using RaftCore.StateMachine.Implementations;
using RaftCore.Connections;
using RaftCore.Connections.Implementations;

namespace RaftCoreWeb.Services {
    public interface ICluster {
        RaftNode GetNode(uint id);
        RaftCluster GetCluster();
    }
}