import { Component } from '@angular/core';
import { NgxSpinnerService } from "ngx-spinner";

import { HostAccount } from './_models/hostAccount';
import { Message } from './_models/message';
import { EmailParams } from './_params/emailParams';
import { EmailService } from './_services/email-service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  hosts: HostAccount[] = [];
  selectedHost: string = "nboyko75@gmail.com";
  messages: Message[] = [];
  currentMessage: Message = new Message();
  currentRowIndex: number = -1;

  constructor(private emailService: EmailService, private spinner: NgxSpinnerService) {}

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.emailService.getHosts().then(data => {
      this.hosts = data;
    });
    const msgs = sessionStorage.getItem('Messages_' + this.selectedHost);
    if (msgs) {
      this.messages = JSON.parse(msgs);
    }
  }

  receive() {
    const params: EmailParams = {
      accountName: this.selectedHost
    };
    this.spinner.show();
    this.emailService.getMessages(params).subscribe({
      next: msgs => {
        this.messages = msgs;
        sessionStorage.setItem('Messages_' + this.selectedHost, JSON.stringify(msgs));
        this.spinner.hide();
      },
      error: errmsg => {
        console.error(errmsg);
        this.spinner.hide();
      }
    });
  }

  receiveAll() {
    
  }

  onRowClick(index: number) {
    this.currentRowIndex = index;
    this.currentMessage = this.messages[index];
  }
}
