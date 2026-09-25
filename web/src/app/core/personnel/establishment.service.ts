import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export interface EstablishmentSummary {
    id: string;
    name: string;
}

@Injectable({ providedIn: 'root' })
export class EstablishmentService {
    private readonly http = inject(HttpClient);

    list(): Observable<EstablishmentSummary[]> {
        return this.http.get<EstablishmentSummary[]>('/api/personnel/etablissements');
    }
}
