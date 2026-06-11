import { ChangeDetectionStrategy, Component, OnInit, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranscriptStore } from '../store/transcript.store';

@Component({
  selector: 'app-transcript-detail',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe, RouterLink],
  template: `
    <div class="p-6 max-w-4xl mx-auto">
      <a [routerLink]="['/transcripts']" class="text-blue-600 hover:underline text-sm mb-4 block">
        ← Back to list
      </a>

      @if (store.loading()) {
        <p class="text-gray-500">Loading...</p>
      } @else if (store.error()) {
        <p class="text-red-600">{{ store.error() }}</p>
      } @else if (store.selected()) {
        <div class="bg-white rounded-lg shadow p-6">
          <h1 class="text-xl font-semibold mb-4">{{ store.selected()!.name }}</h1>

          <div class="grid grid-cols-2 gap-4 mb-6 text-sm">
            <div><span class="font-medium text-gray-600">Company:</span> {{ store.selected()!.company }}</div>
            <div><span class="font-medium text-gray-600">Rep:</span> {{ store.selected()!.repName }}</div>
            <div><span class="font-medium text-gray-600">Deal Stage:</span> {{ store.selected()!.dealStage }}</div>
            <div><span class="font-medium text-gray-600">Deal Type:</span> {{ store.selected()!.dealType }}</div>
            <div><span class="font-medium text-gray-600">Opportunity ID:</span> {{ store.selected()!.opportunityId }}</div>
            <div><span class="font-medium text-gray-600">Duration:</span> {{ store.selected()!.duration ?? 'N/A' }} mins</div>
            <div><span class="font-medium text-gray-600">Created At:</span> {{ store.selected()!.createdAt | date: 'medium' }}</div>
            <div><span class="font-medium text-gray-600">Reviewed:</span> {{ store.selected()!.reviewed ? 'Yes' : 'No' }}</div>
            <div><span class="font-medium text-gray-600">Coachable Moments:</span> {{ store.selected()!.coachableMoments ? 'Yes' : 'No' }}</div>
          </div>

          @if (store.selected()!.keyTopics.length) {
            <div class="mb-6">
              <p class="font-medium text-gray-600 text-sm mb-2">Key Topics</p>
              <div class="flex flex-wrap gap-2">
                @for (topic of store.selected()!.keyTopics; track topic) {
                  <span class="bg-blue-100 text-blue-800 text-xs px-2 py-1 rounded">{{ topic }}</span>
                }
              </div>
            </div>
          }

          <div>
            <p class="font-medium text-gray-600 text-sm mb-2">Transcript</p>
            <pre class="bg-gray-50 p-4 rounded text-sm whitespace-pre-wrap font-mono leading-relaxed">{{ store.selected()!.transcriptText }}</pre>
          </div>
        </div>
      }
    </div>
  `,
})
export class TranscriptDetailComponent implements OnInit {
  readonly store = inject(TranscriptStore);
  private route = inject(ActivatedRoute);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.store.loadTranscript(id);
  }
}
