import { MatButtonModule, MatSliderModule } from '@angular/material';
import { NgModule } from '@angular/core';
 
@NgModule({
  imports: [MatButtonModule, MatSliderModule],
  exports: [MatButtonModule, MatSliderModule]
})
 
export class AngMaterialModule { }
