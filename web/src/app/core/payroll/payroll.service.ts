import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { PayrollPreviewRequest, PayrollPreviewResult } from './payroll.models';
import { CloseMonthResult, CnssQuarterlyDeclaration, WithholdingDeclaration } from './declaration.models';
import { PayslipComparison } from './payslip-comparison.models';

@Injectable({ providedIn: 'root' })
export class PayrollService {
    private readonly http = inject(HttpClient);

    preview(request: PayrollPreviewRequest): Observable<PayrollPreviewResult> {
        return this.http.post<PayrollPreviewResult>('/api/paie/simuler', request);
    }

    closeMonth(year: number, month: number): Observable<CloseMonthResult> {
        return this.http.post<CloseMonthResult>('/api/paie/cloturer', { year, month });
    }

    getCnssQuarterlyDeclaration(year: number, quarter: number): Observable<CnssQuarterlyDeclaration> {
        return this.http.get<CnssQuarterlyDeclaration>(`/api/paie/declarations/cnss/${year}/${quarter}`);
    }

    getWithholdingDeclaration(year: number, month: number): Observable<WithholdingDeclaration> {
        return this.http.get<WithholdingDeclaration>(`/api/paie/declarations/retenue-source/${year}/${month}`);
    }

    comparePayslips(employeeId: string, yearBefore: number, monthBefore: number, yearAfter: number, monthAfter: number): Observable<PayslipComparison> {
        return this.http.get<PayslipComparison>(`/api/paie/bulletins/${employeeId}/comparaison`, {
            params: { yearBefore, monthBefore, yearAfter, monthAfter }
        });
    }
}
