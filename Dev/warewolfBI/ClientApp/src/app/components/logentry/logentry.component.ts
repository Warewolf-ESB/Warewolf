import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';
import { LogEntry } from '../../models/logentry.model';
import { FormBuilder} from '@angular/forms';

@Component({
  selector: 'app-logentry',
  templateUrl: './logentry.component.html',
  styleUrls: ['./logentry.component.scss']
})

export class LogEntryComponent implements OnInit {
   constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<LogEntryComponent>,
    @Inject(MAT_DIALOG_DATA) public data: LogEntry) {
    }

  ngOnInit() { }

  close() {
    this.dialogRef.close();
  }
}
