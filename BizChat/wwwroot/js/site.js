// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function editcateg(categ_id) {
    let edit_div = document.getElementById(categ_id);
    let name_div = document.getElementById('Name' + categ_id);
    console.log(name_div.innerHTML);
    if (edit_div.style.display == 'none') {
        edit_div.style.display = 'flex';
    }
    else {
        edit_div.style.display = 'none';
    }
    if (name_div.style.display == 'none') {
        name_div.style.display = 'flex';
    }
    else {
        name_div.style.display = 'none';
    }

    // Da focus pe element si muta cursorul pe ultima pozitie
    const input = edit_div.firstElementChild.firstElementChild
    const end = input.value.length;
    input.setSelectionRange(end, end);
    input.focus();
}

function editchannel() {
    let channel_desc = document.getElementById('ChannelDesc');
    let channel_form = document.getElementById('ChannelDescForm');
    console.log('editare channel');
    if (channel_desc.style.display == 'none') {
        channel_desc.style.display = 'flex';
    }
    else {
        channel_desc.style.display = 'none';
    }
    if (channel_form.style.display == 'none') {
        channel_form.style.display = 'flex';
    }
    else {
        channel_form.style.display = 'none';
    }
}