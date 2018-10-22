import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ReportBuilderComponent } from '../report-builder/report-builder.component';
import { RoleGuardService as RoleGuard } from '../guards/role-guard.service';
import { ExecutionloggingComponent } from '../executionlogging/executionlogging.component';
import { SettingsComponent } from '../settings/settings.component';
import { CustomMaterialModule } from "./material.module";
const appRoutes: Routes = [
  { path: '', component: ExecutionloggingComponent },
  { path: 'executionlogging', component: ExecutionloggingComponent },
  { path: 'settings', component: SettingsComponent },
  { path: 'report-builder', component: ReportBuilderComponent }
];

@NgModule({
  imports: [
    RouterModule.forRoot(
      appRoutes, { useHash: true }
    ),
    CustomMaterialModule
  ],
  exports: [RouterModule],
  providers: []
})
export class AppRoutingModule { }
