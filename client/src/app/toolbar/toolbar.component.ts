import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { NgxSpinnerService } from 'ngx-spinner';
import { HostAccount } from '../_models/hostAccount';
import { EmailService } from '../_services/email-service';

@Component({
  selector: 'app-toolbar',
  templateUrl: './toolbar.component.html',
  styleUrls: ['./toolbar.component.scss']
})
export class ToolbarComponent implements OnInit {
  hosts: HostAccount[] = [];
  selectedHost: string = "";
  hasSelected: boolean = false;
  
  constructor(private emailService: EmailService, private spinner: NgxSpinnerService) { }

  ngOnInit(): void {
    this.loadHosts();
    this.emailService.messagesSelected.subscribe({
      next: (hasSelected: boolean) => {
        this.hasSelected = hasSelected;
      }
    });
  }

  loadHosts() {
    this.emailService.getHosts().then(data => {
      this.hosts = data;
      this.selectedHost = this.emailService.getHost();
    });
  }

  changeHost() {
    this.emailService.setHost(this.selectedHost);
  }

  receive() {
    this.spinner.show();
    this.emailService.receive().subscribe({
      next: () => {
        this.spinner.hide();
      },
      error: errmsg => {
        alert(errmsg);
        this.spinner.hide();
      }
    });
  }

  receiveAll() {
  }

  delete() {
    if (confirm("Are you sure to delete selected messages?")) {
      this.spinner.show();
      this.emailService.delete().subscribe({
        next: () => {
          this.spinner.hide();
          this.hasSelected = false;
        },
        error: errmsg => {
          alert(errmsg.error || errmsg.message);
          this.spinner.hide();
        }
      });
    }
  }
}
