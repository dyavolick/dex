window.addEventListener('load', async () => {
  // Modern dapp browsers...
  if (window.ethereum) {
    window.web3 = new Web3(window.ethereum);
    try {
      // Request account access if needed
      await window.ethereum.enable();
      // Acccounts now exposed
    } catch (error) {
      // User denied account access...
      console.log('User denied account access');
      console.log(error);
    }
  }
  // Legacy dapp browsers...
  else if (window.web3) {
    window.web3 = new Web3(web3.currentProvider);
    // Acccounts always exposed
  } else {
    // use own web3 that can use Nano
    require("@babel/polyfill");
    const ProviderEngine = require('web3-provider-engine');
    //const RpcSubprovider = require('web3-provider-engine/subproviders/rpc');
    //var LedgerProvider = require('web3-provider-ledger').default;
    const FetchSubprovider = require('web3-provider-engine/subproviders/fetch');
    const TransportU2F = require('@ledgerhq/hw-transport-u2f').default;
    const createLedgerSubprovider = require('@ledgerhq/web3-subprovider').default;

    const rpcUrl = "http://127.0.0.1:8545";
    const networkId = 1;

    const engine = new ProviderEngine();
    const getTransport = () => TransportU2F.create();
    const ledger = createLedgerSubprovider(getTransport, {
      networkId,
      accountsLength: 5
    });
    engine.addProvider(ledger);
   // engine.addProvider(new FetchSubprovider({ rpcUrl }));

    engine.start();
    window.web3 = new Web3(engine);
  }
  // Non-dapp browsers...
  //else {
  //  console.log('Non-Ethereum browser detected. You should consider trying MetaMask!');
  //}
});
