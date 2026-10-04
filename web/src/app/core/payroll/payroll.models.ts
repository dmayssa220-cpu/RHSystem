export interface PayrollTraceLine {
    label: string;
    amount: number;
    detail: string | null;
}

export interface PayrollPreviewResult {
    employeeId: string;
    year: number;
    month: number;
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
    year?: number;
    month?: number;
    isHeadOfHousehold?: boolean;
    dependentChildren?: number;
}
