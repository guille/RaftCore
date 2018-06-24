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
        [HttpGet("[controller]/{id}/log")]
        public JsonResult GetLog(int id) {
            /* Complex json, returning array of logentries
            * [
            *   {
            *       term:           1
            *       command:     "+2"
            *       committed:  false
            *   },
            *   {...}
            * ]
            */
            var node = _cluster.GetNode((uint)id);

            var log = node.Log;
            var commitIndex = node.CommitIndex;
            var res = new object[log.Count];

            for (int i = 0; i < log.Count; i++) {
                bool cm = i <= commitIndex;
                res[i] = new { term = log[i].TermNumber, command = log[i].Command, committed = cm};
            }
            return new JsonResult(res);
        }

        // GET /nodes/:id/sm
        [HttpGet("[controller]/{id}/sm")]
        public ActionResult<int> GetStateMachine(int id) {
            var st = _cluster.GetNode((uint)id).StateMachine.RequestStatus(null);
            return int.Parse(st);
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
