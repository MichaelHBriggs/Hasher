import { NgModule, provideBrowserGlobalErrorListeners } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { HttpErrorInterceptor } from './interceptors/HttpErrorInterceptor';

import { AppRoutingModule } from './app-routing-module';
import { App } from './app';

import { RunInfo } from '../app/run-info/run-info'
import { RunRestService } from './RestServices/runs.rest.service';

@NgModule({
  declarations: [
    App
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    RunInfo
  ],
  providers: [
    provideBrowserGlobalErrorListeners(),

    RunRestService
  ],
  bootstrap: [App]
})
export class AppModule { }
