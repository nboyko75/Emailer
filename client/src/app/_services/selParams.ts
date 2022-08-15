export class SelParams {
  accountName: string;
  ids: string;

  constructor (accName: string, sids: string) {
    this.accountName = accName;
    this.ids = sids;
  } 
};