import { HttpClient, HttpParams } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { environment } from "src/environments/environment";
import { HostAccount } from "../_models/hostAccount";
import { Message } from "../_models/message";
import { EmailParams } from "../_params/emailParams";

@Injectable({
  providedIn: 'root'
})
export class EmailService {
  baseUrl = environment.imapUrl;
  hosts: HostAccount[] = [];
  
  constructor(private http: HttpClient) {}

  getHosts() {
    return fetch('../../assets/hosts.json').then(res => res.json())
    .then(jsonData => {
      this.hosts = jsonData;
      this.hosts.forEach(h => h.email = `${h.userName}@${h.host}`)
      return this.hosts;
    });
  }

  getMessages(emailParams: EmailParams) {
    let params = new HttpParams();
    params = params.append('AccountName', emailParams.accountName);
    return this.http.get<Message[]>(this.baseUrl, { params });
  }
}