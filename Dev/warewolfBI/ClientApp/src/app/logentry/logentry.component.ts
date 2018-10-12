import { Component, Inject, OnInit, ViewEncapsulation } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from "@angular/material";
import { LogEntry } from '../models/logentry.model';
import { FormBuilder, Validators, FormGroup } from "@angular/forms";
import * as moment from 'moment';


@Component({
  selector: 'app-logentry',
  templateUrl: './logentry.component.html',
  styleUrls: ['./logentry.component.scss']
})

export class LogEntryComponent implements OnInit {
  
  ExecutionId: string;
  Url: string;
  Status: string;
  StartDateTime: string;
  ExecutionTime: string;
  CompletedDateTime: string;
  User: string;
  displayedColumns = ["ExecutionId", "Url", "ExecutionTime", "Status", "StartDateTime", "CompletedDateTime", "User"];

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<LogEntryComponent>,
    @Inject(MAT_DIALOG_DATA) public data: LogEntry) {
    this.ExecutionId = data.LogEntry.ExecutionId;
    this.Url = data.LogEntry.Url;
    this.Status = data.LogEntry.Status;
    this.ExecutionTime = data.LogEntry.ExecutionTime;
    this.StartDateTime = data.LogEntry.StartDateTime;
    this.CompletedDateTime = data.LogEntry.CompletedDateTime;
    this.User = data.LogEntry.User;
  }

  ngOnInit() { }

  resume() {
    this.dialogRef.close();
  }

  close() {
    this.dialogRef.close();
  }

}
