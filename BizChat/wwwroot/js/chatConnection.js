var connection = new signalR.HubConnectionBuilder().withUrl("/SignalRHub").build();

$(function () {
	connection.start().then(function () {
		console.log('Connected to SignalRHub');
		InvokeMessages();
	}).catch(function (err) {
		return console.error(err.toString());
	});
});

function InvokeMessages() {
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

$("#sendButton").click(function () {
	let messageBox = $("#input-message");
	let msg = messageBox.val();
	let channelId = $("#channelId").val();
	$.ajax({
		type: "POST",
		url: '/Servers/CreateMessage',
		data: { content: msg, channelId: channelId },
		dataType: "json",
		success: function () { },
		error: function () {
			alert("Eroare la trimiterea mesajului");
		}
	});
	messageBox.val("").focus();
});