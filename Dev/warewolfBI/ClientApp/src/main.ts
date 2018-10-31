//import './testing/global-jasmine';
//import 'jasmine-core/lib/jasmine-core/jasmine-html.js';
//import 'jasmine-core/lib/jasmine-core/boot.js';

//declare var jasmine;

//import 'zone.js/dist/zone-testing';


//import { getTestBed } from '@angular/core/testing';
//import {
//  BrowserDynamicTestingModule,
//  platformBrowserDynamicTesting
//} from '@angular/platform-browser-dynamic/testing';
//// Spec files to include in the Stackblitz tests
//import './tests.sb.ts';

//bootstrap();

import { enableProdMode } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import 'hammerjs';
import { AppModule } from './app/app.module';
import { environment } from './environments/environment';
import './polyfills';

export function getBaseUrl() {
  return document.getElementsByTagName('base')[0].href;
}

const providers = [
  { provide: 'BASE_URL', useFactory: getBaseUrl, deps: [] }
];

if (environment.production) {
  enableProdMode();
}

platformBrowserDynamic(providers).bootstrapModule(AppModule)
  .catch(err => console.log(err));

//function bootstrap() {
//  if (window['jasmineRef']) {
//    location.reload();
//    return;
//  } else {
//    window.onload(undefined);
//    window['jasmineRef'] = jasmine.getEnv();
//  }

//  // First, initialize the Angular testing environment.
//  getTestBed().initTestEnvironment(
//    BrowserDynamicTestingModule,
//    platformBrowserDynamicTesting()
//  );
//}
