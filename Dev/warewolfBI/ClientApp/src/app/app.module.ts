import { NgModule, CUSTOM_ELEMENTS_SCHEMA, NO_ERRORS_SCHEMA } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { CustomMaterialModule } from './core/custommaterial.module';
import { HttpClientModule, HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';
import { CommonModule,DatePipe } from '@angular/common';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ModalModule } from 'angular-custom-modal';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { LayoutModule } from '@angular/cdk/layout';

import {
  MatDialogModule,
  MatDatepickerModule,
  MatNativeDateModule,
  MatGridListModule
} from '@angular/material';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { sanitizeHtmlPipe } from './sanitize-html.pipe';
import { FlexLayoutModule } from '@angular/flex-layout';

import { AppRoutingModule } from './app-routing.module';

import { AppComponent } from './app.component';
import { ServerExplorerComponent } from './components/server-explorer/server-explorer.component';
import { OutputsExplorerComponent } from './components/outputs-explorer/outputs-explorer.component';
import { ExecutionLoggingComponent } from './components/executionlogging/executionlogging.component';
import { SettingsComponent } from './components/settings/settings.component';

import { ExecutionLoggingService } from './services/executionlogging.service';
import { LogEntryComponent } from './components/logentry/logentry.component';

import { MediatorService } from './services/mediator.service';
import { APIService } from './services/api.service';

@NgModule({
  entryComponents: [
     LogEntryComponent
  ],
  declarations: [
    AppComponent,
    sanitizeHtmlPipe,
    ServerExplorerComponent,
    OutputsExplorerComponent,
    ExecutionLoggingComponent,
    SettingsComponent,
    LogEntryComponent
  ],
  imports: [
    NgbModule,
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    BrowserAnimationsModule,
    HttpClientModule,
    MatButtonModule,
    CustomMaterialModule,
    AppRoutingModule,
    CommonModule,
    FlexLayoutModule,
    ModalModule,
    ReactiveFormsModule,
    FormsModule,
    LayoutModule,
    MatDialogModule,
    MatInputModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatGridListModule
  ],
  exports: [
    sanitizeHtmlPipe
  ],
  providers: [
    MediatorService,
    APIService,
    ExecutionLoggingService],
  schemas: [
    CUSTOM_ELEMENTS_SCHEMA,
    NO_ERRORS_SCHEMA
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
