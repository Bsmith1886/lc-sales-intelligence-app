import { ChangeDetectionStrategy, Component, OnInit, inject } from '@angular/core';
import { DatePipe, NgClass } from '@angular/common';
import { RouterLink } from '@angular/router';
import { DashboardStore } from './dashboard.store';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe, RouterLink, NgClass],
  providers: [DashboardStore],
  template: `
    <div class="min-h-screen bg-gray-50">

      <!-- Header -->
      <div class="bg-white border-b border-gray-200 px-6 py-4 flex items-center justify-between">
        <div>
          <h1 class="text-xl font-semibold text-gray-900">Sales Intelligence</h1>
          <p class="text-sm text-gray-500 mt-0.5">Manager Dashboard</p>
        </div>
        <a [routerLink]="['/transcripts']"
           class="text-sm text-blue-600 hover:text-blue-800 font-medium">
          View All Transcripts →
        </a>
      </div>

      @if (store.loading()) {
        <div class="flex items-center justify-center h-64">
          <p class="text-gray-400 text-sm">Loading dashboard...</p>
        </div>
      } @else if (store.error()) {
        <div class="p-6">
          <p class="text-red-600 text-sm">{{ store.error() }}</p>
        </div>
      } @else {

        <div class="p-6 space-y-6">

          <!-- KPI Row -->
          <div class="grid grid-cols-2 lg:grid-cols-4 gap-4">
            <div class="bg-white rounded-lg border border-gray-200 p-5">
              <p class="text-xs font-medium text-gray-500 uppercase tracking-wide">Total Calls</p>
              <p class="text-3xl font-bold text-gray-900 mt-1">{{ store.totalCalls() }}</p>
            </div>
            <div class="bg-white rounded-lg border border-gray-200 p-5">
              <p class="text-xs font-medium text-gray-500 uppercase tracking-wide">Total Duration</p>
              <p class="text-3xl font-bold text-gray-900 mt-1">{{ store.totalDurationMins() }}<span class="text-base font-normal text-gray-400 ml-1">min</span></p>
            </div>
            <div class="bg-white rounded-lg border border-gray-200 p-5">
              <p class="text-xs font-medium text-gray-500 uppercase tracking-wide">Needs Review</p>
              <p class="text-3xl font-bold mt-1"
                 [ngClass]="store.unreviewedCount() > 0 ? 'text-amber-600' : 'text-gray-900'">
                {{ store.unreviewedCount() }}
              </p>
            </div>
            <div class="bg-white rounded-lg border border-gray-200 p-5">
              <p class="text-xs font-medium text-gray-500 uppercase tracking-wide">Coachable Moments</p>
              <p class="text-3xl font-bold mt-1"
                 [ngClass]="store.coachableMomentsCount() > 0 ? 'text-blue-600' : 'text-gray-900'">
                {{ store.coachableMomentsCount() }}
              </p>
            </div>
          </div>

          <!-- Middle row: Audience + Call Type -->
          <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">

            <!-- Internal vs External -->
            <div class="bg-white rounded-lg border border-gray-200 p-5">
              <h2 class="text-sm font-semibold text-gray-700 mb-4">Internal vs External Calls</h2>
              @if (store.callsByAudience().length === 0) {
                <p class="text-sm text-gray-400">No audience data yet.</p>
              }
              <div class="space-y-3">
                @for (item of store.callsByAudience(); track item.label) {
                  <div>
                    <div class="flex justify-between text-sm mb-1">
                      <span class="text-gray-700 font-medium">{{ item.label }}</span>
                      <span class="text-gray-500">{{ item.count }} calls · {{ item.pct }}%</span>
                    </div>
                    <div class="w-full bg-gray-100 rounded-full h-2">
                      <div class="h-2 rounded-full"
                           [ngClass]="item.label === 'External' ? 'bg-blue-500' : item.label === 'Internal' ? 'bg-purple-400' : 'bg-gray-400'"
                           [style.width.%]="item.pct">
                      </div>
                    </div>
                  </div>
                }
              </div>
            </div>

            <!-- Call Type Breakdown -->
            <div class="bg-white rounded-lg border border-gray-200 p-5">
              <h2 class="text-sm font-semibold text-gray-700 mb-4">Call Type Breakdown</h2>
              @if (store.callsByType().length === 0) {
                <p class="text-sm text-gray-400">No call type data yet.</p>
              }
              <div class="space-y-3">
                @for (item of store.callsByType(); track item.label) {
                  <div>
                    <div class="flex justify-between text-sm mb-1">
                      <span class="text-gray-700 font-medium">{{ item.label }}</span>
                      <span class="text-gray-500">{{ item.count }} · {{ item.pct }}%</span>
                    </div>
                    <div class="w-full bg-gray-100 rounded-full h-2">
                      <div class="h-2 rounded-full bg-blue-500" [style.width.%]="item.pct"></div>
                    </div>
                  </div>
                }
              </div>
            </div>

          </div>

          <!-- Middle row: Quality + Deal Stage -->
          <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">

            <!-- Call Quality -->
            <div class="bg-white rounded-lg border border-gray-200 p-5">
              <h2 class="text-sm font-semibold text-gray-700 mb-4">Call Quality</h2>
              <div class="space-y-3">
                @for (item of store.callsByQuality(); track item.label) {
                  <div>
                    <div class="flex justify-between text-sm mb-1">
                      <span class="font-medium"
                            [ngClass]="{
                              'text-green-700': item.label === 'Excellent',
                              'text-blue-700': item.label === 'Good',
                              'text-red-600': item.label === 'Needs Work',
                              'text-gray-500': item.label === 'Not Reviewed'
                            }">
                        {{ item.label }}
                      </span>
                      <span class="text-gray-500">{{ item.count }} · {{ item.pct }}%</span>
                    </div>
                    <div class="w-full bg-gray-100 rounded-full h-2">
                      <div class="h-2 rounded-full"
                           [ngClass]="{
                             'bg-green-500': item.label === 'Excellent',
                             'bg-blue-400': item.label === 'Good',
                             'bg-red-400': item.label === 'Needs Work',
                             'bg-gray-300': item.label === 'Not Reviewed'
                           }"
                           [style.width.%]="item.pct">
                      </div>
                    </div>
                  </div>
                }
              </div>
            </div>

            <!-- Deal Stage Distribution -->
            <div class="bg-white rounded-lg border border-gray-200 p-5">
              <h2 class="text-sm font-semibold text-gray-700 mb-4">Deal Stage Distribution</h2>
              @if (store.callsByDealStage().length === 0) {
                <p class="text-sm text-gray-400">No deal stage data yet.</p>
              }
              <div class="space-y-3">
                @for (item of store.callsByDealStage(); track item.label) {
                  <div>
                    <div class="flex justify-between text-sm mb-1">
                      <span class="text-gray-700 font-medium">{{ item.label }}</span>
                      <span class="text-gray-500">{{ item.count }} · {{ item.pct }}%</span>
                    </div>
                    <div class="w-full bg-gray-100 rounded-full h-2">
                      <div class="h-2 rounded-full bg-indigo-400" [style.width.%]="item.pct"></div>
                    </div>
                  </div>
                }
              </div>
            </div>

          </div>

          <!-- Rep Performance Table -->
          <div class="bg-white rounded-lg border border-gray-200 p-5">
            <h2 class="text-sm font-semibold text-gray-700 mb-4">Rep Performance</h2>
            <table class="w-full text-sm">
              <thead>
                <tr class="text-left text-xs font-medium text-gray-500 uppercase tracking-wide border-b border-gray-100">
                  <th class="pb-2 pr-4">Rep</th>
                  <th class="pb-2 pr-4 text-right">Calls</th>
                  <th class="pb-2 pr-4 text-right">Reviewed</th>
                  <th class="pb-2 pr-4 text-right">Coachable</th>
                  <th class="pb-2 text-right">Avg Duration</th>
                </tr>
              </thead>
              <tbody>
                @for (r of store.repStats(); track r.rep) {
                  <tr class="border-b border-gray-50 last:border-0">
                    <td class="py-2.5 pr-4 font-medium text-gray-800">{{ r.rep }}</td>
                    <td class="py-2.5 pr-4 text-right text-gray-600">{{ r.total }}</td>
                    <td class="py-2.5 pr-4 text-right">
                      <span class="text-gray-600">{{ r.reviewed }}</span>
                      <span class="text-gray-400 text-xs ml-1">({{ r.reviewedPct }}%)</span>
                    </td>
                    <td class="py-2.5 pr-4 text-right">
                      @if (r.coachable > 0) {
                        <span class="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-blue-50 text-blue-700">
                          {{ r.coachable }}
                        </span>
                      } @else {
                        <span class="text-gray-400">—</span>
                      }
                    </td>
                    <td class="py-2.5 text-right text-gray-600">{{ r.avgDurationMins }}m</td>
                  </tr>
                }
              </tbody>
            </table>
          </div>

          <!-- Recent Calls -->
          <div class="bg-white rounded-lg border border-gray-200 p-5">
            <div class="flex items-center justify-between mb-4">
              <h2 class="text-sm font-semibold text-gray-700">Recent Calls</h2>
              <a [routerLink]="['/transcripts']" class="text-xs text-blue-600 hover:text-blue-800">View all →</a>
            </div>
            <div class="divide-y divide-gray-50">
              @for (t of store.recentCalls(); track t.id) {
                <a [routerLink]="['/transcripts', t.id]"
                   class="flex items-center gap-4 py-3 hover:bg-gray-50 -mx-2 px-2 rounded cursor-pointer">
                  <div class="flex-1 min-w-0">
                    <p class="text-sm font-medium text-gray-800 truncate">{{ t.name }}</p>
                    <p class="text-xs text-gray-500 mt-0.5">
                      {{ t.repName ?? '—' }}
                      @if (t.company) { · {{ t.company }} }
                    </p>
                  </div>
                  <div class="flex items-center gap-3 shrink-0 text-xs">
                    @if (t.callType) {
                      <span class="text-gray-500">{{ t.callType }}</span>
                    }
                    @if (t.callQuality) {
                      <span class="px-2 py-0.5 rounded font-medium"
                            [ngClass]="{
                              'bg-green-50 text-green-700': t.callQuality === 'Excellent',
                              'bg-blue-50 text-blue-700': t.callQuality === 'Good',
                              'bg-red-50 text-red-700': t.callQuality === 'Needs Work'
                            }">
                        {{ t.callQuality }}
                      </span>
                    }
                    <span class="text-gray-400">{{ t.createdAt | date: 'MMM d' }}</span>
                    @if (!t.reviewed) {
                      <span class="w-2 h-2 rounded-full bg-amber-400" title="Not reviewed"></span>
                    }
                  </div>
                </a>
              }
            </div>
          </div>

        </div>
      }
    </div>
  `,
})
export class DashboardComponent implements OnInit {
  readonly store = inject(DashboardStore);

  ngOnInit(): void {
    this.store.load();
  }
}
