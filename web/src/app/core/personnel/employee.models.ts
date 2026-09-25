export interface EmployeeSummary {
    id: string;
    firstName: string;
    lastName: string;
    status: string;
    hireDate: string;
}

export interface EmployeeDetail extends EmployeeSummary {
    establishmentId: string;
    departmentId: string | null;
    jobPositionId: string | null;
    gender: string;
    dateOfBirth: string;
    nationalId: string;
    personalEmail: string | null;
    personalPhone: string | null;
}

export interface CreateEmployeeRequest {
    establishmentId: string;
    departmentId: string | null;
    jobPositionId: string | null;
    firstName: string;
    lastName: string;
    gender: string;
    dateOfBirth: string;
    nationalId: string;
    personalEmail: string | null;
    personalPhone: string | null;
    hireDate: string;
}

export interface UpdateEmployeeRequest {
    departmentId: string | null;
    jobPositionId: string | null;
    personalEmail: string | null;
    personalPhone: string | null;
    status: string;
}

export const GENDER_OPTIONS = [
    { label: 'Femme', value: 'Femme' },
    { label: 'Homme', value: 'Homme' }
];

export const EMPLOYEE_STATUS_OPTIONS = [
    { label: 'Actif', value: 'Actif' },
    { label: 'Suspendu', value: 'Suspendu' },
    { label: 'Sorti définitivement', value: 'SortiDefinitivement' }
];

export function employeeStatusLabel(status: string): string {
    return EMPLOYEE_STATUS_OPTIONS.find((option) => option.value === status)?.label ?? status;
}

export function employeeStatusSeverity(status: string): 'success' | 'warn' | 'danger' {
    if (status === 'Actif') return 'success';
    if (status === 'Suspendu') return 'warn';
    return 'danger';
}
