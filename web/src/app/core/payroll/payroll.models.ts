export interface PayrollTraceLine {
    label: string;
    amount: number;
    detail: string | null;
}

export interface PayrollPreviewResult {
    employeeId: string;
    grossMonthlySalary: number;
    cnssEmployeeMonthly: number;
    cnssEmployerMonthly: number;
    taxableAnnual: number;
    irppAnnual: number;
    irppMonthly: number;
    cssMonthly: number;
    netMonthly: number;
    trace: PayrollTraceLine[];
}

export interface PayrollPreviewRequest {
    employeeId: string;
    isHeadOfHousehold: boolean;
    dependentChildren: number;
}
