import { inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { TranscriptApiService } from '../data-access/transcript-api.service';
import { GetTranscriptsApiRequest, TranscriptApiResponse, TranscriptListItemApiResponse } from '../data-access/transcript.model';

interface TranscriptState {
  items: TranscriptListItemApiResponse[];
  selected: TranscriptApiResponse | null;
  loading: boolean;
  error: string | null;
}

const initialState: TranscriptState = {
  items: [],
  selected: null,
  loading: false,
  error: null,
};

export const TranscriptStore = signalStore(
  withState(initialState),
  withMethods((store, api = inject(TranscriptApiService)) => ({
    async loadTranscripts(request?: GetTranscriptsApiRequest) {
      patchState(store, { loading: true, error: null });
      const result = await firstValueFrom(api.getAll(request));
      patchState(store, {
        items: result.success ? (result.data ?? []) : [],
        error: result.success ? null : result.error,
        loading: false,
      });
    },
    async loadTranscript(id: string) {
      patchState(store, { loading: true, error: null, selected: null });
      const result = await firstValueFrom(api.getById(id));
      patchState(store, {
        selected: result.success ? result.data : null,
        error: result.success ? null : result.error,
        loading: false,
      });
    },
  }))
);
