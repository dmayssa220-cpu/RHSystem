import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CreatePayrollVariableRequest, PayrollVariableSummary } from './payroll-variable.models';

@Injectable({ providedIn: 'root' })
export class PayrollVariableService {
    private readonly http = inject(HttpClient);
    private readonly baseUrl = '/api/paie/variables';

    list(employeeId: string, year: number, month: number): Observable<PayrollVariableSummary[]> {
        return this.http.get<PayrollVariableSummary[]>(this.baseUrl, { params: { employeeId, year, month } });
    }

    create(request: CreatePayrollVariableRequest): Observable<PayrollVariableSummary> {
        return this.http.post<PayrollVariableSummary>(this.baseUrl, request);
    }

    delete(id: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/${id}`);
    }
}
