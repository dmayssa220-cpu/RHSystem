export const ABSENCE_TYPE_OPTIONS = [
    { label: 'Congé payé', value: 'CongePaye' },
    { label: 'Maladie', value: 'Maladie' },
    { label: 'Maternité', value: 'Maternite' },
    { label: 'Paternité', value: 'Paternite' },
    { label: 'Sans solde', value: 'SansSolde' },
    { label: 'Autre', value: 'Autre' }
];

export function absenceTypeLabel(type: string): string {
    return ABSENCE_TYPE_OPTIONS.find((o) => o.value === type)?.label ?? type;
}

export function absenceStatusSeverity(status: string): 'success' | 'warn' | 'danger' {
    if (status === 'Validee') return 'success';
    if (status === 'EnAttente') return 'warn';
    return 'danger';
}

export function absenceStatusLabel(status: string): string {
    if (status === 'Validee') return 'Validée';
    if (status === 'EnAttente') return 'En attente';
    return 'Refusée';
}

export interface AbsenceSummary {
    id: string;
    employeeId: string;
    employeeFirstName: string;
    employeeLastName: string;
    type: string;
    startDate: string;
    endDate: string;
    status: string;
    comment: string | null;
}

export interface RequestAbsenceRequest {
    employeeId: string;
    type: string;
    startDate: string;
    endDate: string;
    comment: string | null;
}
