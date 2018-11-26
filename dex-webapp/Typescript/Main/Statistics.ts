/// <reference path="../../node_modules/@types/toastr/index.d.ts" />

import { getClient, Web3 } from "../Common/Web3Accessor"
import { MetamaskAddressChecker, IMetamaskAddressCheckerOptions } from "../Common/MetamaskAddressChecker";

export class Statistics {
  sChecker;

  constructor() {
    this.init();
  }
  errorHandler(res, textStatus, errorThrown) {
    if (res.status === 400 && res.responseJSON) {
      console.log(res.responseJSON.message);
    } else if (res.status === 401) {
      console.log('You should authenticate before adding order.');
    } else {
      console.log(errorThrown);
    }
}

  async init() {
    var currencyPairId = (<any>window).tokenSymbol;
    var start = new Date();
    start.setDate(start.getDate() - 1);
    var end = new Date();
    var url = window.location.protocol + '//' + window.location.hostname + (window.location.port !== undefined ? ':' + window.location.port : '') + '/api/trades/GetOHLC/10/' + currencyPairId + '/' + start.toISOString() + '/' + end.toISOString();
    console.log(url);
    $.ajax({
      type: 'get',
      url: url
    })
      .fail(this.errorHandler)
      .done((result) => {
        console.log(result);
        if (result && result.length) {
          let data = result[result.length - 1];
          let difference = data.max - data.min;
          $(".js-lastvolume").html('<strong>' + data.volume.toFixed(8) + '</strong> <span class="name">' + currencyPairId + '</span> / <strong>' + data.volumeBase.toFixed(8) + '</strong> ETH');
          $(".js-lastprice").html(data.close.toFixed(8) + ' ETH');
          $(".js-minprice").html(data.min.toFixed(8));
          $(".js-maxprice").html(data.max.toFixed(8));
          $(".js-difference").html(difference.toFixed(8) + ' ETH');
          //$(".js-serverdate").html(new Date(data.date).toLocaleDateString('ru-RU') + ' ' + new Date(data.date).toLocaleTimeString('ru-RU'));
        } else {
          let data = { volume: 0.0, max: 0.0, min: 0.0, volumeBase: 0.0, close: 0.0 };
          let difference = 0;

          $(".js-lastvolume").html('<strong>' + data.volume.toFixed(8) + '</strong> <span class="name">' + currencyPairId + '</span> / <strong>' + data.volumeBase.toFixed(8) + '</strong> ETH');
          $(".js-lastprice").html(data.close.toFixed(8) + ' ETH');
          $(".js-minprice").html(data.min.toFixed(8));
          $(".js-maxprice").html(data.max.toFixed(8));
          $(".js-difference").html(difference.toFixed(8) + ' ETH');
        }
      });
  }
}

export function initialize() {
  let obj = new Statistics();
}
