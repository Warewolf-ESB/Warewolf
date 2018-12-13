import { Component, Inject, OnInit, ViewEncapsulation } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from "@angular/material";
import { LogEntry } from '../../models/logentry.model';
import { FormBuilder, Validators, FormGroup } from "@angular/forms";

@Component({
  selector: 'app-logentry',
  templateUrl: './logentry.component.html',
  styleUrls: ['./logentry.component.scss']
})

export class LogEntryComponent implements OnInit {

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<LogEntryComponent>,
    @Inject(MAT_DIALOG_DATA) public logEntry: LogEntry) {
  }

  ngOnInit() { }

  resume() {
    this.dialogRef.componentInstance.resume();
  }

  close() {
    this.dialogRef.close();
  }
}
