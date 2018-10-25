import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule, CUSTOM_ELEMENTS_SCHEMA, NO_ERRORS_SCHEMA } from '@angular/core';

import { AppComponent } from './app.component';
import { ExecutionLoggingComponent } from './components/executionlogging/executionlogging.component';
import { SettingsComponent } from './components/settings/settings.component';
import { ServerExplorerComponent } from './components/server-explorer/server-explorer.component';
import { OutputsExplorerComponent } from './components/outputs-explorer/outputs-explorer.component';

import { MediatorService } from './services/mediator.service';
import { AuthGuardService } from './guards/auth.guard.service';
import { AuthenticationService } from './services/authentication.service';
import { UserService } from './services/user.service';
import { APIService } from './services/api.service';
import { ExecutionLoggingService } from './services/executionlogging.service';

import { RouterModule, Routes } from '@angular/router';
import { HttpModule } from '@angular/http';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CustomMaterialModule } from "./core/custommaterial.module";
import { APP_BASE_HREF } from '@angular/common';

import { RouterLinkDirective } from '../testing/router-link.directive';
import { sanitizeHtmlPipe } from './sanitize-html.pipe';

import { InMemoryWebApiModule } from 'angular-in-memory-web-api';
import { InMemoryDataService } from './in-memory-data.service';

const routes: Routes = [
  { path: '', component: ExecutionLoggingComponent },
  { path: 'executionlogging', component: ExecutionLoggingComponent },
  { path: 'settings', component: SettingsComponent }
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
    BrowserModule,
    RouterModule.forRoot(routes),
    BrowserAnimationsModule,
    CustomMaterialModule,
    HttpModule,
    HttpClientModule,
    ReactiveFormsModule,
    FormsModule,
    InMemoryWebApiModule.forRoot(InMemoryDataService)
  ],
  exports: [
    sanitizeHtmlPipe
  ],
  providers: [
    MediatorService,
    UserService,
    APIService,
    ExecutionLoggingService,
    { provide: APP_BASE_HREF, useValue: '/' }
  ],
  schemas: [
    CUSTOM_ELEMENTS_SCHEMA,
    NO_ERRORS_SCHEMA
  ]
})
export class AppTestingModule { }
