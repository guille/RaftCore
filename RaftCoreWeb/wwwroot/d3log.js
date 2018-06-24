var width = 500, height = 400

logs = [{}, {}, {}, {}, {}]
sm = []

// var svg = d3.select("body")
// 	.select('div#log')
// 	.append("svg")
//   .attr("width", width)
//   .attr("height", height)

// var g = svg.append("g")
//   .selectAll('rect')
//   .data(logs)
//   .enter()
//   .append("rect")
//   .attr("x", 0)
// 	.attr("y", function(d, i) { return i*60 })
// 	.attr("width", width)
// 	.attr("height", 50)

createIndexes()
createLogs()



// function updateSM(error, response) {
// 	d3.request("nodes/1/sm")
//     .get(updateSM)

// 	data.push(response)
// 	var svg = d3.select("body")
// 		.select('div#sm')
// 		.data(sm)
// 		.enter()
// 		.append("div")
// 		.attr('class', 'sm-index')
// 		.text(function(d) { return d; } )
// }

function createIndexes() {
	var svg = d3.select("body")
		.select('div#log')
		.append("div")
		.attr('class', 'log-index')
		.text("0")
}

// Appends one div for each log
function createLogs() {
	var svg = d3.select("body")
		.select('div#log')
		.selectAll('div')
		.data(logs)
		.enter()
		.append("div")
		.attr('class', 'node_log')
		.text("h")
}


function sendRequest() {
	httpRequest = new XMLHttpRequest()
	var userRequest = document.getElementById("request").value
	console.log(userRequest)

  if (!httpRequest) {
    alert('Giving up :( Cannot create an XMLHTTP instance');
    return false;
  }
  // httpRequest.onreadystatechange = alertContents;
  httpRequest.open('POST', 'nodes/requests/');
  httpRequest.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
  httpRequest.send('userRequest=' + encodeURIComponent(userRequest));
}

function switchNode() {
		httpRequest = new XMLHttpRequest()
		var nodeToSwitch = 1

	  if (!httpRequest || selected === undefined) {
	    alert('Giving up :( Cannot create an XMLHTTP instance');
	    return false;
	  }
	  // httpRequest.onreadystatechange = alertContents;
	  httpRequest.open('PATCH', 'nodes/' + selected);
	  httpRequest.send();
}