import { Routes } from '@angular/router';
import { MsalGuard } from '@azure/msal-angular';

export const routes: Routes = [
  {
    path: '',
    canActivate: [MsalGuard],
    loadComponent: () =>
      import('./dashboard/dashboard.component').then((m) => m.DashboardComponent),
  },
  {
    path: 'transcripts',
    canActivate: [MsalGuard],
    loadChildren: () =>
      import('./transcripts/transcripts.routes').then((m) => m.transcriptRoutes),
  },
];
