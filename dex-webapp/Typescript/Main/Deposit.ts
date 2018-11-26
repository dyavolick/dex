/// <reference path="../../node_modules/@types/toastr/index.d.ts" />

import { getClient, Web3 } from "../Common/Web3Accessor"
import { Web3Helper } from "../Common/Web3Helper";
import { EtherDeltaContractHelper } from "../Common/EtherDeltaContractHelper";
import { createTokenContractHelper, TokenContractHelper } from "../Common/TokenContractHelper";
import {getEtherDeltaContractAddress} from "../Common/AppsettingsFunctions";

export class Deposit {
    client: Web3;
    web3Helper: Web3Helper;
    etherDeltaHelper: EtherDeltaContractHelper;
    tokenContractHelper: TokenContractHelper;

    constructor() {
      this.init();
    }

    async init() {
      this.web3Helper = new Web3Helper(this.client);
      this.etherDeltaHelper = new EtherDeltaContractHelper(this.client);
      this.tokenContractHelper = await createTokenContractHelper((<any>window).tokenAddress, this.client);
        
      let feeDeposit = this.etherDeltaHelper.feeFormat(await this.etherDeltaHelper.getFee("feeDeposit", (<any>window).tokenAddress));
      let feeWithdraw = this.etherDeltaHelper.feeFormat(await this.etherDeltaHelper.getFee("feeWithdraw", (<any>window).tokenAddress));
      let feeMake = this.etherDeltaHelper.feeFormat(await this.etherDeltaHelper.getFee("feeMake", (<any>window).tokenAddress));
      let feeTake = this.etherDeltaHelper.feeFormat(await this.etherDeltaHelper.getFee("feeTake", (<any>window).tokenAddress));
      $(".feeDeposit").text(feeDeposit);
      $(".feeWithdraw").text(feeWithdraw);
      $(".feeMake").text(feeMake);
      $(".feeTake").text(feeTake);
      
      this.updateBalances();
      setInterval(async () => {
        this.updateBalances();
      }, 5 * 1000);

      $("#depositEthBtn").click(async () => {
        let amount = $("#depositEthInput").val().toString();
        let numAmount = Number(amount);
        if (!numAmount || numAmount <= 0) {
          alert("Incorrect deposit amount value!");
          return;
        }
        $("#depositEthInput").val("");
        await this.etherDeltaHelper.depositEth(amount);
      });

      $("#depositTokenBtn").click(async () => {
        let amount = $("#depositTokenInput").val().toString();
        let numAmount = Number(amount);
        if (!numAmount || numAmount <= 0) {
          alert("Incorrect deposit amount value!");
          return;
        }
        $("#depositTokenInput").val("");
        let etherDeltaContractAddress = await getEtherDeltaContractAddress();
        await this.tokenContractHelper.approve(etherDeltaContractAddress, amount);
        await this.etherDeltaHelper.depositToken(amount, (<any>window).tokenAddress);
      });

      $("#withdrawEthBtn").click(async () => {
        let amount = $("#withdrawEthInput").val().toString();
        let numAmount = Number(amount);
        if (!numAmount || numAmount <= 0) {
          alert("Incorrect withdraw amount value!");
          return;
        }
        $("#withdrawEthInput").val("");
        await this.etherDeltaHelper.withdrawEth(amount);
      });

      $("#withdrawTokenBtn").click(async () => {
        let amount = $("#withdrawTokenInput").val().toString();
        let numAmount = Number(amount);
        if (!numAmount || numAmount <= 0) {
          alert("Incorrect withdraw amount value!");
          return;
        }
        $("#withdrawTokenBtn").val("");
        await this.etherDeltaHelper.withdrawToken(amount.toString(), (<any>window).tokenAddress);
      });
    }

    async updateBalances() {
      let userEthAmount = this.weiToEthFormat(await this.web3Helper.getCurrentBalance());
      let contractEthAmount = this.weiToEthFormat(await this.etherDeltaHelper.getEthBalance());
      $(".userWalletEthBalance").text(userEthAmount);
      $(".tradexEthBalance").text(contractEthAmount);

      let userTokenAmount = this.tokenContractHelper.tokensAmountFormat(await this.tokenContractHelper.getBalance());
      let contractTokenAmount = this.tokenContractHelper.tokensAmountFormat(await this.etherDeltaHelper.getBalance((<any>window).tokenAddress));
      $(".userWalletTokenBalance").text(userTokenAmount);
      $(".tradexTokenBalance").text(contractTokenAmount);
    }

    weiToEthFormat(amount: number) {
      return (amount / 1000000000000000000).toFixed(6);
    }
}

export function initialize() {
    let obj = new Deposit();
}

