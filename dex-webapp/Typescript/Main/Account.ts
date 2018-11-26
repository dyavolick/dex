/// <reference path="../../node_modules/@types/toastr/index.d.ts" />

import { getClient, Web3 } from "../Common/Web3Accessor"
import { MetamaskAddressChecker, IMetamaskAddressCheckerOptions } from "../Common/MetamaskAddressChecker";

export class Account {
  client: Web3;
  checker: MetamaskAddressChecker;

  constructor() {
    this.client = getClient();
    var isInstalled = !!this.client;
    if (isInstalled) {
      setInterval(function () {
        var client = getClient();
        if (isInstalled) {
          var netId = client.eth.net.getId();
          client.eth.getAccounts((err, accounts) => {
            configurePage(isInstalled, accounts, netId);
          });
        }
      }, 500);
    } else {
      configurePage(isInstalled, null, null);
    }
  }
}

var BlockedClose = false;
var EthNet = false;

function configurePage(isInstalled, accounts, netId) {
  if (isInstalled) {
    $('.LinkSignin').hide();
    var isBlocked = isInstalled && (accounts == null || (accounts as any as Array<string>).length == 0);
    if (isBlocked) {
      $('#TestNetMetamask').modal('hide');
      if (!BlockedClose) {
        BlockedClose = true;
        $('.LinkSigninBlocked').show();
        if (!$('#BlockedMetamask').hasClass('show')) {
          $('#BlockedMetamask').modal('show');
        }
      }
    } else {
      BlockedClose = false;
      $('.LinkSigninBlocked').hide();
      $('#BlockedMetamask').modal('hide');

      var text;
      netId.then(netId => {
        if (EthNet !== netId) {
          EthNet = netId;
          if (netId === 1)
            $('#TestNetClose').modal('hide');
          else {
            switch (netId) {
              case 2:
                text = 'This is the deprecated Morden test network';
                $("#TestNetMetamaskModalLabel").html(text);
                break
              case 3:
                text = 'This is the ropsten test network.';
                $("#TestNetMetamaskModalLabel").html(text);
                break
              case 4:
                text = 'This is the rinkenby test network.';
                $("#TestNetMetamaskModalLabel").html(text);
                break
              case 42:
                text = 'This is the kovan test network.';
                $("#TestNetMetamaskModalLabel").html(text);
                break
              default:
                text = 'This is an unknown network.';
                $("#TestNetMetamaskModalLabel").html(text);
            }
            //$('#TestNetMetamask').modal('show');
          }
        }
      })
    }
  } else {
    $('.LinkSigninBlocked').hide();
    $('.LinkSignin').show();
  }
}

export function initialize() {
  let obj = new Account();
}
