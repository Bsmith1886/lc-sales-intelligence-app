import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'transcripts',
    loadChildren: () =>
      import('./transcripts/transcripts.routes').then((m) => m.transcriptRoutes),
  },
  { path: '', redirectTo: 'transcripts', pathMatch: 'full' },
];
