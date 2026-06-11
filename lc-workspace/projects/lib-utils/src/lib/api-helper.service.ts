import { Injectable } from '@angular/core';
import { HttpResponse } from '@angular/common/http';
import { Observable, catchError, map, of } from 'rxjs';
import { ApiResult } from './api-result';

@Injectable({ providedIn: 'root' })
export class ApiHelperService {
  handleRequest<T>(request: Observable<HttpResponse<T>>): Observable<ApiResult<T>> {
    return request.pipe(
      map((response) => ({ success: true, data: response.body, error: null })),
      catchError((error) => {
        const message: string = error?.error?.title ?? error?.message ?? 'An unexpected error occurred';
        return of({ success: false, data: null, error: message });
      })
    );
  }
}
