import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { MediatorService } from './services/mediator.service';

import { ExecutionLoggingComponent } from './components/executionlogging/executionlogging.component';
import { CustomMaterialModule } from "./core/custommaterial.module";

const appRoutes: Routes = [
  { path: '', redirectTo: '/executionlogging', pathMatch: 'full' },
  { path: 'executionlogging', component: ExecutionLoggingComponent }
];

@NgModule({
  imports: [
    CommonModule,
    CustomMaterialModule,
    RouterModule.forRoot(appRoutes)
  ],
  exports: [RouterModule],
  providers: [MediatorService]
})
export class AppRoutingModule { }
