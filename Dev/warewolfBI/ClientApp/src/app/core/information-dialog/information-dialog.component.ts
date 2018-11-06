import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';

@Component({
  templateUrl: './information-dialog.component.html'
})
export class InformationDialogComponent {
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    public dialogRef: MatDialogRef<InformationDialogComponent>
  ) { }

  close() {
    this.dialogRef.close();
  }
}
