/// <reference path="../../node_modules/@types/toastr/index.d.ts" />

import { getClient, Web3 } from "../Common/Web3Accessor"
import { Web3Helper } from "../Common/Web3Helper";
import { EtherDeltaContractHelper } from "../Common/EtherDeltaContractHelper";
import { createTokenContractHelper, TokenContractHelper } from "../Common/TokenContractHelper";

export class MyOrders {
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

    $(".js-myorders").click(".js-cancel", async (event) => {
      const order = JSON.parse(atob($(event.target).data("order")));
      this.etherDeltaHelper.cancelOrder(order);
    });

    this.updateOrders();
    setInterval(async () => {
      this.updateOrders();
    }, 60 * 1000);
  }

  async updateOrders() {
    const address = await this.web3Helper.getCurrentAddress();
    //let address = '0xe98456088a623ae623a9d64672901f7d5a32405c';
    if (address == null) {
      $(".js-myorders").html("");
    } else {
      $.ajax(`/api/orders/${(<any>window).tokenSymbol}/${address}`).done(data => {
        let tableData = "";
        data.forEach(order => {

          const tokenAmount = this.tokenContractHelper.tokensAmountFormat(order.tokenGet === "0x0000000000000000000000000000000000000000" ? order.amountGive : order.amountGet);
          const ethAmount = this.client.utils.fromWei(order.tokenGet === "0x0000000000000000000000000000000000000000" ? order.amountGet : order.amountGive, 'ether');
          const price = parseFloat(ethAmount) / parseFloat(tokenAmount);
          let availableAmount = 0, filledAmount = 0;
          if (order.tokenGet === "0x0000000000000000000000000000000000000000") {
            availableAmount = this.client.utils.fromWei(order.available, 'ether') / price;
            filledAmount = this.client.utils.fromWei(order.filled, 'ether') / price;
          } else {
            availableAmount = this.tokenContractHelper.tokensAmount(order.available) * price;
            filledAmount = this.tokenContractHelper.tokensAmount(order.filled) * price;
          }
        

          tableData += `<tr class='${order.tokenGet === "0x0000000000000000000000000000000000000000" ? 'buy' : 'sell'}'>` +
            `<td>${order.tokenGet === "0x0000000000000000000000000000000000000000" ? 'buy' : 'sell'}</td>` +
            `<td>${tokenAmount}</td>` +
            `<td>${price.toFixed(8)}</td>` +
            `<td>${parseFloat(ethAmount).toFixed(8)}</td>` +
            `<td>${availableAmount.toFixed(8)}</td>` +
            //`<td>${filledAmount.toFixed(8)}</td>` +
            `<td> <a class="tbl-link js-cancel" href="javascript:void(0)" data-order="${btoa(JSON.stringify(order))}"> Cancel </a></td></tr>`;
        });
        $(".js-myorders").html(tableData);
      });
    }
  }
}

export function initialize() {
  let obj = new MyOrders();
}

