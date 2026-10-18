export interface AiStatus {
    configured: boolean;
}

export interface PayrollReadinessIssue {
    category: string;
    employeeId: string | null;
    message: string;
}

export interface PayrollReadinessReport {
    year: number;
    month: number;
    activeEmployeeCount: number;
    readyEmployeeCount: number;
    issues: PayrollReadinessIssue[];
    summary: string;
    summaryReformulated: boolean;
}

export interface ExplanationResult {
    text: string;
    reformulated: boolean;
}

export interface WorkCertificateResult {
    content: string;
    reformulated: boolean;
}
