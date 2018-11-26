import { Web3, getClient } from "./Web3Accessor";

export class Web3Helper {
  constructor(private client?: Web3) {
    if (!client) {
      this.client = getClient();
    }

  }

  public async getCurrentNetwork(): Promise<number> {
    return await this.client.eth.net.getId();
  }

  public async getCurrentAddress(): Promise<string> {
    let accounts = await this.client.eth.getAccounts();
    return accounts.length > 0 ? accounts[0] : null;
  }

  public async getCurrentBalance(): Promise<number> {
    let address = await this.getCurrentAddress();
    if (!address)
        return null;
    return await this.client.eth.getBalance(address);
  }
}