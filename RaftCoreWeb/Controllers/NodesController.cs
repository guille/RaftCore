using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace RaftCoreWeb.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class NodesController : ControllerBase
    {
        // GET /nodes/:id
        [HttpGet("{id}")]
        public ActionResult<IEnumerable<string>> Get(int id) {
            /* Returns all the relevant properties of the node in json:
            *      -  State (candidate, leader...)
            *      -  CurrentTerm
            */
            return new string[] { "candidate" };
        }

        // GET /nodes/:id/log
        [HttpGet("{id}/log")]
        public ActionResult<IEnumerable<string>> GetLog(int id) {
            // Complex json, returning array of logentries
            // campos: term, command, index
            // añadir otro campo 'committed: true/false'
            return new string[] { "node1", "node2" };
        }

        // GET /nodes/:id/sm
        [HttpGet("{id}/sm")]
        public ActionResult<int> GetStateMachine(int id) {
            return 20;
        }

        // PATCH /nodes
        [HttpPatch("{id}")]
        public void Patch(int id, [FromBody] string value) {
            // Stòp/Restart node <id>
        }
    }
}
