var width = 500, height = 400

logs = [{}, {}, {}, {}, {}]

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
