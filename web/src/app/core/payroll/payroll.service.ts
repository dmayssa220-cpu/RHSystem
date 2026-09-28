import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { PayrollPreviewRequest, PayrollPreviewResult } from './payroll.models';

@Injectable({ providedIn: 'root' })
export class PayrollService {
    private readonly http = inject(HttpClient);

    preview(request: PayrollPreviewRequest): Observable<PayrollPreviewResult> {
        return this.http.post<PayrollPreviewResult>('/api/paie/simuler', request);
    }
}
