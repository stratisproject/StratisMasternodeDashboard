"use strict";
var connection = new signalR.HubConnectionBuilder().withUrl("http://localhost:39823/events-hub", {
    skipNegotiation: true,
    transport: signalR.HttpTransportType.WebSockets
})
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();
connection.start().then(function () {
}).catch(function (err) {
    return console.error(err.toString());
});

connection.on("receiveEvent", function (message) {
    if (message.height) {
        document.getElementById('lblSidechainNodeBlockHeight').innerHTML = ` ${message.height}`;
    }

    if (message.hash) {
        document.getElementById('lblSidechainNodeHash').innerHTML = ` ${message.hash}`;
    }
   
    if (message.accountsBalances) {
        var confirmedAmount = (((` ${message.accountsBalances[0].amountConfirmed}`) / 100000000).toFixed(8));
        var parts = confirmedAmount.toString().split(".");
        var confirmedAmountwithComma = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ",") + (parts[1] ? "." + parts[1] : "");
        document.getElementById('lblSidechainAmountConfirmed').innerHTML = confirmedAmountwithComma;

        document.getElementById('lblSidechainAmountUnConfirmed').innerHTML = ` ${message.accountsBalances[0].amountUnconfirmed}`;
    }

});