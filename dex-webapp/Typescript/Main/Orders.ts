/// <reference path="../../node_modules/@types/toastr/index.d.ts" />

import { getClient, Web3 } from "../Common/Web3Accessor"
import { Web3Helper } from "../Common/Web3Helper";
import { EtherDeltaContractHelper } from "../Common/EtherDeltaContractHelper";
import { createTokenContractHelper, TokenContractHelper } from "../Common/TokenContractHelper";

const ContractEthAddress = "0x0000000000000000000000000000000000000000";

export class Orders {
  addressInput: JQuery;
  client: Web3;
  web3Helper: Web3Helper;
  etherDeltaHelper: EtherDeltaContractHelper;
  tokenContractHelper: TokenContractHelper;


  order: any;
  feeTake: number;
  lastBlock: number;
  constructor() {
    this.init();
  }

  async init() {
    this.client = getClient();
    this.web3Helper = new Web3Helper(this.client);
    this.etherDeltaHelper = new EtherDeltaContractHelper(this.client);
    this.tokenContractHelper = await createTokenContractHelper((<any>window).tokenAddress, this.client);

    this.feeTake = Number(this.etherDeltaHelper.feeFormat(await this.etherDeltaHelper.getFee("feeTake", (<any>window).tokenAddress)));
    this.lastBlock = await this.client.eth.getBlockNumber();

    $(".js-submitrade").click(async (event) => {
      const $container = $(event.target).closest(".modal");
      const data = $container.data("order");
      const order = JSON.parse(atob(data));
      const amount = $container.find(".js-amount").val();
      await this.etherDeltaHelper.tradeOrder(order, amount.toString());
      $container.hide();
    });

    $(".js-buyorders").click(".js-buy", async (event) => {
      var data = $(event.target).data("order");
      var order = JSON.parse(atob(data));
      this.order = order;
      const tokenAmount = this.tokenContractHelper.tokensAmountFormat(order.amountGive);
      const ethAmount = this.client.utils.fromWei(order.amountGet, 'ether');
      const price = parseFloat(ethAmount) / parseFloat(tokenAmount);

      $('.js-buymodal .js-avaliableamount ').text(tokenAmount);
      $('.js-buymodal .js-amount ').val(tokenAmount);

      $('.js-buymodal .js-expires ').text(order.expires);
      $('.js-buymodal .js-price ').text(price.toFixed(8));
      $('.js-buymodal .js-eth ').text(Number(ethAmount).toFixed(8));
      $('.js-buymodal .js-fee ').text((ethAmount * this.feeTake).toFixed(8));
      $('.js-buymodal .js-feevalue').val(this.feeTake);

      $('.js-buymodal').attr("data-order", data);
      $('.js-buymodal').show();
    });
    $(".js-sellorders").click(".js-buy", async (event) => {
      var data = $(event.target).data("order");
      var order = JSON.parse(atob(data));
      this.order = order;

      const tokenAmount = this.tokenContractHelper.tokensAmountFormat(order.amountGet);
      const ethAmount = this.client.utils.fromWei(order.amountGive, 'ether');
      const price = parseFloat(ethAmount) / parseFloat(tokenAmount);

      $('.js-sellmodal .js-avaliableamount ').text(tokenAmount);
      $('.js-sellmodal .js-amount ').val(tokenAmount);
      $('.js-sellmodal .js-expires ').text(order.expires);
      $('.js-sellmodal .js-price ').text(price.toFixed(8));
      $('.js-buymodal .js-eth ').text(Number(ethAmount).toFixed(8));
      $('.js-buymodal .js-fee ').text((ethAmount * this.feeTake).toString());
      $('.js-buymodal .js-feevalue').val(this.feeTake);

      $('.js-sellmodal').attr("data-order", data);
      $('.js-sellmodal').show();
    });
    $(".js-buymodal .js-amount, .js-sellmodal .js-amount").on('input', function () {
      var $container = $(this).closest(".modal");
      var price = Number($container.find(".js-price").text());
      var amount = Number($container.find(".js-amount").val());
      var expires = Number($container.find(".js-expires").text());
      var feeTake = Number($container.find('.js-feevalue').val());

      var total = null;
      if (!price || price <= 0 || !amount || amount <= 0) {
        $container.find(".js-eth").val("");
        $container.find(".js-fee").val("");
      } else {
        total = price * amount;
        $container.find(".js-eth").val(total.toFixed(8));
        $container.find(".js-fee").val((total * feeTake).toFixed(8));
      }
      var isIncorrect = !total || !expires;
      $container.find(".js-submitsell").prop("disabled", isIncorrect);
      $container.find(".js-submitbuy").prop("disabled", isIncorrect);

    });
    $("#createBuyOrderBtn").click(() => {
      this.createOrder(true);
    });
    $("#createSellOrderBtn").click(() => {
      this.createOrder(false);
    });

    this.updateOrders();
    setInterval(async () => {
      if (this.order !== undefined && this.order !== null) {
        const expiresIn = (await this.client.eth.getBlockNumber()) - Number(this.order.expirse);
        const available = await this.etherDeltaHelper.availableVolume(this.order);
        if (this.order.tokenGet === ContractEthAddress) {
          $(".js-sellmodal js-expires").text(expiresIn.toString());
          $(".js-sellmodal js-availablevolume").text(available.toString());
        } else {
          $(".js-buymodal js-expires").text(expiresIn.toString());
          $(".js-buymodal js-availablevolume").text(available.toString());
        }
      }

      this.updateOrders();
    }, 30 * 1000);

  }

