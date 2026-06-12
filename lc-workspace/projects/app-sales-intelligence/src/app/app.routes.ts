import { Routes } from '@angular/router';
import { MsalGuard } from '@azure/msal-angular';

export const routes: Routes = [
  {
    path: 'transcripts',
    canActivate: [MsalGuard],
    loadChildren: () =>
      import('./transcripts/transcripts.routes').then((m) => m.transcriptRoutes),
  },
  { path: '', redirectTo: 'transcripts', pathMatch: 'full' },
];
