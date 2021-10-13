import { ConfirmService } from './../_services/confirm.service';
import { MessageService } from './../_services/message.service';
import { Pagination } from './../_models/pagination';
import { Component, OnInit } from '@angular/core';
import { Message } from '../_models/message';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css'],
})
export class MessagesComponent implements OnInit {
  messages: Message[];
  pagination: Pagination;
  container = 'Unread';
  pageNumber = 1;
  pageSize = 5;
  loading = false;

  constructor(
    private messageService: MessageService,
    private confirmServie: ConfirmService
  ) {}

  ngOnInit(): void {
    this.loadMessages();
  }

  loadMessages() {
    this.loading = true;
    this.messageService
      .getMessages(this.pageNumber, this.pageSize, this.container)
      .subscribe((response) => {
        this.messages = response.result;
        this.pagination = response.pagination;
        this.loading = false;
      });
  }

  pageChanged(event: any) {
    this.pageNumber = event.page;
    this.loadMessages();
  }

  deleteMessage(id: number) {
    this.confirmServie
      .confirm('Confirm delete message', 'This cannot be undone.')
      .subscribe((result) => {
        if (result) {
          this.messageService.deleteMessage(id).subscribe(() => {
            this.messages.splice(
              this.messages.findIndex((m) => m.id === id),
              1
            );
          });
        }
      });
  }
}
