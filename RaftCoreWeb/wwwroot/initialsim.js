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

function updateLinks() {
	var u = d3.select('.links')
	          .selectAll('line')
	          .data(links);

	u.enter()
	 .append('line')
	 .merge(u)
	 .attr('x1', function(d) {
	   return d.source.x;
	 })
	 .attr('y1', function(d) {
	   return d.source.y;
	 })
	 .attr('x2', function(d) {
	   return d.target.x;
	 })
	 .attr('y2', function(d) {
	   return d.target.y;
	 });

	u.exit().remove();
}

function updateNodeLabels() {
	u = d3.select('.tags')
	      .selectAll('text')
	      .data(nodes);

	u.enter()
	 .append('text')
	 .on('click', clickedNode)
	 .text(function(d) {
	   return d.name;
	 })
	 .merge(u)
	 .attr('x', function(d) {
	   return d.x;
	 })
	 .attr('y', function(d) {
	   return d.y;
	 })
	 .attr('dy', function(d) {
	   return 5;
	 });

	u.exit().remove();
}


function updateCircles() {
	c = d3.select('.circles')
	      .selectAll('circle')
	      .data(nodes)
	      .attr('class', 'stopped');

	c.enter()
	 .append('circle')
	 .on('click', clickedNode)
	 .merge(c)
	 .attr('cx', function(d,i){
	   return d.x;
	 })
	 .attr('cy', function(d,i){
	   return d.y;
	 })
	 .attr('r', 20)
	 .attr('stroke', "#0f0f0f")
	 .attr('class', function(d,i){
	   return states[i];
	 });

	c.exit().remove();
}