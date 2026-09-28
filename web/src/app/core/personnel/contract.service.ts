import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ContractSummary, CreateContractRequest } from './contract.models';

@Injectable({ providedIn: 'root' })
export class ContractService {
    private readonly http = inject(HttpClient);

    listForEmployee(employeeId: string): Observable<ContractSummary[]> {
        return this.http.get<ContractSummary[]>(`/api/personnel/salaries/${employeeId}/contrats`);
    }

    create(request: CreateContractRequest): Observable<ContractSummary> {
        return this.http.post<ContractSummary>(`/api/personnel/salaries/${request.employeeId}/contrats`, request);
    }
}
