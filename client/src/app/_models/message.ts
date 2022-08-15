export class Message {
  id: number = 0;
  accountName: string = "";
  from: string = "";
  to: string = "";
  cc: string = "";
  subject: string = "";
  htmlBody: string = "";
  receiveDate?: Date;
  seen: boolean = true;
  isSelected: boolean = false;
}