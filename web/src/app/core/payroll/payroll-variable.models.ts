export const PAYROLL_VARIABLE_TYPE_OPTIONS = [
    { label: 'Prime', value: 'Prime' },
    { label: 'Heures supplémentaires', value: 'HeuresSupplementaires' },
    { label: 'Autre', value: 'Autre' }
];

export interface PayrollVariableSummary {
    id: string;
    employeeId: string;
    periodYear: number;
    periodMonth: number;
    type: string;
    label: string;
    amount: number;
}

export interface CreatePayrollVariableRequest {
    employeeId: string;
    periodYear: number;
    periodMonth: number;
    type: string;
    label: string;
    amount: number;
}
