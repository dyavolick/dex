/// <reference path="../../node_modules/@types/toastr/index.d.ts" />

import { getClient, Web3 } from "../Common/Web3Accessor"
import { Web3Helper } from "../Common/Web3Helper";
import { EtherDeltaContractHelper } from "../Common/EtherDeltaContractHelper";
import { createTokenContractHelper, TokenContractHelper } from "../Common/TokenContractHelper";

const ContractEthAddress = "0x0000000000000000000000000000000000000000";

export class Trades {
  addressInput: JQuery;
  client: Web3;
  web3Helper: Web3Helper;
  etherDeltaHelper: EtherDeltaContractHelper;
  tokenContractHelper: TokenContractHelper;

  constructor() {
    this.init();
  }

  async init() {
    this.client = getClient();
    this.web3Helper = new Web3Helper(this.client);
    this.etherDeltaHelper = new EtherDeltaContractHelper(this.client);
    this.tokenContractHelper = await createTokenContractHelper((<any>window).tokenAddress, this.client);

    await this.tokenContractHelper.init();

    this.updateTrades();
    setInterval(async () => {
      this.updateTrades();
    }, 30 * 1000);
  }

  async updateTrades() {
    const address = await this.web3Helper.getCurrentAddress();

    $.ajax(`/api/trades/${(<any>window).tokenSymbol}`).done(data => {
      let trades = "";
      data.forEach(trade => {
        const tokenAmount = this.tokenContractHelper.tokensAmountFormat(trade.tokenGet === ContractEthAddress ? trade.amountGive : trade.amountGet);
        const ethAmount = this.client.utils.fromWei(trade.tokenGet === ContractEthAddress ? trade.amountGet : trade.amountGive, 'ether');
        const price = parseFloat(ethAmount) / parseFloat(tokenAmount);

        trades += `<tr><td> ${(new Date(trade.timestamp)).toLocaleString("de-DE")}</td>` +
          `<td>${trade.tokenGet === ContractEthAddress ? 'sell' : 'buy'}</td>` +
          `<td>${price.toFixed(8)}</td>` +
          `<td>${tokenAmount}</td>` +
          `<td>${parseFloat(ethAmount).toFixed(8)}</td>` +
          '</tr>';
      });
      $(".js-trades").html(trades);
    });
    $.ajax(`/api/trades/${(<any>window).tokenSymbol}/${address}`).done(data => {
      let trades = "";
      data.forEach(trade => {
        const tokenAmount = this.tokenContractHelper.tokensAmountFormat(trade.tokenGet === ContractEthAddress ? trade.amountGive : trade.amountGet);
        const ethAmount = this.client.utils.fromWei(trade.tokenGet === ContractEthAddress ? trade.amountGet : trade.amountGive, 'ether');
        const price = parseFloat(ethAmount) / parseFloat(tokenAmount) ;
        trades += `<tr><td> ${(new Date(trade.timestamp)).toLocaleString("de-DE")}</td>` +
          `<td>${trade.tokenGet === ContractEthAddress ? 'sell' : 'buy'}</td>` +
          `<td>${price.toFixed(8)}</td>` +
          `<td>${tokenAmount}</td>` +
          `<td>${parseFloat(ethAmount).toFixed(8)}</td>` +
          '</tr>';
      });
      $(".js-mytrades").html(trades);
    });
  }
}

export function initialize() {
  let obj = new Trades();
}

