import { APP_INITIALIZER, ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import {
  MSAL_GUARD_CONFIG,
  MSAL_INSTANCE,
  MSAL_INTERCEPTOR_CONFIG,
  MsalBroadcastService,
  MsalGuard,
  MsalInterceptor,
  MsalService,
} from '@azure/msal-angular';
import {
  BrowserCacheLocation,
  InteractionType,
  PublicClientApplication,
} from '@azure/msal-browser';
import { firstValueFrom } from 'rxjs';
import { routes } from './app.routes';
import { environment } from '../environments/environment';

function msalInstanceFactory() {
  return new PublicClientApplication({
    auth: {
      clientId: environment.clientId,
      authority: `https://login.microsoftonline.com/${environment.tenantId}`,
      redirectUri: window.location.origin,
    },
    cache: { cacheLocation: BrowserCacheLocation.LocalStorage },
  });
}

function msalGuardConfigFactory() {
  return {
    interactionType: InteractionType.Redirect,
    authRequest: { scopes: ['user.read'] },
  };
}

function msalInterceptorConfigFactory() {
  return {
    interactionType: InteractionType.Redirect,
    protectedResourceMap: new Map([[`${window.location.origin}/api/*`, [environment.apiScope]]]),
  };
}

function msalInitializerFactory(msalService: MsalService) {
  return () =>
    firstValueFrom(msalService.handleRedirectObservable()).then((result) => {
      if (result?.account) {
        msalService.instance.setActiveAccount(result.account);
      } else {
        const accounts = msalService.instance.getAllAccounts();
        if (accounts.length > 0) {
          msalService.instance.setActiveAccount(accounts[0]);
        }
      }
    });
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptorsFromDi()),
    { provide: HTTP_INTERCEPTORS, useClass: MsalInterceptor, multi: true },
    { provide: MSAL_INSTANCE, useFactory: msalInstanceFactory },
    { provide: MSAL_GUARD_CONFIG, useFactory: msalGuardConfigFactory },
    { provide: MSAL_INTERCEPTOR_CONFIG, useFactory: msalInterceptorConfigFactory },
    MsalService,
    MsalGuard,
    MsalBroadcastService,
    {
      provide: APP_INITIALIZER,
      useFactory: msalInitializerFactory,
      deps: [MsalService],
      multi: true,
    },
  ],
};
