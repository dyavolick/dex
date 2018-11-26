import { Web3, getClient } from "./Web3Accessor";
import { etherDeltaAbi } from "./ContractOptions";
import { Web3Helper } from "./Web3Helper";
import { getEtherDeltaContractAddress } from "./AppsettingsFunctions";
import { createTokenContractHelper, TokenContractHelper } from "./TokenContractHelper";

const ContractEthAddress = "0x0000000000000000000000000000000000000000";

export class EtherDeltaContractHelper {
  constructor(private client?: Web3) {
    if (!client) {
      this.client = getClient();
    }
  }

  public async getEthBalance(): Promise<number> {
    return await this.getBalance(ContractEthAddress);
  }

  public async getBalance(tokenAddress: string): Promise<number> {
    let etherDeltaContractAddress = await getEtherDeltaContractAddress();
    let contract = new this.client.eth.Contract(etherDeltaAbi, etherDeltaContractAddress) as any;
    let helper = new Web3Helper(this.client);
    let userAddress = await helper.getCurrentAddress();
    let res = await contract.methods.balanceOf(tokenAddress, userAddress).call();
    return res;
  }

  public async getFee(feeType: string, tokenAddress: string): Promise<number> {
    let etherDeltaContractAddress = await getEtherDeltaContractAddress();
    let contract = new this.client.eth.Contract(etherDeltaAbi, etherDeltaContractAddress) as any;
    let helper = new Web3Helper(this.client);
    let userAddress = await helper.getCurrentAddress();
    let result;
    switch (feeType) {
      case "feeMake": result = await contract.methods.feeMake(tokenAddress).call();
        break;
      case "feeTake": result = await contract.methods.feeTake(tokenAddress).call();
        break;
      case "feeDeposit": result = await contract.methods.feeDeposit(tokenAddress).call();
        break;
      case "feeWithdraw": result = await contract.methods.feeWithdraw(tokenAddress).call();
        break;
    }
    return result;
  }
  public feeFormat(fee): string { // return percentage
    return (Number(fee) / 10000000000000000).toFixed(2);
  }

  public async depositEth(ethAmount: string) {
    let etherDeltaContractAddress = await getEtherDeltaContractAddress();
    let contract = new this.client.eth.Contract(etherDeltaAbi, etherDeltaContractAddress) as any;
    let helper = new Web3Helper(this.client);
    let userAddress = await helper.getCurrentAddress();
    let wei = this.client.utils.toWei(ethAmount, "ether");
    let gasPrice = await this.client.eth.getGasPrice(); //this line is IMPORTANT (transaction somehow fails without it)
    let observer = contract.methods.deposit().send({
      value: wei,
      from: userAddress
    });
  }

  public async depositToken(tokenAmount: string, tokenAddress: string) {
    let etherDeltaContractAddress = await getEtherDeltaContractAddress();
    let contract = new this.client.eth.Contract(etherDeltaAbi, etherDeltaContractAddress) as any;
    let web3helper = new Web3Helper(this.client);
    let userAddress = await web3helper.getCurrentAddress();
    let tokenHelper = await createTokenContractHelper((<any>window).tokenAddress, this.client);
    let tokenUnitsAmount = tokenHelper.tokensToTokenUnits(tokenAmount);
    //let wei = this.client.utils.toWei(ethAmount, "ether"); todo convert
    let gasPrice = await this.client.eth.getGasPrice(); //this line is IMPORTANT (transaction somehow fails without it)
    let observer = contract.methods.depositToken(tokenAddress, tokenUnitsAmount).send({
      value: 0,
      from: userAddress
    });
  }

  public async withdrawEth(ethAmount: string) {
    let etherDeltaContractAddress = await getEtherDeltaContractAddress();
    let contract = new this.client.eth.Contract(etherDeltaAbi, etherDeltaContractAddress) as any;
    let helper = new Web3Helper(this.client);
    let userAddress = await helper.getCurrentAddress();
    let wei = this.client.utils.toWei(ethAmount, "ether");
    let gasPrice = await this.client.eth.getGasPrice(); //this line is IMPORTANT (transaction somehow fails without it)
    let observer = contract.methods.withdraw(wei).send({
      value: 0,
      from: userAddress
    });
  }

