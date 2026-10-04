import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AbsenceSummary, RequestAbsenceRequest } from './absence.models';

@Injectable({ providedIn: 'root' })
export class AbsenceService {
    private readonly http = inject(HttpClient);
    private readonly baseUrl = '/api/temps/absences';

    list(): Observable<AbsenceSummary[]> {
        return this.http.get<AbsenceSummary[]>(this.baseUrl);
    }

    request(request: RequestAbsenceRequest): Observable<AbsenceSummary> {
        return this.http.post<AbsenceSummary>(this.baseUrl, request);
    }

    decide(id: string, approve: boolean, comment: string | null = null): Observable<AbsenceSummary> {
        return this.http.post<AbsenceSummary>(`${this.baseUrl}/${id}/decider`, { approve, comment });
    }
}
