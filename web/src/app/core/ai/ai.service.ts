import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AiStatus, ExplanationResult, PayrollReadinessReport, WorkCertificateResult } from './ai.models';

@Injectable({ providedIn: 'root' })
export class AiService {
    private readonly http = inject(HttpClient);

    status(): Observable<AiStatus> {
        return this.http.get<AiStatus>('/api/ia/statut');
    }

    checkPayrollReadiness(year: number, month: number): Observable<PayrollReadinessReport> {
        return this.http.get<PayrollReadinessReport>(`/api/ia/preparation-paie/${year}/${month}`);
    }

    explainComparison(employeeId: string, yearBefore: number, monthBefore: number, yearAfter: number, monthAfter: number): Observable<ExplanationResult> {
        return this.http.get<ExplanationResult>(`/api/ia/explication-paie/${employeeId}`, {
            params: { yearBefore, monthBefore, yearAfter, monthAfter }
        });
    }

    generateWorkCertificate(employeeId: string): Observable<WorkCertificateResult> {
        return this.http.get<WorkCertificateResult>(`/api/ia/documents/attestation-travail/${employeeId}`);
    }
}
