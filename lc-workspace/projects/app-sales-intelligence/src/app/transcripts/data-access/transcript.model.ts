export interface TranscriptListItemApiResponse {
  id: string;
  name: string;
  company: string;
  repName: string;
  dealStage: string;
  createdAt: string;
  reviewed: boolean;
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
