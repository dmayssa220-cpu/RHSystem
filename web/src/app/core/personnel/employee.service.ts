import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateEmployeeRequest, EmployeeDetail, EmployeeSummary, UpdateEmployeeRequest } from './employee.models';

@Injectable({ providedIn: 'root' })
export class EmployeeService {
    private readonly http = inject(HttpClient);
    private readonly baseUrl = '/api/personnel/salaries';

    list(): Observable<EmployeeSummary[]> {
        return this.http.get<EmployeeSummary[]>(this.baseUrl);
    }

    get(id: string): Observable<EmployeeDetail> {
        return this.http.get<EmployeeDetail>(`${this.baseUrl}/${id}`);
    }

    create(request: CreateEmployeeRequest): Observable<EmployeeDetail> {
        return this.http.post<EmployeeDetail>(this.baseUrl, request);
    }

    update(id: string, request: UpdateEmployeeRequest): Observable<EmployeeDetail> {
        return this.http.put<EmployeeDetail>(`${this.baseUrl}/${id}`, request);
    }
}
