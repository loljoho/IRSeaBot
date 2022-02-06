// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {
    console.log("ready!");

    $("#submit").click(() => {
        var server = $("#server").val();
        var port = $("#port").val();
        var username = $("#username").val();
        var nick = $("#nick").val();
        var channel = $("#channel").val();

        var obj = {
            "Server" : server,
            "Port": port,
            "Username": username,
            "Nick": nick,
            "Channel": channel,
        }
        $.post("/Home/StartBot", { config: JSON.stringify(obj) }).done(function (data) {
            console.log("started bot");
            var bot = JSON.parse(data);
            debugger;
            addBotRow(bot);
            //$('#bot-table tr:last').after(
            //    `<tr>
            //        <td>` + bot.id + `</td>
            //        <td>` + bot.server + `</td>
            //        <td>` + bot.port + `</td>
            //        <td>` + bot.username + `</td>
            //        <td>` + bot.nick + `</td>
            //        <td>` + bot.server + `</td>
            //        <td><button class='btn-danger' id='kill-`+ bot.id +`'>Kill</button></td>
            //    </tr>`
            //);
            resetForm();
        });

    });

    $("#bot-table").on("click", "button", function () {
        debugger
        var id = $(this).data("guid");
        $.get("/Home/Kill?id=" + id).done(function (data) {
            $("#" + data).remove();
            getList();
        });
    });

    function getList() {
        $.get("/Home/GetBotList").done(function (data) {
            console.log(data);
        });
    }



    function addBotRow(bot) {
        $('#bot-table tr:last').after(
            `<tr id='` +bot.Id +`'>
                <td>` + bot.Id + `</td>
                <td>` + bot.Server + `</td>
                <td>` + bot.Port + `</td>
                <td>` + bot.Username + `</td>
                <td>` + bot.Nick + `</td>
                <td>` + bot.Channel + `</td>
                <td><button class='btn btn-danger' data-guid='`+ bot.Id + `'>Kill</button></td>
            </tr>`
        );
    }

    function resetForm() {
        $("#server").val("");
        $("#port").val("");
        $("#username").val("");
        $("#nick").val("");
        $("#channel").val("");
    }
});