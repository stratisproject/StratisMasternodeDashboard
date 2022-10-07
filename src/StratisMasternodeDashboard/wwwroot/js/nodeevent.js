"use strict"

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

        ConnectAndReceiveSignalRServerHub(signalRPort)
    });


function ConnectAndReceiveSignalRServerHub(signalRPort) {
    var connection = new signalR.HubConnectionBuilder().withUrl('http://localhost:' + signalRPort + '/events-hub', {
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

    var blockHeight="...";
    var hash="...";
    connection.on("receiveEvent", function (message) {
        if (message.nodeEventType.includes("Stratis.Bitcoin.EventBus.CoreEvents.BlockConnected")) {
            if (message.height) {
                blockHeight = ` ${message.height}`;            
            }

            if (message.hash) {
                hash = ` ${message.hash}`;
                var hashelement = document.getElementById("sidechainBlockHash");
                hashelement.setAttribute('href', "https://chainz.cryptoid.info/cirrus/block.dws?" + ` ${message.hash}` + ".htm");
            }
        }
        document.getElementById('lblSidechainNodeBlockHeight').innerHTML = blockHeight;
        document.getElementById('lblSidechainNodeHash').innerHTML = hash;

        document.getElementById('lblSidechainMempoolSize').innerHTML = 0;
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

        document.getElementById('lblSidechainMinersHits').innerHTML = 0;
        var minersHits = document.getElementById('lblSidechainMinerHitsinPercentage');
        minersHits.style.margin = 0;
        minersHits.innerHTML = "0.0%";
        var blockProducerHitProgressBar = document.getElementById('blockProducerHitProgressBar');
        blockProducerHitProgressBar.style.width = "0%";
        blockProducerHitProgressBar.setAttribute("aria-valuenow", "0");
        var miningStatus = document.getElementById('lblSidechainMiningStatus');
        miningStatus.innerHTML = 'loading';
        miningStatus.className = "badge badge-info";
        if (message.nodeEventType.includes("Stratis.Bitcoin.Features.PoA.Events.MiningStatisticsEvent")) {           
            if (message.isMining = true) {
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
                document.getElementById('lblSidechainMinersHits').innerHTML = 0;

            if (message.federationMemberSize) {
                var minersHitsinPercentage = (Math.round(((` ${message.blockProducerHit}` / ` ${message.federationMemberSize}`) * 100))).toFixed(1);
                minersHits.innerHTML = minersHitsinPercentage + "%";
                minersHits.style.margin = 0;
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

        var sidechainconnections = '';
        if (message.nodeEventType.includes("Stratis.Bitcoin.EventBus.CoreEvents.PeerConnectionInfoEvent")) {
            var inbountCount = 0;
            var type;
            var filtered = message.peerConnectionModels.filter(function (d) {
                if (d.inbound) {
                    inbountCount++;
                }
            });
            document.getElementById('lblSidechainTotalConnectedNode').innerHTML = ` ${message.peerConnectionModels.length}` + ' /';

            document.getElementById('lblSidechainTotalInNode').innerHTML = inbountCount + ' /';
            document.getElementById('lblSidechainTotalOutNode').innerHTML = (` ${message.peerConnectionModels.length}` - inbountCount);

            console.info(new Blob([JSON.stringify(message.peerConnectionModels)]).size);//to get the array's size in bytes 

            var peerconnections = message.peerConnectionModels;
            for (var i = 0; i < peerconnections.length; i++) {
                sidechainconnections += "<tr>";
                sidechainconnections += "<td class='text-left' style='width: 250px;'>" + peerconnections[i].address + "</td>";
                sidechainconnections += "<td class='text-center' style='width: 150px;'>" + peerconnections[i].inbound + "</td>";
                sidechainconnections += "<td class='text-center' style='width: 150px;'>" + peerconnections[i].height + "</td>";
                sidechainconnections += "<td class='text-left' style='width: 450px;'>" + peerconnections[i].subversion + "</td>";
                sidechainconnections += "</tr>";
            }
            document.getElementById("sidechain-peerconnection-data").innerHTML = sidechainconnections;
        }

    });
}