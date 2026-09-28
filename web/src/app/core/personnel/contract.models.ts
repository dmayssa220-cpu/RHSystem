export const CONTRACT_TYPE_OPTIONS = [
    { label: 'CDI', value: 'Cdi' },
    { label: 'CDD', value: 'Cdd' },
    { label: 'Stage', value: 'Stage' },
    { label: 'Alternance', value: 'Alternance' }
];

export interface ContractSummary {
    id: string;
    employeeId: string;
    type: string;
    startDate: string;
    endDate: string | null;
    baseSalary: number;
}

export interface CreateContractRequest {
    employeeId: string;
    type: string;
    startDate: string;
    endDate: string | null;
    baseSalary: number;
    weeklyHours: number | null;
}
