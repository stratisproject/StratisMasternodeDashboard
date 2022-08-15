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
    if (message.nodeEventType.includes("Stratis.Bitcoin.EventBus.CoreEvents.MultiSigMemberStateRequestEvent")) {
        $("#" + message.pubKey + "-Online").text("Yes");
        $("#" + message.pubKey + "-LastUpdate").text(new Date().toLocaleString());
        $("#" + message.pubKey + "-CrossChainStoreHeight").text(message.crossChainStoreHeight);
    }
});