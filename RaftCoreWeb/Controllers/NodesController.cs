#define SIM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RaftCoreWeb.Services;
using RaftCore;

namespace RaftCoreWeb.Controllers {

    [ApiController]
    public class NodesController : ControllerBase {
        private readonly ICluster _cluster;

        public NodesController(ICluster cluster) {
            _cluster = cluster;
        }

        // GET /nodes/states
        [HttpGet("[controller]")]
        public IEnumerable<IEnumerable<string>> GetNodesInfo() {
            // Returns terms and states
            var res = new List<IEnumerable<string>>();
            res.Add(_cluster.GetNodes().Select(x => x.CurrentTerm.ToString()));
            res.Add(_cluster.GetNodes().Select(x => x.NodeState.ToString()));
            return res;
        }

        // GET /nodes/:id/log
        [HttpGet("[controller]/log")]
        public JsonResult GetLog() {
            /* Complex json, returning array of node logs (containing arrays of logentries)
            * [
            *   [
            *      {
            *       term:           1
            *       command:     "+2"
            *       committed:  false
            *      },
            *      {...}
            *   ],
            *   [...]
            * ]
            */
            var res = new List<object>();

            foreach (var node in _cluster.GetNodes()) {
                var log = node.Log;
                var commitIndex = node.CommitIndex;
                var nodeLog = new object[log.Count];

                for (int i = 0; i < log.Count; i++) {
                    bool cm = i <= commitIndex;
                    nodeLog[i] = new { term = log[i].TermNumber, command = log[i].Command, committed = cm};
                }
                res.Add(nodeLog);
            }

            return new JsonResult(res);
        }

        // GET /nodes/sm
        [HttpGet("[controller]/sm")]
        public IEnumerable<string> GetStateMachines() {
            return _cluster.GetNodes().Select(x => x.StateMachine.RequestStatus(null));
            // var st = _cluster.GetNode((uint)id).StateMachine.RequestStatus(null);
            // return int.Parse(st);
        }

        // PATCH /nodes
        [HttpPatch("[controller]/{id}")]
        public string SwitchState(int id) {
            // Stòp/Restart node <id>
            var node = _cluster.GetNode((uint)id);
            if (node.NodeState == NodeState.Stopped) {
                node.Restart();
            }
            else {
                node.Stop();
            }
            return "";
        }

        // POST /nodes/requests
        [HttpPost("[controller]/requests")]
        public string MakeRequest() {
            foreach (var node in _cluster.GetNodes()) {
                if (node.NodeState == NodeState.Leader) {
                    node.MakeRequest(Request.Form["userRequest"].ToString());
                }
            }
            return "";
        }
    }
}
