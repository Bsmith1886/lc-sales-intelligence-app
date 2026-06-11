import { Routes } from '@angular/router';
import { TranscriptStore } from './store/transcript.store';
import { TranscriptListComponent } from './transcript-list/transcript-list';
import { TranscriptDetailComponent } from './transcript-detail/transcript-detail';

export const transcriptRoutes: Routes = [
  {
    path: '',
    component: TranscriptListComponent,
    providers: [TranscriptStore],
  },
  {
    path: ':id',
    component: TranscriptDetailComponent,
    providers: [TranscriptStore],
  },
];
