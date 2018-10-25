import { Component, Inject, OnInit, ViewEncapsulation } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from "@angular/material";
import { LogEntry } from '../../models/logentry.model';
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
    public dialogRef: MatDialogRef<LogEntryComponent>,
    @Inject(MAT_DIALOG_DATA) public data: LogEntry) {
    this.ExecutionId = data.ExecutionId;
    this.Url = data.Url;
    this.Status = data.Status;
    this.ExecutionTime = data.ExecutionTime;
    this.StartDateTime = data.StartDateTime;
    this.CompletedDateTime = data.CompletedDateTime;
    this.User = data.User;
  }

  ngOnInit() { }

  resume() {
    this.dialogRef.close();
  }

  close() {
    this.dialogRef.close();
  }
}
