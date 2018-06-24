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

        // GET /nodes/:id
        [HttpGet("[controller]/{id}")]
        public JsonResult GetNode(int id) {
            /* Returns all the relevant properties of the node in json:
            * {
            *    state: "Candidate",
            *    term: 0
            * }
            */
            var st = _cluster.GetNode((uint)id).NodeState.ToString();
            var termNumber = _cluster.GetNode((uint)id).CurrentTerm;
            var data = new { state = st, term = termNumber};
            return new JsonResult(data);
        }

        // GET /nodes/states
        [HttpGet("[controller]/states")]
        public IEnumerable<string> GetNodeStates() {
            return _cluster.GetNodes().Select(x => x.NodeState.ToString());
        }

        // GET /nodes/terms
        [HttpGet("[controller]/terms")]
        public IEnumerable<string> GetNodeTerms() {
            return _cluster.GetNodes().Select(x => x.CurrentTerm.ToString());
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
            var log = _cluster.GetNode((uint)id).Log;
            var commitIndex = _cluster.GetNode((uint)id).CommitIndex;
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
            var req = Request.Form["userRequest"].ToString();
            var node = _cluster.GetNode(1);
            node.MakeRequest(req);
            return "";
        }
    }
}
