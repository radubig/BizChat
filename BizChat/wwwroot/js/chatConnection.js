var connection = new signalR.HubConnectionBuilder().withUrl("/SignalRHub").build();
var editMessage = false;
var editMessageId;

$(function () {
	connection.start().then(function () {
		InvokeMessages();
	}).catch(function (err) {
		return console.error(err.toString());
	});

	// Attach click event handler to <a> tags
	let chatBox = $("#chatBox");
	// EDIT
	chatBox.on('click', 'a.fa-square-pen', function () {
		let messageId = $(this).data('message-id');

		editMessage = true;
		editMessageId = messageId;

		let messageContent = $('div[data-message-id="' + messageId + '"]').text();

		let messageBox = $("#input-message");
		messageBox.val(messageContent);

		$("#sendButton").text("Edit");
	});

	// DELETE
	chatBox.on('click', 'a.fa-trash', function () {
		let messageId = $(this).data('message-id');
		DispatchDeleteMessage(messageId);
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

function DispatchEditMessage(messageId, content) {
	$.ajax({
		type: "POST",
		url: '/Messages/EditMessage',
		data: { content: content, messageId: messageId },
		dataType: "json",
		success: function () { },
		error: function () {
			alert("Eroare la editarea mesajului");
		}
	});
}

function DispatchDeleteMessage(messageId) {
	$.ajax({
		type: "POST",
		url: '/Messages/DeleteMessage',
		data: {messageId: messageId },
		dataType: "json",
		success: function () { },
		error: function () {
			alert("Eroare la stergerea mesajului");
		}
	});
}

connection.on("ReceivedMessages", function (messages) {
	let chatBox = $("#chatBox");
	chatBox.empty();
	$.each(messages, function (index, message) {
		let div = $(`<div class="msg-meta bubble-left" style="display: flex; flex-direction: row; align-items: center; justify-content: center">
						<div style="margin-right: 7px">${message.userName}</div>
						<div style="margin-right: 7px">${(message.date).substring(11, 19) + ' ' + (message.date).substring(0, 10)}</div>
						<a class="fa-solid fa-square-pen" style="margin-right: 7px" data-message-id="${message.id}"></a>
						<a class="fa-solid fa-trash" style="margin-right: 7px" data-message-id="${message.id}"></a>
					  </div>`);
		chatBox.append(div);
		chatBox.append(`<div class="bubble bubble-left" data-message-id="${message.id}">${message.content}</div>`);
	});

	chatBox = document.querySelector("#chatBox");
	chatBox.scrollTop = chatBox.scrollHeight;
});

$("#sendButton").click(function () {
	let messageBox = $("#input-message");
	let msg = messageBox.val();
	let channelId = $("#channelId").val();

	if (editMessage) {
		DispatchEditMessage(editMessageId, msg);
		editMessage = false;
		editMessageId = null;
		$("#sendButton").text("Send");
	} else {
		$.ajax({
			type: "POST",
			url: '/Messages/CreateMessage',
			data: { content: msg, channelId: channelId },
			dataType: "json",
			success: function () { },
			error: function () {
				alert("Eroare la trimiterea mesajului");
			}
		});
	}
	messageBox.val("").focus();
});