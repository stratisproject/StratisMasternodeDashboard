﻿"use strict"

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
            signalRPort = "27102";
        else
            signalRPort = "17102";

        ConnectAndReceiveSignalRServerHubMainchain(signalRPort)
    });

function ConnectAndReceiveSignalRServerHubMainchain(signalRPort) {
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
    
    document.getElementById('lblMainchainNodeBlockHeight').innerHTML = "...";
    document.getElementById('lblMainchainNodeHash').innerHTML = "...";
    document.getElementById('lblMainchainNodeHeaderHeight').innerHTML = "...";
    document.getElementById('lblMainchainMempoolSize').innerHTML = 0;
    document.getElementById("mainchain-peerconnection-data").innerHTML = "...";

    connection.on("receiveEvent", function (message) {
        if (message.nodeEventType.includes("Stratis.Bitcoin.EventBus.CoreEvents.BlockConnected")) {
            if (message.height) {
                document.getElementById('lblMainchainNodeBlockHeight').innerHTML = ` ${message.height}`;
            }

            if (message.hash) {
                document.getElementById('lblMainchainNodeHash').innerHTML = ` ${message.hash}`;
                var hashelement = document.getElementById("mainchainBlockHash");
                hashelement.setAttribute('href', "https://chainz.cryptoid.info/strax/block.dws?" + ` ${message.hash}` + ".htm");
            }
        }
        
        if (message.nodeEventType.includes("Stratis.Bitcoin.Features.MemoryPool.TransactionAddedToMemoryPoolEvent")) {
            if (message.memPoolSize) {
                document.getElementById('lblMainchainMempoolSize').innerHTML = ` ${message.memPoolSize}`;
            }
            else
                document.getElementById('lblMainchainMempoolSize').innerHTML = 0;
        }

        if (message.nodeEventType.includes("Stratis.Bitcoin.EventBus.CoreEvents.ConsensusManagerStatusEvent")) {
            var syncStatus = document.getElementById('lblMainchainSyncStatus');
            if (!message.isIbd) {
                syncStatus.innerHTML = `Synced 100.00%`;
                syncStatus.className = "badge badge-success";
            }
            else {
                syncStatus.innerHTML = 'Syncing';
                syncStatus.className = "badge badge-warning";
            }

            if (message.headerHeight) {
                document.getElementById('lblMainchainNodeHeaderHeight').innerHTML = ` ${message.headerHeight}`;
            }

        }       
        
        if (message.nodeEventType.includes("Stratis.Bitcoin.EventBus.CoreEvents.PeerConnectionInfoEvent")) {
            var mainchainconnections = '';
            var inbountCount = 0;
            var filtered = message.peerConnectionModels.filter(function (d) {
                if (d.inbound) {
                    inbountCount++;
                }
            });

            document.getElementById('lblMainchainTotalConnectedNode').innerHTML = ` ${message.peerConnectionModels.length}` + ' /';
            document.getElementById('lblMainchainTotalInNode').innerHTML = inbountCount + ' /';
            document.getElementById('lblMainchainTotalOutNode').innerHTML = (` ${message.peerConnectionModels.length}` - inbountCount);

            var peerconnections = message.peerConnectionModels;

            for (var i = 0; i < peerconnections.length; i++) {
                var type = (peerconnections[i].inbound) ? "inbound" : "outbound";
                mainchainconnections += "<tr>";
                mainchainconnections += "<td class='text-left' style='width: 250px;'>" + peerconnections[i].address + "</td>";
                mainchainconnections += "<td class='text-center' style='width: 150px;'>" + type + "</td>";
                mainchainconnections += "<td class='text-center' style='width: 150px;'>" + peerconnections[i].height + "</td>";
                mainchainconnections += "<td class='text-left' style='width: 450px;'>" + peerconnections[i].subversion + "</td>";
                mainchainconnections += "</tr>";
            }

            document.getElementById("mainchain-peerconnection-data").innerHTML = mainchainconnections;
        }

        if (message.nodeEventType.includes("Stratis.Bitcoin.EventBus.CoreEvents.FederationWalletStatusEvent")) {
            document.getElementById('lblMainchainFederationWalletAmountConfirmed').innerHTML = message.confirmedBalance.toString("N2");
            document.getElementById('lblMainchainFederationWalletAmountUnconfirmed').innerHTML = message.unconfirmedBalance.toString("N2");
        }

    });
}