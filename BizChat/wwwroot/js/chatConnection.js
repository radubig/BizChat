var connection = new signalR.HubConnectionBuilder().withUrl("/SignalRHub").build();

$(function () {
	connection.start().then(function () {
		console.log('Connected to SignalRHub');
		InvokeMessages();
		console.log('S-au invocat mesaje');
	}).catch(function (err) {
		return console.error(err.toString());
	});
});

function InvokeMessages() {
	debugger;
	connection.invoke("SendMessages")
		.catch(function (err) {
		return console.error(err.toString());
	});
}

connection.on("ReceivedMessages", function (messages) {
	let chatBox = $("#chatBox");
	chatBox.empty();
	$.each(messages, function (index, message) {
		chatBox.append(`<p><b>${message.userName}</b>: ${message.content}</p>`)
	});
});

/*
connection.on("ReceiveMessage", function (user, message) {
	var encodedUser = $("<div />").text(user).html();
	var encodedMsg = $("<div />").text(message).html();
	$("#chatBox").append("<p><strong>" + encodedUser + "</strong>: " + encodedMsg + "</p>");
});

$("#sendButton").click(function () {
	var user = $("#username").val();
	var message = $("#message").val();
	connection.invoke("SendMessage", user, message);
	$("#message").val("").focus();
});

connection.start().then(function () {
	console.log("Connected!");
}).catch(function (err) {
	console.error(err.toString());
});
*/