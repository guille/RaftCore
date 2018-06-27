var initial = [{}, {}, {}, {}]

function sendHeartbeats() {
	d3.select('.messages')
	  .selectAll('circle')
	  .interrupt()
	  .remove();

	if (!document.getElementById("send-heartbeats").checked) return;
	// for state in states
	// if any are leader, call heartbeatFrom()
	// otherwise, wait a second and retry
	for (var i = 0; i < states.length; i++) {
		if (states[i] === "Leader") {
			heartbeatFrom(i);
			return;
		}
	}
	setTimeout(sendHeartbeats, 1000);
}

function heartbeatFrom(source) {
	// nth of type starts at 1
	var source_x = d3.selectAll('.circles circle').filter(':nth-of-type(' + (source + 1) + ')').attr('cx');
	var source_y = d3.selectAll('.circles circle').filter(':nth-of-type(' + (source + 1) + ')').attr('cy');


	var message_i = 0;
	var circles = d3.selectAll('.circles circle');
	for (var node_i = 0; node_i < 5; node_i++) {
		if (node_i != source) {
			var target_x = circles.filter(':nth-of-type(' + (node_i + 1) + ')').attr('cx');
			var target_y = circles.filter(':nth-of-type(' + (node_i + 1) + ')').attr('cy');
			var simulation = d3.forceSimulation(initial.slice(message_i, message_i + 1))
			                   .velocityDecay(0.3)
			                   .alphaMin(0.09)
			                   .force('r', d3.forceRadial(0, target_x, target_y))
			                   .on('tick', updateMessages);

			// Add the callback only to one of them
			if (message_i === 0) simulation.on('end', sendHeartbeats);
			message_i++;
		}
	}

	// create the 5 circles
	var u = d3.select('.messages')
	          .selectAll('circle')
	          .data(initial);

	u.enter()
	 .append('circle')
	 .attr('cx', source_x)
	 .attr('cy', source_y)
	 .attr('r', 10);

	u.exit().remove();
}

function updateMessages() {
	var u = d3.select('.messages')
	          .selectAll('circle')
	          .data(initial);

	u.enter()
	 .append('circle')
	 .merge(u)
	 .transition()
	 .duration(75)
	 .ease(d3.easeCircleIn)
	 .attr('cx', function(d,i){
	    return d.x;
	 })
	 .attr('cy', function(d,i){
	    return d.y;
	 });

	u.exit().remove();
}