  public async withdrawToken(tokenAmount: string, tokenAddress: string) {
    let etherDeltaContractAddress = await getEtherDeltaContractAddress();
    let contract = new this.client.eth.Contract(etherDeltaAbi, etherDeltaContractAddress) as any;
    let web3helper = new Web3Helper(this.client);
    let userAddress = await web3helper.getCurrentAddress();
    let tokenHelper = await createTokenContractHelper((<any>window).tokenAddress, this.client);
    let tokenUnitsAmount = tokenHelper.tokensToTokenUnits(tokenAmount);
    //let wei = this.client.utils.toWei(ethAmount, "ether"); todo convert
    let gasPrice = await this.client.eth.getGasPrice(); //this line is IMPORTANT (transaction somehow fails without it)
    console.log(tokenAmount);
    let observer = contract.methods.withdrawToken(tokenAddress, tokenUnitsAmount).send({
      value: 0,
      from: userAddress
    });
  }

  public async createOrder(isBuy: boolean, tokenAddress: string, tokenAmount: string, ethAmount: string, expires: string) {
    let etherDeltaContractAddress = await getEtherDeltaContractAddress();
    let contract = new this.client.eth.Contract(etherDeltaAbi, etherDeltaContractAddress) as any;
    let web3helper = new Web3Helper(this.client);
    let userAddress = await web3helper.getCurrentAddress();
    let wei = this.client.utils.toWei(ethAmount, "ether");
    let tokenHelper = await createTokenContractHelper((<any>window).tokenAddress, this.client);
    let tokenUnitsAmount = tokenHelper.tokensToTokenUnits(tokenAmount);
    let lastBlockNumber = await this.client.eth.getBlockNumber();
    expires = (Number(expires) + lastBlockNumber).toString();
    let tokenGet = isBuy ? ContractEthAddress : tokenAddress,
      amountGet = isBuy ? wei : tokenUnitsAmount,
      tokenGive = isBuy ? tokenAddress : ContractEthAddress,
      amountGive = isBuy ? tokenUnitsAmount : wei,
      nonce = Math.floor(Math.random() * Math.floor(99999999999)).toString();

    let gasPrice = await this.client.eth.getGasPrice(); //this line is IMPORTANT (transaction somehow fails without it)
    let observer = contract.methods.order(tokenGet, amountGet, tokenGive, amountGive, expires, nonce).send({
      value: 0,
      from: userAddress
    });
  }

  public async cancelOrder(order: any) {
    let etherDeltaContractAddress = await getEtherDeltaContractAddress();
    let contract = new this.client.eth.Contract(etherDeltaAbi, etherDeltaContractAddress) as any;
    let web3Helper = new Web3Helper(this.client);
    let userAddress = await web3Helper.getCurrentAddress();

    let gasPrice = await this.client.eth.getGasPrice(); //this line is IMPORTANT (transaction somehow fails without it)
    let observer = contract.methods.cancelOrder(order.tokenGet, order.amountGet, order.tokenGive, order.amountGive, order.expires, order.nonce, 0, "0x00", "0x00").send({
      value: 0,
      from: userAddress
    });
  }

  public async availableVolume(order: any) {
    let etherDeltaContractAddress = await getEtherDeltaContractAddress();
    let contract = new this.client.eth.Contract(etherDeltaAbi, etherDeltaContractAddress) as any;
    let web3Helper = new Web3Helper(this.client);
    let userAddress = await web3Helper.getCurrentAddress();

    let gasPrice = await this.client.eth.getGasPrice(); //this line is IMPORTANT (transaction somehow fails without it)
    let available = await contract.methods.availableVolume(order.tokenGet, order.amountGet, order.tokenGive, order.amountGive, order.expires, order.nonce, order.user).call({
      from: userAddress
    });

    if (order.tokenGet === ContractEthAddress) {
      const tokenContract = new TokenContractHelper(order.tokenGive, this.client) as any;
      return tokenContract.tokensAmountFormat(available);
    } else {
      return this.client.utils.fromWei(available, 'ether');
    }
  }

