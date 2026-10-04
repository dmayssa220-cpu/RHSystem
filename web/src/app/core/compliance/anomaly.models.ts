export function severityLabel(severity: string): string {
    if (severity === 'Eleve') return 'Élevée';
    if (severity === 'Moyen') return 'Moyenne';
    return 'Faible';
}

export function severitySeverity(severity: string): 'success' | 'warn' | 'danger' {
    if (severity === 'Eleve') return 'danger';
    if (severity === 'Moyen') return 'warn';
    return 'success';
}

export function anomalyStatusLabel(status: string): string {
    if (status === 'Confirmee') return 'Confirmée';
    if (status === 'FauxPositif') return 'Faux positif';
    return 'Détectée';
}

export interface AnomalyAlertSummary {
    id: string;
    payslipId: string;
    employeeId: string;
    employeeFirstName: string;
    employeeLastName: string;
    periodYear: number;
    periodMonth: number;
    ruleCode: string;
    severity: string;
    message: string;
    status: string;
    decisionComment: string | null;
}
