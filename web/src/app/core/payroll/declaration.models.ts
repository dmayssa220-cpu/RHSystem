export interface CloseMonthResult {
    year: number;
    month: number;
    employeesProcessed: number;
    employeesSkippedNoContract: number;
    payslips: { id: string; employeeFirstName: string; employeeLastName: string; grossMonthlySalary: number; netMonthly: number }[];
}

export interface CnssQuarterlyDeclarationLine {
    employeeId: string;
    employeeFirstName: string;
    employeeLastName: string;
    grossQuarterly: number;
    cnssEmployeeQuarterly: number;
    cnssEmployerQuarterly: number;
}

export interface CnssQuarterlyDeclaration {
    year: number;
    quarter: number;
    dueDate: string;
    lines: CnssQuarterlyDeclarationLine[];
    totalGross: number;
    totalCnssEmployee: number;
    totalCnssEmployer: number;
}

export interface WithholdingDeclarationLine {
    employeeId: string;
    employeeFirstName: string;
    employeeLastName: string;
    irppMonthly: number;
    cssMonthly: number;
}

export interface WithholdingDeclaration {
    year: number;
    month: number;
    dueDate: string;
    lines: WithholdingDeclarationLine[];
    totalIrpp: number;
    totalCss: number;
}
