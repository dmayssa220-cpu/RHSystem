export interface PayslipComparisonLine {
    label: string;
    amountBefore: number | null;
    amountAfter: number | null;
    delta: number;
}

export interface PayslipComparison {
    employeeId: string;
    yearBefore: number;
    monthBefore: number;
    yearAfter: number;
    monthAfter: number;
    netBefore: number;
    netAfter: number;
    netDelta: number;
    lines: PayslipComparisonLine[];
    narrative: string;
}
