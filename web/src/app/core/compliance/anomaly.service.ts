import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AnomalyAlertSummary } from './anomaly.models';

@Injectable({ providedIn: 'root' })
export class AnomalyService {
    private readonly http = inject(HttpClient);
    private readonly baseUrl = '/api/conformite/anomalies';

    list(status?: string): Observable<AnomalyAlertSummary[]> {
        return this.http.get<AnomalyAlertSummary[]>(this.baseUrl, { params: status ? { status } : {} });
    }

    decide(id: string, confirm: boolean, comment: string | null = null): Observable<AnomalyAlertSummary> {
        return this.http.post<AnomalyAlertSummary>(`${this.baseUrl}/${id}/decider`, { confirm, comment });
    }
}
