import { ChangeDetectionStrategy, Component, OnInit, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TranscriptStore } from '../store/transcript.store';

@Component({
  selector: 'app-transcript-list',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe, RouterLink],
  template: `
    <div class="p-6">
      <h1 class="text-2xl font-semibold mb-6">Sales Transcripts</h1>

      @if (store.loading()) {
        <p class="text-gray-500">Loading...</p>
      } @else if (store.error()) {
        <p class="text-red-600">{{ store.error() }}</p>
      } @else {
        <table class="w-full border-collapse text-sm">
          <thead>
            <tr class="bg-gray-100 text-left text-gray-600">
              <th class="p-3 border-b font-medium">Recording Name</th>
              <th class="p-3 border-b font-medium">Company</th>
              <th class="p-3 border-b font-medium">Rep</th>
              <th class="p-3 border-b font-medium">Deal Stage</th>
              <th class="p-3 border-b font-medium">Created At</th>
              <th class="p-3 border-b font-medium">Reviewed</th>
            </tr>
          </thead>
          <tbody>
            @for (item of store.items(); track item.id) {
              <tr
                class="hover:bg-gray-50 cursor-pointer"
                [routerLink]="['/transcripts', item.id]"
              >
                <td class="p-3 border-b">{{ item.name }}</td>
                <td class="p-3 border-b">{{ item.company }}</td>
                <td class="p-3 border-b">{{ item.repName }}</td>
                <td class="p-3 border-b">{{ item.dealStage }}</td>
                <td class="p-3 border-b">{{ item.createdAt | date: 'mediumDate' }}</td>
                <td class="p-3 border-b">{{ item.reviewed ? 'Yes' : 'No' }}</td>
              </tr>
            } @empty {
              <tr>
                <td colspan="6" class="p-4 text-gray-500 text-center">No transcripts found.</td>
              </tr>
            }
          </tbody>
        </table>
      }
    </div>
  `,
})
export class TranscriptListComponent implements OnInit {
  readonly store = inject(TranscriptStore);

  ngOnInit(): void {
    this.store.loadTranscripts();
  }
}
