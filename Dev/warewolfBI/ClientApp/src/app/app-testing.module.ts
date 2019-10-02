import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule, CUSTOM_ELEMENTS_SCHEMA, NO_ERRORS_SCHEMA } from '@angular/core';
import { RouterTestingModule } from '@angular/router/testing';

import { AppComponent } from './app.component';
import { ExecutionLoggingComponent } from './components/executionlogging/executionlogging.component';
import { SettingsComponent } from './components/settings/settings.component';
import { ServerExplorerComponent } from './components/server-explorer/server-explorer.component';
import { OutputsExplorerComponent } from './components/outputs-explorer/outputs-explorer.component';
import { CustomMaterialModule } from './core/custommaterial.module';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';

import { MediatorService } from './services/mediator.service';

import { APIService } from './services/api.service';
import { ExecutionLoggingService } from './services/executionlogging.service';

import { HttpClientModule } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { sanitizeHtmlPipe } from './sanitize-html.pipe';

import { Routes } from '@angular/router';

export const testRoutes: Routes = [
  { path: '', redirectTo: '/executionlogging', pathMatch: 'full' },
  { path: 'executionlogging', component: ExecutionLoggingComponent }
];

@NgModule({
  declarations: [
    AppComponent,
    sanitizeHtmlPipe,
    ServerExplorerComponent,
    OutputsExplorerComponent,
    ExecutionLoggingComponent,
    SettingsComponent
  ],
  imports: [
    CustomMaterialModule, MatInputModule, MatButtonModule, MatDialogModule,
    BrowserModule,
    BrowserAnimationsModule,
    CustomMaterialModule,
    HttpClientModule,
    ReactiveFormsModule,
    FormsModule,
    RouterTestingModule.withRoutes(testRoutes)
  ],
  exports: [
    sanitizeHtmlPipe
  ],
  providers: [
    MediatorService,
    APIService,
    ExecutionLoggingService
  ],
  schemas: [
    CUSTOM_ELEMENTS_SCHEMA,
    NO_ERRORS_SCHEMA
  ]
})
export class AppTestingModule { }
