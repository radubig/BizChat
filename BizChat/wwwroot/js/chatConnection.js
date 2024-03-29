﻿var connection = new signalR.HubConnectionBuilder().withUrl("/SignalRHub").build();
var editMessage = false;
var editMessageId;

var Moderator = false;
var CurrentUserId;
$(function () {
	connection.start().then(function () {
		InvokeMessages();
	}).catch(function (err) {
		return console.error(err.toString());
	});

	var channelId = $("#channelId").val();

	$.ajax({
		type: 'POST',
		url: '/Servers/UserInformation/',
		data: { channelId },
		dataType: 'json',
		cache: false,
		connection: 'application/json',
		success: function (ret) {
			console.log(ret);
			if (ret.isMod == true) {
				Moderator = true;
			}
			CurrentUserId = ret.userId;
		},
		error: function () { },
	});

	// Attach click event handler to <a> tags
	let chatBox = $("#chatBox");
	// EDIT
	chatBox.on('click', 'a.fa-square-pen', function () {
		let messageId = $(this).data('message-id');

		editMessage = true;
		editMessageId = messageId;

		let messageContent = document.querySelectorAll('[data-message-id="' + messageId + '"]')[2].dataset.textcontent;
		//console.log(document.querySelectorAll('[data-message-id="' + messageId + '"]')[2].dataset.textcontent);

		let messageBox = $("#input-message");
		messageBox.val(messageContent);

		$("#sendButton").text("Edit");
	});

	// DELETE
	chatBox.on('click', 'a.fa-trash', function () {
		let messageId = $(this).data('message-id');
		DispatchDeleteMessage(messageId);
	});

	// Attach event listener to input box when pressing ENTER
	$("#input-message").on('keypress', function (event) {
		if (event.which == 13) {
			onSend();
		}
	});

	$("#sendButton").click(onSend);
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
		var div;
		console.log(message.userId + ' ' + CurrentUserId);
		if (message.userId == CurrentUserId) {
			div = $(`<div class="msg-meta bubble-right" style="display: flex; flex-direction: row; align-items: center; justify-content: center">
						<a class="fa-solid fa-square-pen" style="margin-right: 7px" data-message-id="${message.id}"></a>
						<a class="fa-solid fa-trash" style="margin-right: 7px" data-message-id="${message.id}"></a>
						<div style="margin-right: 7px">${(message.date).substring(11, 19) + ' ' + (message.date).substring(0, 10)}</div>
						<div style="margin-right: 7px">${message.userName}</div>
					  </div>`);
		}
		else {
			if (Moderator == true) {
				div = $(`<div class="msg-meta bubble-left" style="display: flex; flex-direction: row; align-items: center; justify-content: center">
						<div style="margin-right: 7px">${message.userName}</div>
						<div style="margin-right: 7px">${(message.date).substring(11, 19) + ' ' + (message.date).substring(0, 10)}</div>
						<a class="fa-solid fa-trash" style="margin-right: 7px" data-message-id="${message.id}"></a>
					  </div>`);
			}
			else {
				div = $(`<div class="msg-meta bubble-left" style="display: flex; flex-direction: row; align-items: center; justify-content: center">
						<div style="margin-right: 7px">${message.userName}</div>
						<div style="margin-right: 7px">${(message.date).substring(11, 19) + ' ' + (message.date).substring(0, 10)}</div>
					  </div>`);
			}
		}
		chatBox.append(div);
		if (isSingleLink(message.content)) {
			if (isImageUrl(message.content)) {
				//<div>${message.content}</div>
				if (message.userId == CurrentUserId) {

					chatBox.append(`<div class="bubble bubble-right" data-message-id="${message.id}" style="width: auto" data-textcontent="${message.content}">
										
										<img src="${message.content}" alt="Imagine" data-img-id="${message.id}" style="max-height: 40vh"/>
									</div>`);

				}
				else {

					chatBox.append(`<div class="bubble bubble-left" data-message-id="${message.id}" style="width: auto" data-textcontent="${message.content}">
										
										<img src="${message.content}" alt="Imagine" data-img-id="${message.id}" style="max-height: 40vh"/>
										</div>`);

				}

			}
			else if (isVideoUrl(message.content)) {

				//<div>${message.content}</div>
				if (message.userId == CurrentUserId) {
					chatBox.append(`<div class="bubble bubble-right" data-message-id="${message.id}" style="width: auto" data-textcontent="${message.content}">
										
										<video controls data-video-id="${message.id}" style="max-height: 40vh">
										<source src="${message.content}" type="video/mp4" />
										Video
										</video>
									</div>`);
				}
				else {
					chatBox.append(`<div class="bubble bubble-left" data-message-id="${message.id}" style="width: auto" data-textcontent="${message.content}">
										<video controls data-video-id="${message.id}" style="max-height: 40vh">
										<source src="${message.content}" type="video/mp4" />
										Video
										</video>
									</div>`);
				}
			}
			else {
				if (message.userId == CurrentUserId) {
					chatBox.append(`<div class="bubble bubble-right" data-message-id="${message.id}" data-textcontent="${message.content}">
									<a href="${message.content}" style="color: white">${message.content}</a>
									</div>`);
				}
				else {
					chatBox.append(`<div class="bubble bubble-left" data-message-id="${message.id}" data-textcontent="${message.content}">
								    <a href="${message.content}" style="color: white">${message.content}</a>
									</div>`);
				}
			}
		}
		else {
			if (message.userId == CurrentUserId) {
				chatBox.append(`<div class="bubble bubble-right" data-message-id="${message.id}" data-textcontent="${message.content}">${message.content}</div>`);
			}
			else {
				chatBox.append(`<div class="bubble bubble-left" data-message-id="${message.id}" data-textcontent="${message.content}">${message.content}</div>`);
			}
		}
	});

	chatBox = document.querySelector("#chatBox");
	chatBox.scrollTop = chatBox.scrollHeight;
});

function onSend() {
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
}

// functii pentru verificarea link-urilor
function isSingleLink(inputString) {
	// Regular expression pattern for a valid URL
	var urlPattern = /^(https?:\/\/)?([\w.-]+)\.([a-z]{2,})(\/\S*)?$/i;

	// Test if the inputString matches the URL pattern
	return urlPattern.test(inputString);
}

function isImageUrl(url) {
	// Define a regular expression pattern for image file extensions
	var pattern = /\.(png|jpe?g|gif|bmp|tiff)$/i;

	// Use RegExp.test to check if the URL ends with an image extension
	return pattern.test(url);
}

function isVideoUrl(url) {
	// Define a regular expression pattern for image file extensions
	var pattern = /\.(mp4)$/i;

	// Use RegExp.test to check if the URL ends with an image extension
	return pattern.test(url);
}