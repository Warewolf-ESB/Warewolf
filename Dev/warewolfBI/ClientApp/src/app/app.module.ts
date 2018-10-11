import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { CustomMaterialModule } from "./core/material.module";
import { FormsModule } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ModalModule } from 'angular-custom-modal';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { AngMaterialModule } from './angmaterial';
import { LayoutModule } from '@angular/cdk/layout';

import { ErrorInterceptor } from './helpers/error.interceptor';
import { AuthInterceptor } from './helpers/auth.interceptor';
import { GoogleChartsModule } from 'angular-google-charts';
import { sanitizeHtmlPipe } from './sanitize-html.pipe';
import { FlexLayoutModule } from '@angular/flex-layout';
import { AppComponent } from './app.component';
import { AppRoutingModule } from './core/app-routing.module';
import { DashboardComponent } from './dashboard/dashboard/dashboard.component';
import { ReportBuilderComponent } from './report-builder/report-builder.component';
import { DashboardUsersComponent } from './dashboard/cards/dashboard-users/dashboard-users.component';
import { DashboardService } from './dashboard/services/dashboard.service';
import { ServerExplorerComponent } from './server-explorer/server-explorer.component';
import { OutputsExplorerComponent } from './outputs-explorer/outputs-explorer.component';
import { ExecutionloggingComponent } from './executionlogging/executionlogging.component';
import { SettingsComponent } from './settings/settings.component';
import { MaindashboardComponent } from './maindashboard/maindashboard.component';

import { MediatorService } from './services/mediator.service';
import { UserService } from './services/user.service';
import { APIService } from './services/api.service';
import { ExecutionLoggingService } from './services/executionlogging.service';
import { ErrorDialogComponent} from './core/error-dialog/error-dialog.component'

@NgModule({
  entryComponents: [ErrorDialogComponent],
  declarations: [
    AppComponent,
    sanitizeHtmlPipe,
    DashboardComponent,
    DashboardUsersComponent,
    ReportBuilderComponent,
    ServerExplorerComponent,
    OutputsExplorerComponent,
    ExecutionloggingComponent,
    SettingsComponent,
    MaindashboardComponent
  ],
  imports: [
    NgbModule.forRoot(),
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    BrowserAnimationsModule,
    CustomMaterialModule,
    AppRoutingModule,
    CommonModule,
    FlexLayoutModule,
    ModalModule,
    HttpClientModule,
    ReactiveFormsModule,
    FormsModule,
    LayoutModule,
    GoogleChartsModule,
  ],
  exports: [sanitizeHtmlPipe],
  providers: [DashboardService,
    MediatorService,
    UserService,
    APIService,
    ExecutionLoggingService],
  schemas: [
    CUSTOM_ELEMENTS_SCHEMA
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
