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

var links = [
  {source: 0, target: 1},
  {source: 0, target: 2},
  {source: 0, target: 3},
  {source: 0, target: 4},
  {source: 1, target: 2},
  {source: 1, target: 3},
  {source: 1, target: 4},
  {source: 2, target: 3},
  {source: 2, target: 4},
  {source: 3, target: 4},
]

var simulation = d3.forceSimulation(nodes)
  .force('charge', d3.forceManyBody().strength(-4000))
  .force('center', d3.forceCenter(width / 2, height / 2))
  .force('link', d3.forceLink().links(links))
  .on('tick', ticked)
  .on('end', autoPolling)

function autoPolling() {
  console.log("running")
  setInterval(ticked, 1000)
}

function updateLinks() {
  var u = d3.select('.links')
    .selectAll('line')
    .data(links)

  u.enter()
    .append('line')
    .merge(u)
    .attr('x1', function(d) {
      return d.source.x
    })
    .attr('y1', function(d) {
      return d.source.y
    })
    .attr('x2', function(d) {
      return d.target.x
    })
    .attr('y2', function(d) {
      return d.target.y
    })

  u.exit().remove()
}

function updateText() {
  u = d3.select('.tags')
    .selectAll('text')
    .data(nodes)

  u.enter()
    .append('text')
    .on('click', clickedNode)
    .text(function(d) {
      return d.name
    })
    .merge(u)
    .attr('x', function(d) {
      return d.x
    })
    .attr('y', function(d) {
      return d.y
    })
    .attr('dy', function(d) {
      return 5
    })
  

  u.exit().remove()

}



function updateNodeStates() {
  // update variable states
  var oReq = new XMLHttpRequest();
  oReq.open("GET", "nodes/states/");
  oReq.onload = function (e) {
    if (oReq.readyState === 4) {
      if (oReq.status === 200) {
        states = JSON.parse(oReq.responseText)
      } else {
        console.error(oReq.statusText);
      }
    }
  };
  oReq.send();
  
}

function updateNodeTerms() {
  // update variable states
  var oReq = new XMLHttpRequest();
  oReq.open("GET", "nodes/terms/");
  oReq.onload = function (e) {
    if (oReq.readyState === 4) {
      if (oReq.status === 200) {
        terms = JSON.parse(oReq.responseText)
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
  selected = i.index
}

function updateCircles() {
	c = d3.select('.circles')
    .selectAll('circle')
    .data(nodes)
    .attr('class', 'stopped')

	c.enter()
	  .append('circle')
    .on('click', clickedNode)
	  .merge(c)
	  .attr('cx', function(d,i){
	      return d.x
	  })
	  .attr('cy', function(d,i){
	      return d.y
	  })
	  .attr('r', 20)
	  .attr('stroke', "#0f0f0f")
    .attr('class', function(d,i){
      return states[i]
    })

	c.exit().remove()
}

function ticked() {
  updateLinks()
  updateCircles()
  updateText()
  updateNodeStates()
  updateNodeTerms()
  updatePanel()
}


function createPathsFrom(source) {
  var ret = []
  var j = 0
  for (var i = 0; i < 5; i++) {
    if (i != source) {
      ret[j] = {source: source, target: i}
      j++
    }
  }
  return ret
}

var initial = [{}, {}, {}, {}]


function heartbeatFrom(source) {
  // nth of type starts at 1
  var source_x = d3.selectAll('.circles circle').filter(':nth-of-type(' + (source + 1) + ')').attr('cx')
  var source_y = d3.selectAll('.circles circle').filter(':nth-of-type(' + (source + 1) + ')').attr('cy')


  var message_i = 0
  var circles = d3.selectAll('.circles circle')
  for (var node_i = 0; node_i < 5; node_i++) {
    if (node_i != source) {
      var target_x = circles.filter(':nth-of-type(' + (node_i + 1) + ')').attr('cx')
      var target_y = circles.filter(':nth-of-type(' + (node_i + 1) + ')').attr('cy')
      var simulation = d3.forceSimulation(initial.slice(message_i, message_i + 1))
        .velocityDecay(0.3)
        .alphaMin(0.09)
        .force('r', d3.forceRadial(0, target_x, target_y))
        .on('tick', updateMessages)
        .on('end', removeMessages)
        // .on('end', sendMessageBack(target_x, target_y, source_x, source_y))
      message_i++
    }
  }

  var u = d3.select('.messages')
    .selectAll('circle')
    .interrupt()
    .remove()

  var u = d3.select('.messages')
    .selectAll('circle')
    .data(initial)

  u.enter()
    .append('circle')
    .attr('cx', source_x)
    .attr('cy', source_y)
    .attr('r', 10)

  u.exit().remove()
}

function updateMessages() {
  // Instead of a force simply apply on tick
  var u = d3.select('.messages')
    .selectAll('circle')
    .data(initial)

  u.enter()
    .append('circle')
    .merge(u)
    .transition()
    .duration(75)
    .ease(d3.easeCircleIn)
    .attr('cx', function(d,i){
        return d.x
    })
    .attr('cy', function(d,i){
        return d.y
    })

  u.exit().remove()
}

function removeMessages() {
  d3.select('.messages')
    .selectAll('circle')
    .interrupt()
    .remove()
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