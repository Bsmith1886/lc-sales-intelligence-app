import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiHelperService, ApiResult } from 'lib-utils';
import {
  GetTranscriptsApiRequest,
  TranscriptApiResponse,
  TranscriptListItemApiResponse,
} from './transcript.model';

@Injectable({ providedIn: 'root' })
export class TranscriptApiService {
  private httpClient = inject(HttpClient);
  private apiHelper = inject(ApiHelperService);
  private readonly path = 'api/transcripts';

  getAll = (request?: GetTranscriptsApiRequest): Observable<ApiResult<TranscriptListItemApiResponse[]>> => {
    const params: Record<string, string> = {};
    if (request?.repName) params['repName'] = request.repName;
    if (request?.dealStage) params['dealStage'] = request.dealStage;
    if (request?.reviewed !== undefined) params['reviewed'] = String(request.reviewed);
    return this.apiHelper.handleRequest(
      this.httpClient.get<TranscriptListItemApiResponse[]>(this.path, { observe: 'response', params })
    );
  };

  getById = (id: string): Observable<ApiResult<TranscriptApiResponse>> =>
    this.apiHelper.handleRequest(
      this.httpClient.get<TranscriptApiResponse>(`${this.path}/${id}`, { observe: 'response' })
    );
}
