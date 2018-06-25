var selected;

var states = ['Stopped', 'Stopped', 'Stopped', 'Stopped', 'Stopped']
var terms = [0, 0, 0, 0, 0]

var width = 400, height = 300

var nodes = [
  {name: 'A'},
  {name: 'B'},
  {name: 'C'},
  {name: 'D'},
  {name: 'E'},
]



var simulation = d3.forceSimulation(nodes)
  .alpha(0.9)
  .alphaMin(0.25)
  .force('charge', d3.forceManyBody().strength(-4000))
  .force('center', d3.forceCenter(width / 2, height / 2))
  .force('link', d3.forceLink().links(links))
  .on('tick', updateSimulationForces)
  .on('end', autoPolling)

function updateSimulationForces() {
  updateLinks()
  updateCircles()
  updateNodeLabels()
}


function autoPolling() {
  // sendHeartbeats()
  (function update(){
    updateView()
    setTimeout(update, 100);
  })();
}


function updateView() {
  updateCircles()
  updateNodesInfo()
  updatePanel()
  updateLogs()
  updateSM()
}



function updateNodesInfo() {
  // update variable states
  var oReq = new XMLHttpRequest();
  oReq.open("GET", "nodes/");
  oReq.onload = function (e) {
    if (oReq.readyState === 4) {
      if (oReq.status === 200) {
        res = JSON.parse(oReq.responseText)
        terms = res[0]
        states = res[1]
      } else {
        console.error(oReq.statusText);
      }
    }
  };
  oReq.send();
  
}

function updatePanel() {
  if (selected !== undefined) {
    document.getElementById("selected-node-id").innerHTML = nodes[selected].name
    document.getElementById("selected-node-state").innerHTML = states[selected]
    document.getElementById("selected-node-term").innerHTML = terms[selected]
  }
}

function clickedNode(i) {
  document.getElementById("switch-node").removeAttribute("disabled")
  selected = i.index
}


function switchNode() {
    httpRequest = new XMLHttpRequest()
    var nodeToSwitch = 1

    if (!httpRequest || selected === undefined) {
      document.getElementById("errors").innerHTML = "Click on a node to select it"
      return false;
    }
    // httpRequest.onreadystatechange = alertContents;
    httpRequest.open('PATCH', 'nodes/' + selected);
    httpRequest.send();
}

messages_back = [{}, {}, {}, {}, {}]

function sendMessageBackTo(source_x, source_y, target_x, target_y) {
  // Two options:
  // Delete nodes at target and spawn new messages here from target back to source (sounds hard)
  // Or make a new simulation inverse to the previous one
  // msg_data = [{}]
  // var simulation = d3.forceSimulation(msg_data)
  //       .velocityDecay(0.3)
  //       .alphaMin(0.09)
  //       .force('r', d3.forceRadial(0, target_x, target_y))
  //       .on('tick', updateMessageBack)
  //       .on('end', removeMessages)
}