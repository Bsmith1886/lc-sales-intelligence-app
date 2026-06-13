import { inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { computed } from '@angular/core';
import { TranscriptApiService } from '../transcripts/data-access/transcript-api.service';
import { TranscriptListItemApiResponse } from '../transcripts/data-access/transcript.model';

interface DashboardState {
  transcripts: TranscriptListItemApiResponse[];
  loading: boolean;
  error: string | null;
}

const initialState: DashboardState = {
  transcripts: [],
  loading: false,
  error: null,
};

export const DashboardStore = signalStore(
  withState(initialState),
  withComputed((store) => ({
    totalCalls: computed(() => store.transcripts().length),

    totalDurationMins: computed(() =>
      Math.round(store.transcripts().reduce((sum, t) => sum + (t.durationMins ?? 0), 0))
    ),

    unreviewedCount: computed(() =>
      store.transcripts().filter(t => !t.reviewed).length
    ),

    coachableMomentsCount: computed(() =>
      store.transcripts().filter(t => t.coachableMoments).length
    ),

    callsByAudience: computed(() => {
      const counts: Record<string, number> = { External: 0, Internal: 0, Other: 0 };
      for (const t of store.transcripts()) {
        const key = t.audience === 'Internal' ? 'Internal'
          : t.audience === 'External' ? 'External'
          : 'Other';
        counts[key]++;
      }
      const total = store.transcripts().length || 1;
      return Object.entries(counts)
        .filter(([, count]) => count > 0)
        .map(([label, count]) => ({ label, count, pct: Math.round((count / total) * 100) }));
    }),

    callsByType: computed(() => {
      const order = ['Cold Call', 'Discovery', 'Demo', 'Follow-up', 'QBR'];
      const counts: Record<string, number> = {};
      for (const t of store.transcripts()) {
        const key = t.callType ?? 'Unknown';
        counts[key] = (counts[key] ?? 0) + 1;
      }
      const total = store.transcripts().length || 1;
      return Object.entries(counts)
        .sort((a, b) => {
          const ai = order.indexOf(a[0]);
          const bi = order.indexOf(b[0]);
          return (ai === -1 ? 99 : ai) - (bi === -1 ? 99 : bi);
        })
        .map(([label, count]) => ({ label, count, pct: Math.round((count / total) * 100) }));
    }),

    callsByQuality: computed(() => {
      const order = ['Excellent', 'Good', 'Needs Work'];
      const counts: Record<string, number> = { Excellent: 0, Good: 0, 'Needs Work': 0, 'Not Reviewed': 0 };
      for (const t of store.transcripts()) {
        if (t.callQuality && order.includes(t.callQuality)) {
          counts[t.callQuality]++;
        } else {
          counts['Not Reviewed']++;
        }
      }
      const total = store.transcripts().length || 1;
      return Object.entries(counts)
        .filter(([, count]) => count > 0)
        .map(([label, count]) => ({ label, count, pct: Math.round((count / total) * 100) }));
    }),

    callsByDealStage: computed(() => {
      const counts: Record<string, number> = {};
      for (const t of store.transcripts()) {
        const key = t.dealStage ?? 'Unknown';
        counts[key] = (counts[key] ?? 0) + 1;
      }
      const total = store.transcripts().length || 1;
      return Object.entries(counts)
        .sort((a, b) => b[1] - a[1])
        .map(([label, count]) => ({ label, count, pct: Math.round((count / total) * 100) }));
    }),

    repStats: computed(() => {
      const map: Record<string, { total: number; reviewed: number; coachable: number; durationMins: number }> = {};
      for (const t of store.transcripts()) {
        const rep = t.repName ?? 'Unknown';
        if (!map[rep]) map[rep] = { total: 0, reviewed: 0, coachable: 0, durationMins: 0 };
        map[rep].total++;
        if (t.reviewed) map[rep].reviewed++;
        if (t.coachableMoments) map[rep].coachable++;
        map[rep].durationMins += t.durationMins ?? 0;
      }
      return Object.entries(map)
        .sort((a, b) => b[1].total - a[1].total)
        .map(([rep, s]) => ({
          rep,
          total: s.total,
          reviewed: s.reviewed,
          reviewedPct: Math.round((s.reviewed / s.total) * 100),
          coachable: s.coachable,
          avgDurationMins: Math.round(s.durationMins / s.total),
        }));
    }),

    recentCalls: computed(() =>
      [...store.transcripts()]
        .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
        .slice(0, 8)
    ),
  })),
  withMethods((store, api = inject(TranscriptApiService)) => ({
    async load() {
      patchState(store, { loading: true, error: null });
      const result = await firstValueFrom(api.getAll());
      patchState(store, {
        transcripts: result.success ? (result.data ?? []) : [],
        error: result.success ? null : result.error,
        loading: false,
      });
    },
  }))
);
