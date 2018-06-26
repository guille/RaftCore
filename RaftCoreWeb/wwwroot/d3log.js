function sendRequest() {
	httpRequest = new XMLHttpRequest()
	var userRequest = document.getElementById("request").value

	if (!httpRequest) {
		alert('Giving up :( Cannot create an XMLHTTP instance');
		return false;
	}
	// httpRequest.onreadystatechange = alertContents;
	httpRequest.open('POST', 'nodes/requests/');
	httpRequest.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
	httpRequest.send('userRequest=' + encodeURIComponent(userRequest));
}


sm = [0, 0, 0, 0, 0]

function updateSM() {
	var oReq = new XMLHttpRequest();
	oReq.open("GET", "nodes/sm/");
	oReq.onload = function (e) {
		if (oReq.readyState === 4) {
			if (oReq.status === 200) {
				sm = JSON.parse(oReq.responseText)
				displaySM()
			}
			else {
				console.error(oReq.statusText);
			}
		}
	};
	oReq.send();
}

function displaySM() {
	var u = d3.select('#sm')
	          .selectAll('div')
	          .data(sm)

	u.enter()
	 .append('div')
	 .attr('class', 'node_sm')
	 .merge(u)
	 .text(function(d, i) {
	   return sm[i]
	 })

	u.exit().remove()
}


function updateLogs() {
	var oReq = new XMLHttpRequest()
	oReq.open("GET", "nodes/log/")
	oReq.onload = function (e) {
		if (oReq.readyState === 4) {
		 	if (oReq.status === 200) {
				logs = JSON.parse(oReq.responseText)
				displayLogs()
			} else {
				console.error(oReq.statusText);
			}
		}
	};
	oReq.send();
}

logs=[[],[],[],[],[]]

function displayLogs() {
	for (var i = 0; i < logs.length; i++) {
		// Show node i log
		v = d3.selectAll('.node_log')
		      .filter(':nth-of-type(' + (i+1) + ')')
		      .selectAll('div')
		      .data(logs[i])

		v.enter()
		 .append('div')
		 .attr('class', 'log_entry')
		 .merge(v)
		 .text(function(d, j) {
		 	return logs[i][j].command
		 })
		
		v.exit().remove()
	}
}