import { NgModule, CUSTOM_ELEMENTS_SCHEMA} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FlexLayoutModule } from '@angular/flex-layout';
import { DashboardRoutingModule } from './routing/dashboard-routing.module';
import { DashboardService } from './services//dashboard.service';
import { CustomMaterialModule } from '../../core/custommaterial.module';
@NgModule({
  imports: [
    CommonModule,
    FlexLayoutModule,
    CustomMaterialModule,
    DashboardRoutingModule
  ],
  declarations: [],
  providers: [DashboardService]
})
export class DashboardModule {
}
