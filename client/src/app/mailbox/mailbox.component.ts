import { Component, OnInit } from '@angular/core';

import { Message } from '../_models/message';
import { EmailService } from '../_services/email-service';

@Component({
  selector: 'app-mailbox',
  templateUrl: './mailbox.component.html',
  styleUrls: ['./mailbox.component.scss']
})
export class MailboxComponent implements OnInit {
  selectedHost: string = "";
  messages: Message[] = [];
  currentMessage: Message = new Message();
  currentRowIndex: number = -1;

  constructor(private emailService: EmailService) {}

  ngOnInit() {
    this.emailService.messagesLoaded.subscribe({
      next: (msgs: Message[]) => {
        this.messages = msgs;
        this.clear();
      }
    });
  }

  onRowClick(index: number) {
    this.currentRowIndex = index;
    this.currentMessage = this.messages[index];
    this.emailService.markAsRead(this.currentMessage).subscribe();
  }

  onSelChange(msg: Message) {
    msg.isSelected = !msg.isSelected;
    this.emailService.selectMessage(msg);
  }

  private clear() {
    const existedMessage = this.currentMessage && this.messages.find(msg => msg.id == this.currentMessage.id);
    if (!existedMessage) {
      this.currentRowIndex = -1;
      this.currentMessage = new Message();
    }
  }
}