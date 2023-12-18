var connection = new signalR.HubConnectionBuilder().withUrl("/SignalRHub").build();

$(function () {
	connection.start().then(function () {
		InvokeMessages();
	}).catch(function (err) {
		return console.error(err.toString());
	});
});

function InvokeMessages() {
	let channelId = $("#channelId").val();
	console.log('Connected to SignalRHub on room ' + channelId);

	connection.invoke("JoinRoom", channelId)
		.catch(function (err) {
			return console.error(err.toString());
	});

	connection.invoke("SendMessages", channelId)
		.catch(function (err) {
		return console.error(err.toString());
	});
}

connection.on("ReceivedMessages", function (messages) {
	let chatBox = $("#chatBox");
	chatBox.empty();
	$.each(messages, function (index, message) {
		chatBox.append(`<div class="msg-meta bubble-left">${message.userName}<div>`);
		chatBox.append(`<div class="bubble bubble-left"> ${message.content} </div>`);
	});
	chatBox = document.querySelector("#chatBox");
	chatBox.scrollTop = chatBox.scrollHeight;
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