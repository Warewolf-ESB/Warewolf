import { NgModule, CUSTOM_ELEMENTS_SCHEMA} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FlexLayoutModule } from '@angular/flex-layout';
import { DashboardComponent } from './dashboard/dashboard.component';
import { DashboardRoutingModule } from './routing/dashboard-routing.module';
import { DashboardService } from './services//dashboard.service';
import { DashboardUsersComponent } from './cards/dashboard-users/dashboard-users.component';
import { DashboardCardsCreatorComponent } from './cards/dashboard-cards-creator/dashboard-cards-creator.component';
import { CustomMaterialModule } from '../core/material.module';
@NgModule({
  imports: [
    CommonModule,
    FlexLayoutModule,
    CustomMaterialModule,
    DashboardRoutingModule
  ],
  declarations: [
    DashboardComponent,
    DashboardUsersComponent,
    DashboardCardsCreatorComponent],
  providers: [DashboardService]
})
export class DashboardModule {
}
