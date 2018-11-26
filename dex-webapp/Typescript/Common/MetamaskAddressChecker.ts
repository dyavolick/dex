import { getClient } from "./Web3Accessor";
import {Web3Helper} from "./Web3Helper";

export class MetamaskAddressChecker {
  constructor(protected options: IMetamaskAddressCheckerOptions) {
    this.currentAddress = options.startingAddress;
    if (options.intervalMilliseconds) {
      this.interval = options.intervalMilliseconds;
    }
  }

  protected currentAddress: string;
  protected currentNetwork: number;
  protected addressCheckerInterval: any;
  protected interval: number = 500;

  public start() {
    if (this.addressCheckerInterval) {
      return;
    }

    this.addressCheckerInterval = setInterval(async () => { await this.checkAddress(); }, this.interval);

  }

  public stop() {
    clearInterval(this.addressCheckerInterval);
    delete this.addressCheckerInterval;
  }

  private async checkAddress() {

    let helper = new Web3Helper();
    let address = await helper.getCurrentAddress();

    let network = await helper.getCurrentNetwork();

    if (this.currentAddress !== address) {
      this.currentAddress = address;
      if (this.options.addressChanged) {
        await this.options.addressChanged(this.currentAddress);
      }
    }

    if (this.currentNetwork !== network) {
      this.currentNetwork = network;
      if (this.options.networkChanged) {
        await this.options.networkChanged(this.currentNetwork);
      }
    }
  }

}

export interface IMetamaskAddressCheckerOptions {
  startingAddress?: string;
  intervalMilliseconds?: number;
  addressChanged?: (newAddress: string) => Promise<void>;
  networkChanged?: (newNetwork: number) => Promise<void>;
}