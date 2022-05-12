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
    if (message.nodeEventType.includes("Stratis.Bitcoin.EventBus.CoreEvents.BlockConnected")) {
        if (message.height) {
            document.getElementById('lblSidechainNodeBlockHeight').innerHTML = ` ${message.height}`;
        }

        if (message.hash) {
            document.getElementById('lblSidechainNodeHash').innerHTML = ` ${message.hash}`;
            var hashelement = document.getElementById("sidechainBlockHash");
            hashelement.setAttribute('href', "https://chainz.cryptoid.info/cirrus/block.dws?" + ` ${message.hash}` + ".htm");
        }
    }

    if (message.nodeEventType.includes("Stratis.Bitcoin.Features.MemoryPool.TransactionAddedToMemoryPoolEvent")) {
        alert("Mempool")
    }

    if (message.nodeEventType.includes("Stratis.Bitcoin.Features.SignalR.Events.WalletGeneralInfo")) {
        if (message.connectedNodes) {
            document.getElementById('lblSidechainTotalConnectedNode').innerHTML = ` ${message.connectedNodes}` + ' /';
        }

        if (message.accountsBalances) {
            var confirmedAmount = (((` ${message.accountsBalances[0].amountConfirmed}`) / 100000000).toFixed(8));
            var parts = confirmedAmount.toString().split(".");
            var confirmedAmountwithComma = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ",") + (parts[1] ? "." + parts[1] : "");
            document.getElementById('lblSidechainAmountConfirmed').innerHTML = confirmedAmountwithComma;

            var unconfirmedAmount = (((` ${message.accountsBalances[0].amountUnconfirmed}`) / 100000000).toFixed(8));
            var unconfirmedparts = unconfirmedAmount.toString().split(".");
            var unConfirmedAmountwithComma = unconfirmedparts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ",") + (unconfirmedparts[1] ? "." + unconfirmedparts[1] : "");
            document.getElementById('lblSidechainAmountUnConfirmed').innerHTML = unConfirmedAmountwithComma;
        }

        if (message.accountsBalances) {
            document.getElementById('sidechainMiningAddress').innerHTML = ` ${message.accountsBalances[0].addresses[0].address}`;
            document.getElementById('lblSidechainMiningWalletAddress').innerHTML = ` ${message.accountsBalances[0].addresses[0].address}`;
        }
    }
});