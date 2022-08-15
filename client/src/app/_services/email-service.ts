import { HttpClient, HttpHeaders, HttpParams } from "@angular/common/http";
import { EventEmitter, Injectable } from "@angular/core";
import { catchError, tap, throwError } from "rxjs";
import { environment } from "src/environments/environment";
import { HostAccount } from "../_models/hostAccount";
import { Message } from "../_models/message";
import { SelParams } from "./selParams";

@Injectable({
  providedIn: 'root'
})
export class EmailService {
  baseUrl = environment.imapUrl;
  messagesLoaded = new EventEmitter<Message[]>();

  private hosts: HostAccount[] = [];
  private messages: Message[] = [];
  private selectedHost: string = "nboyko75@gmail.com";
  private hostIsLoaded: boolean = false;
  private messagesIsLoaded: boolean = false;
  
  constructor(private http: HttpClient) { }

  getHost() {
    return this.selectedHost;
  }

  setHost(host: string) {
    this.selectedHost = host;
  }

  private loadHosts() {
    const host = sessionStorage.getItem('SelectedHost');
    if (host) {
      this.selectedHost = host;
    }
    return fetch('../../assets/hosts.json').then(res => res.json())
      .then(jsonData => {
        this.hosts = jsonData;
        this.hosts.forEach(h => h.email = `${h.userName}@${h.host}`)
        this.hostIsLoaded = true;
        return this.hosts;
      });
  }

  getHosts() {
    if (this.hostIsLoaded) {
      return Promise.resolve(this.hosts);
    } else {
      return this.loadHosts();
    }
  }

  private loadMessages() {
    const msgs = sessionStorage.getItem('Messages_' + this.selectedHost);
    if (msgs) {
      this.messages = JSON.parse(msgs);
      this.messagesIsLoaded = true;
      this.messagesLoaded.emit(this.messages);
    }
  }

  getMessages() {
    if (!this.messagesIsLoaded) {
      this.loadMessages();
    }
    return this.messages;
  }

  receive() {
    let params = new HttpParams();
    params = params.append('AccountName', this.selectedHost);
    return this.http.get<Message[]>(this.baseUrl, { params }).pipe(
      tap(msgs => {
        this.messages = msgs;
        sessionStorage.setItem('Messages_' + this.selectedHost, JSON.stringify(msgs));
        this.messagesLoaded.emit(this.messages);
      })
    );
  }

  selectMessage(msg: Message) {
    const selMsg = this.messages.find(m => m.id == msg.id);
    if (selMsg != null) selMsg.isSelected = msg.isSelected;
  }

  delete() {
    const ids = this.messages.filter(msg => msg.isSelected).map(msg => msg.id);
    const delmsgs = ids.join();
    const httpOptions = {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' }),
      body: new SelParams(this.selectedHost, delmsgs)
    };
    return this.http.delete(this.baseUrl, httpOptions).pipe(
      tap(errmsg => {
        if (this.checkErrMsg(errmsg)) {
          this.messages = this.messages.filter(msg => !ids.includes(msg.id));
          this.messagesLoaded.emit(this.messages);
        }
      })
    );
  }

  markAsRead(msg: Message) {
    const requestOptions = { headers: new HttpHeaders({
      'Content-Type': 'application/json'
    })};
    const prms = new SelParams(this.selectedHost, msg.id.toString());
    return this.http.post(this.baseUrl + "markasread", prms, requestOptions).pipe(
      catchError(this.handleError),
      tap(errmsg => {
        if (this.checkErrMsg(errmsg)) {
          const markedMsg = this.messages.find(m => m.id === msg.id);
          if (markedMsg) {
            markedMsg.seen = true;
            this.messagesLoaded.emit(this.messages);
          }
        }
      })
    );
  }

  private checkErrMsg(errmsg: Object): boolean {
    if (errmsg !== null && errmsg.toString().length > 0) {
      alert(`An error occured: ${errmsg}`);
      return false;
    }
    return true;
  }

  private handleError(errmsg: string) {
    return throwError(() => errmsg);
  }
}