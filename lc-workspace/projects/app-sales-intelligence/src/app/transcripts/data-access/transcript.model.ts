export interface TranscriptListItemApiResponse {
  id: string;
  name: string;
  company: string | null;
  repName: string | null;
  dealStage: string | null;
  callType: string | null;
  audience: string | null;
  durationMins: number | null;
  createdAt: string;
  reviewed: boolean;
  callQuality: string | null;
  coachableMoments: boolean;
}

export interface TranscriptApiResponse {
  id: string;
  name: string;
  company: string;
  repName: string;
  opportunityId: string;
  dealStage: string;
  dealType: string;
  duration: number | null;
  keyTopics: string[];
  coachableMoments: boolean;
  reviewed: boolean;
  createdAt: string;
  transcriptText: string;
}

export interface GetTranscriptsApiRequest {
  repName?: string;
  dealStage?: string;
  reviewed?: boolean;
}