  async updateOrders() {
    const address = await this.web3Helper.getCurrentAddress();
    $.ajax(`/api/orders/${(<any>window).tokenSymbol}`).done(data => {
      let sellOrders = [];
      let buyOrders = [];
      let totalSell = 0;
      let totalBuy = 0;
      let highestBuyPrice = null;
      let lowestSellPrice = null;

      data.forEach((order: any) => {
        if (order.tokenGet === ContractEthAddress) {
          const tokenAmount = this.tokenContractHelper.tokensAmountFormat(order.amountGive);
          const ethAmount = this.client.utils.fromWei(order.amountGet, 'ether');
          const price = parseFloat(ethAmount) / parseFloat(tokenAmount);
          const availableAmount = this.client.utils.fromWei(order.available, 'ether') / price;
          const filledAmount = this.client.utils.fromWei(order.filled, 'ether') / price;
          if (!highestBuyPrice || highestBuyPrice < price)
            highestBuyPrice = price;
          totalBuy += parseFloat(ethAmount);
          buyOrders.push({
            tokenAmount,
            price,
            ethAmount: parseFloat(ethAmount),
            availableAmount,
            filledAmount,
            data: (address === undefined || address === null || order.user.toLowerCase() === address.toLowerCase() ? undefined : btoa(JSON.stringify(order)))
          });
        } else {
          const tokenAmount = this.tokenContractHelper.tokensAmountFormat(order.amountGet);
          const ethAmount = this.client.utils.fromWei(order.amountGive, 'ether');
          const price = parseFloat(ethAmount) / parseFloat(tokenAmount);
          const availableAmount = this.tokenContractHelper.tokensAmount(order.available) * price;
          const filledAmount = this.tokenContractHelper.tokensAmount(order.filled) * price;
          if (!lowestSellPrice || lowestSellPrice > price)
            lowestSellPrice = price;
          totalSell += parseFloat(tokenAmount);
          sellOrders.push({
            tokenAmount,
            price,
            ethAmount: parseFloat(ethAmount),
            availableAmount,
            filledAmount,
            data: (address === undefined || address === null || order.user.toLowerCase() === address.toLowerCase() ? undefined : btoa(JSON.stringify(order)))
          });
        }

      });
      $(".js-totalbuy").text(totalBuy.toFixed(8));
      $(".js-totalsell").text(totalSell.toFixed(8));
      buyOrders = buyOrders.sort((o1, o2) => o2.price - o1.price);
      sellOrders = sellOrders.sort((o2, o1) => o2.price - o1.price);

      $(".js-buyorders").html(this.renderTemplate(buyOrders, true));
      $(".js-sellorders").html(this.renderTemplate(sellOrders, false));
      $(".highest-buy").text(highestBuyPrice ? highestBuyPrice.toFixed(6) + "ETH" : "-");
      $(".lowest-sell").text(lowestSellPrice ? lowestSellPrice.toFixed(6) + "ETH" : "-");
    });
  }
  renderTemplate(orders: Array<any>, isbuy: boolean) : string {
    let template = "";
    orders.forEach(order => {
      template += '<tr>' +
        `<td>${order.tokenAmount}</td>` +
        `<td>${order.price.toFixed(8)}</td>` +
        `<td>${order.ethAmount.toFixed(8)}</td>` +
        `<td>${order.availableAmount.toFixed(8)}</td>` +
        //`<td>${order.filledAmount.toFixed(8)}</td>` +
        (order.data === undefined ? '<td></td>' : `<td> <a class="tbl-link ${(isbuy ? "sell" : "buy")}  js-buy" href="javascript:void(0)" data-order="${order.data}">${(isbuy ? "SELL" : "BUY")}</a></td></tr>`);
    });
    return template;
  }

  async createOrder(isBuy: boolean) {
    let $container = $(isBuy ? ".create-buy-order" : ".create-sell-order");
    let price = $container.find(".order-price").val().toString();
    let tokenAmount = $container.find(".order-amount").val().toString();
    let ethAmount = (Number(price) * Number(tokenAmount)).toString();
    let expires = $container.find(".order-expires").val().toString();
    this.etherDeltaHelper.createOrder(isBuy, (<any>window).tokenAddress, tokenAmount, ethAmount, expires);
  }
}

export function initialize() {
  let obj = new Orders();
}

