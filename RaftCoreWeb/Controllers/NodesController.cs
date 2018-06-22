using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RaftCoreWeb.Services;

namespace RaftCoreWeb.Controllers {

    // [Route("[controller]")]
    [ApiController]
    public class NodesController : ControllerBase {
        private readonly ICluster _cluster;

        public NodesController(ICluster cluster) {
            _cluster = cluster;
        }

        // GET /nodes/:id
        [HttpGet("[controller]/{id}")]
        public ActionResult<IEnumerable<string>> GetNode(int id) {
            /* Returns all the relevant properties of the node in json:
            *      -  State (candidate, leader...)
            *      -  CurrentTerm
            */
            return new string[] { "candidate" };
        }

        // GET /nodes/:id/log
        [HttpGet("[controller]/{id}/log")]
        public ActionResult<IEnumerable<string>> GetLog(int id) {
            // Complex json, returning array of logentries
            // campos: term, command, index
            // añadir otro campo 'committed: true/false'
            return new string[] { "node1", "node2" };
        }

        // GET /nodes/:id/sm
        [HttpGet("[controller]/{id}/sm")]
        public ActionResult<int> GetStateMachine(int id) {
            var st = _cluster.GetNode((uint)id).StateMachine.RequestStatus(null);
            return int.Parse(st);
        }

        // PATCH /nodes
        [HttpPatch("[controller]/{id}")]
        public void Patch(int id, [FromBody] string value) {
            // Stòp/Restart node <id>
        }
    }
}
