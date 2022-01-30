// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {
    console.log("ready!");

    $("#submit").click(() => {
        debugger;
        var username = $("#nick").val();
        if (username.length > 2) {
            $.get("/Home/Restart?nick=" + username, function (data) {
                console.log("restarted");
            });
        }
    });
});