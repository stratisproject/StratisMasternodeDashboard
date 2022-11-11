"use strict"

GetSidechainConfiguration();

var sidechainInterval = setInterval(function () {
    GetSidechainConfiguration();
}, 5000);

function GetSidechainConfiguration() {
    $.ajax({
        type: "GET",
        url: "/getConfiguration",
        data: {
            sectionName: "DefaultEndpoints",
            paramName: "EnvType"
        }
    }).done(
        function (parameterValue) {

            var signalRPort = "";
            if (parameterValue.parameter.includes('TestNet'))
                signalRPort = "39823";
            else
                signalRPort = "38823";

            ConnectToSidechainHub(signalRPort);
        });
};

function LoadSidechainPartial(connection) {

    // Stop trying to connect to the node.
    clearInterval(sidechainInterval);

    // Refresh the sidechain partial view.
    $.ajax({
        type: "GET",
        url: "/sidechaindata"
    }).done(
        function (response) {

            // Set the div's HTML
            $('#divSidechainPartial').html(response);

            // Configure all the signalR events.
            ConfigureSidechainSignalREvents(connection);
        });
}

function ConnectToSidechainHub(signalRPort) {

    var connection = new signalR
        .HubConnectionBuilder()
        .withUrl('http://localhost:' + signalRPort + '/events-hub', {
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets
    })
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build();

    connection
        .start()
        .then(function () {

            // If we successfully connect to the hub we can load the data into the partial view.
            LoadSidechainPartial(connection);
        })
        .catch(function (err) {
            return console.error(err.toString());
        });
}

function ConfigureSidechainSignalREvents(connection) {
    document.getElementById('lblSidechainNodeBlockHeight').innerHTML = "waiting for next block...";
    document.getElementById('lblSidechainNodeHash').innerHTML = "waiting for next block...";
    document.getElementById('lblSidechainNodeHeaderHeight').innerHTML = "waiting for next block...";
    document.getElementById('lblSidechainMempoolSize').innerHTML = 0;
    document.getElementById("sidechain-peerconnection-data").innerHTML = "initializing...";

    connection.on("receiveEvent", function (message) {

        if (message.nodeEventType.includes("Stratis.Bitcoin.EventBus.CoreEvents.BlockConnected")) {

            if (message.height) {
                document.getElementById('lblSidechainNodeBlockHeight').innerHTML = `${message.height}`;
            }

            if (message.hash) {
                document.getElementById('lblSidechainNodeHash').innerHTML = `${message.hash}`;
                var hashelement = document.getElementById("sidechainBlockHash");
                hashelement.setAttribute('href', "https://chainz.cryptoid.info/cirrus/block.dws?" + ` ${message.hash}` + ".htm");
            }
        }

        if (message.nodeEventType.includes("Stratis.Bitcoin.Features.MemoryPool.TransactionAddedToMemoryPoolEvent")) {

            if (message.memPoolSize) {
                document.getElementById('lblSidechainMempoolSize').innerHTML = ` ${message.memPoolSize}`;
            }
            else
                document.getElementById('lblSidechainMempoolSize').innerHTML = 0;
        }

        if (message.nodeEventType.includes("Stratis.Bitcoin.Features.SignalR.Events.WalletGeneralInfo")) {

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


        if (message.nodeEventType.includes("Stratis.Bitcoin.Features.PoA.Events.MiningStatisticsEvent")) {

            var minersHits = document.getElementById('lblSidechainMinerHitsinPercentage');
            var blockProducerHitProgressBar = document.getElementById('blockProducerHitProgressBar');
            var miningStatus = document.getElementById('lblSidechainMiningStatus');

            if (message.isMining == true) {
                miningStatus.innerHTML = `Mining`;
                miningStatus.className = "badge badge-success";
            }
            else {
                miningStatus.innerHTML = 'Not Mining';
                miningStatus.className = "badge badge-danger";
            }

            if (message.blockProducerHit) {
                document.getElementById('lblSidechainMinersHits').innerHTML = ` ${message.blockProducerHit}`;
            }
            else
                document.getElementById('lblSidechainMinersHits').innerHTML = "n/a";

            if (message.federationMemberSize) {
                var minersHitsinPercentage = (Math.round(((` ${message.blockProducerHit}` / ` ${message.federationMemberSize}`) * 100))).toFixed(1);
                minersHits.innerHTML = minersHitsinPercentage + "%";
                minersHits.style.margin = 0;
                blockProducerHitProgressBar.className = "progress-bar progress-bar-striped progress-bar-animated";
                blockProducerHitProgressBar.style.width = minersHitsinPercentage + "%";
                blockProducerHitProgressBar.setAttribute("aria-valuenow", minersHitsinPercentage);
            }
        }

        if (message.nodeEventType.includes("Stratis.Bitcoin.EventBus.CoreEvents.ConsensusManagerStatusEvent")) {
            var syncStatus = document.getElementById('lblSidechainSyncStatus');
            if (!message.isIbd) {
                syncStatus.innerHTML = `Synced 100.00%`;
                syncStatus.className = "badge badge-success";
            }
            else {
                syncStatus.innerHTML = 'Syncing';
                syncStatus.className = "badge badge-warning";
            }

            if (message.headerHeight) {
                document.getElementById('lblSidechainNodeHeaderHeight').innerHTML = ` ${message.headerHeight}`;
            }
        }

        if (message.nodeEventType.includes("Stratis.Bitcoin.EventBus.CoreEvents.PeerConnectionInfoEvent")) {

            var sidechainconnections = '';
            var inbountCount = 0;

            message.peerConnectionModels.filter(function (d) {
                if (d.inbound) {
                    inbountCount++;
                }
            });

            document.getElementById('lblSidechainTotalConnectedNode').innerHTML = ` ${message.peerConnectionModels.length}` + ' /';
            document.getElementById('lblSidechainTotalInNode').innerHTML = inbountCount + ' /';
            document.getElementById('lblSidechainTotalOutNode').innerHTML = (` ${message.peerConnectionModels.length}` - inbountCount);

            var peerconnections = message.peerConnectionModels;

            for (var i = 0; i < peerconnections.length; i++) {
                var type = (peerconnections[i].inbound) ? "inbound" : "outbound";
                sidechainconnections += "<tr>";
                sidechainconnections += "<td class='text-left' style='width: 250px;'>" + peerconnections[i].address + "</td>";
                sidechainconnections += "<td class='text-center' style='width: 150px;'>" + type + "</td>";
                sidechainconnections += "<td class='text-center' style='width: 150px;'>" + peerconnections[i].height + "</td>";
                sidechainconnections += "<td class='text-left' style='width: 450px;'>" + peerconnections[i].subversion + "</td>";
                sidechainconnections += "</tr>";
            }

            document.getElementById("sidechain-peerconnection-data").innerHTML = sidechainconnections;
        }

        if (message.nodeEventType.includes("Stratis.Bitcoin.EventBus.CoreEvents.FederationWalletStatusEvent")) {
            document.getElementById('lblSidechainFederationWalletAmountConfirmed').innerHTML = message.confirmedBalance.toString("N2");
            document.getElementById('lblSidechainFederationWalletAmountUnconfirmed').innerHTML = message.unconfirmedBalance.toString("N2");
        }
    });
}