  public async amountFilled(order: any) {
    let etherDeltaContractAddress = await getEtherDeltaContractAddress();
    let contract = new this.client.eth.Contract(etherDeltaAbi, etherDeltaContractAddress) as any;
    let web3Helper = new Web3Helper(this.client);
    let userAddress = await web3Helper.getCurrentAddress();

    let gasPrice = await this.client.eth.getGasPrice(); //this line is IMPORTANT (transaction somehow fails without it)
    let filled = await contract.methods.amountFilled(order.tokenGet, order.amountGet, order.tokenGive, order.amountGive, order.expires, order.nonce, order.user).call({
      from: userAddress
    });
    return filled;

  }

  public async tradeOrder(order: any, amount: string) {
    console.log('tradeOrder');
    const etherDeltaContractAddress = await getEtherDeltaContractAddress();
    const contract = new this.client.eth.Contract(etherDeltaAbi, etherDeltaContractAddress) as any;
    const web3Helper = new Web3Helper(this.client);
    const userAddress = await web3Helper.getCurrentAddress();

    const tokenContract = new TokenContractHelper(order.tokenGet === ContractEthAddress ? order.tokenGive: order.tokenGet, this.client) as any;

    let amountValue = '';
    if (order.tokenGet === ContractEthAddress) {
      const tokenAmount = tokenContract.tokensAmountFormat(order.amountGive);
      const ethAmount = this.client.utils.fromWei(order.amountGet, 'ether');
      const price = parseFloat(ethAmount) / parseFloat(tokenAmount);
      const v = price * parseFloat(amount);
      amountValue = this.client.utils.toWei(v.toString(), 'ether');
    } else {
      amountValue = tokenContract.tokensToTokenUnits(amount);
    }
    var test = await this.testTrade(order, amountValue);
    if (test) {
      let gasPrice =
        await this.client.eth.getGasPrice(); //this line is IMPORTANT (transaction somehow fails without it)
      let observer = contract.methods.trade(order.tokenGet, order.amountGet, order.tokenGive, order.amountGive, order.expires, order.nonce, order.user, 0, "0x00", "0x00", amountValue)
        .send({ value: 0, from: userAddress });
    } else {
      alert('trade can not be done');
    }
  }

  public async testTrade(order: any, amount: string) : Promise<boolean> {
    console.log('testTrade');
    let etherDeltaContractAddress = await getEtherDeltaContractAddress();
    let contract = new this.client.eth.Contract(etherDeltaAbi, etherDeltaContractAddress) as any;
    let web3Helper = new Web3Helper(this.client);
    let userAddress = await web3Helper.getCurrentAddress();

    let gasPrice = await this.client.eth.getGasPrice(); //this line is IMPORTANT (transaction somehow fails without it)
    let result = await contract.methods.testTrade(order.tokenGet, order.amountGet, order.tokenGive, order.amountGive, order.expires, order.nonce, order.user, 0, "0x00", "0x00", amount, userAddress).call({
      from: userAddress
    });
    return result;
  }
  /*public async buyTokens(ethAmount: string): Promise<string> {
    let crowdsaleContractAddress = await getCrowdsaleAddress();
    let contract = new this.client.eth.Contract(crowdsaleAbi, crowdsaleContractAddress) as any;
    let helper = new Web3Helper(this.client);
    let address = await helper.getCurrentAddress();
    let wei = this.client.utils.toWei(ethAmount, "ether");
    let gasPrice = await this.client.eth.getGasPrice(); //this line is IMPORTANT (transaction somehow fails without it)
    let observer = contract.methods.buyTokens(address).send({
      value: wei,
      from: address
    });

    let defHash = $.Deferred<string>();

    observer
      .on("transactionHash", (hash: string) => {
        defHash.resolve(hash);
      })
      .on("error", (error) => {
        defHash.reject();
        console.error(error);
      });

    return await defHash.promise();

  }*/
}
