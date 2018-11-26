export async function getEtherDeltaContractAddress(): Promise<string> {
  var url = "/api/appsettings/etherdeltaaddress";
  var res = await $.getJSON(url);
  return res.result as string;
}

