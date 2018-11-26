import { Web3, getClient } from "./Web3Accessor";
import { tokenAbi } from "./ContractOptions";
import { Web3Helper } from "./Web3Helper";

export async function createTokenContractHelper(tokenAddress: string, client?: Web3) { // call this to create TokenContractHelper instance
    let tokenContractHelper = new TokenContractHelper(tokenAddress, this.client);
    await tokenContractHelper.init();
    return tokenContractHelper;
}

export class TokenContractHelper {
  tokenAddress: string;
  decimals: number = 0;

  constructor(tokenAddress: string, private client?: Web3) {
    if (!client) {
      this.client = getClient();
      this.tokenAddress = tokenAddress;
    }
  }

  public async init() {
    let contract = new this.client.eth.Contract(tokenAbi, this.tokenAddress) as any;
    let helper = new Web3Helper(this.client);

    this.decimals = Number(await contract.methods.decimals().call({ from: await helper.getCurrentAddress()}));
  }

  public async getBalance(): Promise<number> {
    let contract = new this.client.eth.Contract(tokenAbi, this.tokenAddress);
    let helper = new Web3Helper(this.client);
    let userAddress = await helper.getCurrentAddress();
    let res = await contract.methods.balanceOf(userAddress).call();
    return res;
  }

  public async approve(to: string, tokens: string) {
    let contract = new this.client.eth.Contract(tokenAbi, this.tokenAddress) as any;
    let helper = new Web3Helper(this.client);
    let userAddress = await helper.getCurrentAddress();
    let gasPrice = await this.client.eth.getGasPrice(); //this line is IMPORTANT (transaction somehow fails without it)
    let tokenUnitsAmount = this.tokensToTokenUnits(tokens);
    await contract.methods.approve(to, tokenUnitsAmount).send({
      value: 0,
      from: userAddress
    });
  }
  
  public tokensToTokenUnits(tokens: string): string {
    const BN = this.client.utils.BN;
    const val = new BN(this.client.utils.toWei(tokens, 'ether'));
    const corrcet = new BN((10 ** (18 - this.decimals)).toString());
    return val.div(corrcet).toString();;
  }
  
  public tokensAmountFormat(tokenUnits: number | string) : string {
    const val = Number(this.client.utils.fromWei(tokenUnits, 'ether')) * (10 ** (18 - this.decimals));
    return val.toFixed(8);
  }
  public tokensAmount(tokenUnits: number | string): number {
    return Number(this.client.utils.fromWei(tokenUnits, 'ether')) * (10 ** (18 - this.decimals));
  }
}
