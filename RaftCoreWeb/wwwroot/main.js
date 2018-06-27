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

initSim();

function initSim() {
	var simulation = d3.forceSimulation(nodes)
	                   .alpha(0.9)
	                   .alphaMin(0.25)
	                   .force('charge', d3.forceManyBody().strength(-4000))
	                   .force('center', d3.forceCenter(width / 2, height / 2))
	                   .force('link', d3.forceLink().links(links))
	                   .on('tick', updateSimulationForces)
	                   .on('end', autoPolling);
}

function updateSimulationForces() {
	updateLinks();
	updateCircles();
	updateNodeLabels();
}

function autoPolling() {
	drawLogContainers();
	showBody();
	(function update(){
		updateView();
		setTimeout(update, 100);
	})();
}

function showBody() {
	document.body.style.display = "block";
}


function updateView() {
	updateCircles();
	updateNodesInfo();
	updatePanel();
	updateLogs();
	updateSM();
}

function updateNodesInfo() {
	// update variable states
	var oReq = new XMLHttpRequest();
	oReq.open("GET", "nodes/");
	oReq.onload = function (e) {
		if (oReq.readyState === 4) {
			if (oReq.status === 200) {
				res = JSON.parse(oReq.responseText);
				terms = res[0];
				states = res[1];
			}
			else {
				console.error(oReq.statusText);
			}
		}
	};
	oReq.send();
}

function updatePanel() {
	if (selected !== undefined) {
		document.getElementById("selected-node-id").innerHTML = nodes[selected].name;
		document.getElementById("selected-node-state").innerHTML = states[selected];
		document.getElementById("selected-node-term").innerHTML = terms[selected];
	}
}

function clickedNode(i) {
	document.getElementById("switch-node").removeAttribute("disabled");
	document.getElementById("messages").innerHTML="";
	selected = i.index;
}


function switchNode() {
	httpRequest = new XMLHttpRequest();

	if (!httpRequest || selected === undefined) {
		document.getElementById("messages").style.color="red";
		return false;
	}
	httpRequest.open('PATCH', 'nodes/' + selected);
	httpRequest.send();
}