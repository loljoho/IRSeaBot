// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {
    console.log("ready!");
    setInterval(getList, 3000);

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
        });

    });

    $("#bot-table").on("click", "button", function () {
        var id = $(this).data("guid");
        $.get("/Home/Kill?id=" + id).done(function (data) {
            console.log("killed bot");
        });
    });

    function getList() {
        $.get("/Home/GetBotList").done(function (data) {
            console.log(data);
            $('.removeRow').remove();
            var list = JSON.parse(data);
            $.each(list, function (index, element) {
                addBotRow(element);
            })
        });
    }


    function addBotRow(bot) {
        $('#bot-table tbody').append(
            `<tr class='removeRow' id='` +bot.Id +`'>
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