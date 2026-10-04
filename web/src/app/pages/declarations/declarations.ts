import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TabsModule } from 'primeng/tabs';
import { ToastModule } from 'primeng/toast';
import { PayrollService } from '@/app/core/payroll/payroll.service';
import { CloseMonthResult, CnssQuarterlyDeclaration, WithholdingDeclaration } from '@/app/core/payroll/declaration.models';
import { PayslipComparison } from '@/app/core/payroll/payslip-comparison.models';
import { EmployeeService } from '@/app/core/personnel/employee.service';
import { EmployeeSummary } from '@/app/core/personnel/employee.models';

const MONTH_OPTIONS = [
    { label: 'Janvier', value: 1 },
    { label: 'Février', value: 2 },
    { label: 'Mars', value: 3 },
    { label: 'Avril', value: 4 },
    { label: 'Mai', value: 5 },
    { label: 'Juin', value: 6 },
    { label: 'Juillet', value: 7 },
    { label: 'Août', value: 8 },
    { label: 'Septembre', value: 9 },
    { label: 'Octobre', value: 10 },
    { label: 'Novembre', value: 11 },
    { label: 'Décembre', value: 12 }
];

const QUARTER_OPTIONS = [
    { label: '1er trimestre (jan-mars)', value: 1 },
    { label: '2e trimestre (avr-juin)', value: 2 },
    { label: '3e trimestre (juil-sept)', value: 3 },
    { label: '4e trimestre (oct-déc)', value: 4 }
];

@Component({
    selector: 'app-declarations',
    standalone: true,
    imports: [CommonModule, FormsModule, ButtonModule, CardModule, SelectModule, InputNumberModule, TableModule, TabsModule, ToastModule],
    providers: [MessageService],
    template: `
        <div class="card">
            <div class="flex items-center gap-3 mb-4">
                <i class="pi pi-file text-3xl text-primary"></i>
                <h1 class="text-2xl font-semibold m-0">Déclarations sociales</h1>
            </div>
            <p class="text-muted-color mb-6">
                Construites à partir des bulletins déjà clôturés — jamais recalculées indépendamment, pour que la
                déclaration corresponde toujours à ce qui a été payé.
            </p>

            <p-tabs value="0">
                <p-tablist>
                    <p-tab value="0">Clôturer un mois</p-tab>
                    <p-tab value="1">CNSS (trimestrielle)</p-tab>
                    <p-tab value="2">Retenue à la source (mensuelle)</p-tab>
                    <p-tab value="3">Paie explicable (comparer deux mois)</p-tab>
                </p-tablist>
                <p-tabpanels>
                    <p-tabpanel value="0">
                        <p class="text-sm text-muted-color mb-4">
                            Calcule et enregistre le bulletin de chaque salarié actif pour le mois choisi. Un mois déjà
                            clôturé ne peut pas être reclôturé : les bulletins sont immuables.
                        </p>
                        <div class="flex items-end gap-4 mb-4">
                            <div>
                                <label class="block mb-2">Mois</label>
                                <p-select [(ngModel)]="closeMonth" [options]="monthOptions" optionLabel="label" optionValue="value" />
                            </div>
                            <div>
                                <label class="block mb-2">Année</label>
                                <p-inputnumber [(ngModel)]="closeYear" [useGrouping]="false" />
                            </div>
                            <p-button label="Clôturer le mois" icon="pi pi-lock" [loading]="closing()" (onClick)="doCloseMonth()" />
                        </div>

                        @if (closeResult(); as result) {
                            <p-table [value]="result.payslips" [rows]="10" [paginator]="result.payslips.length > 10">
                                <ng-template #header>
                                    <tr>
                                        <th>Nom</th>
                                        <th>Prénom</th>
                                        <th>Brut</th>
                                        <th>Net</th>
                                    </tr>
                                </ng-template>
                                <ng-template #body let-line>
                                    <tr>
                                        <td>{{ line.employeeLastName }}</td>
                                        <td>{{ line.employeeFirstName }}</td>
                                        <td>{{ line.grossMonthlySalary | number: '1.3-3' }} DT</td>
                                        <td>{{ line.netMonthly | number: '1.3-3' }} DT</td>
                                    </tr>
                                </ng-template>
                            </p-table>
                            <div class="text-sm text-muted-color mt-2">
                                {{ result.employeesProcessed }} bulletin(s) créé(s)
                                @if (result.employeesSkippedNoContract > 0) {
                                    , {{ result.employeesSkippedNoContract }} salarié(s) ignoré(s) (aucun contrat)
                                }
                            </div>
                        }
                    </p-tabpanel>

                    <p-tabpanel value="1">
                        <p class="text-sm text-muted-color mb-4">
                            Déclaration Trimestrielle des Salaires (DTS) — dépôt au plus tard le 15 du mois suivant le
                            trimestre échu (loi n° 60-30, art. 46).
                        </p>
                        <div class="flex items-end gap-4 mb-4">
                            <div>
                                <label class="block mb-2">Trimestre</label>
                                <p-select [(ngModel)]="cnssQuarter" [options]="quarterOptions" optionLabel="label" optionValue="value" />
                            </div>
                            <div>
                                <label class="block mb-2">Année</label>
                                <p-inputnumber [(ngModel)]="cnssYear" [useGrouping]="false" />
                            </div>
                            <p-button label="Consulter" icon="pi pi-search" [loading]="loadingCnss()" (onClick)="loadCnssDeclaration()" />
                        </div>

                        @if (cnssDeclaration(); as declaration) {
                            <div class="text-sm text-muted-color mb-2">Date limite de dépôt : {{ declaration.dueDate | date: 'dd/MM/yyyy' }}</div>
                            <p-table [value]="declaration.lines" [rows]="10" [paginator]="declaration.lines.length > 10">
                                <ng-template #header>
                                    <tr>
                                        <th>Nom</th>
                                        <th>Prénom</th>
                                        <th>Brut trimestriel</th>
                                        <th>CNSS salariale</th>
                                        <th>CNSS patronale</th>
                                    </tr>
                                </ng-template>
                                <ng-template #body let-line>
                                    <tr>
                                        <td>{{ line.employeeLastName }}</td>
                                        <td>{{ line.employeeFirstName }}</td>
                                        <td>{{ line.grossQuarterly | number: '1.3-3' }} DT</td>
                                        <td>{{ line.cnssEmployeeQuarterly | number: '1.3-3' }} DT</td>
                                        <td>{{ line.cnssEmployerQuarterly | number: '1.3-3' }} DT</td>
                                    </tr>
                                </ng-template>
                                <ng-template #footer>
                                    <tr>
                                        <td colspan="2" class="font-bold">Total</td>
                                        <td class="font-bold">{{ declaration.totalGross | number: '1.3-3' }} DT</td>
                                        <td class="font-bold">{{ declaration.totalCnssEmployee | number: '1.3-3' }} DT</td>
                                        <td class="font-bold">{{ declaration.totalCnssEmployer | number: '1.3-3' }} DT</td>
                                    </tr>
                                </ng-template>
                                <ng-template #empty>
                                    <tr>
                                        <td colspan="5" class="text-center py-4">Aucun bulletin clôturé sur ce trimestre.</td>
                                    </tr>
                                </ng-template>
                            </p-table>
                        }
                    </p-tabpanel>

                    <p-tabpanel value="2">
                        <p class="text-sm text-muted-color mb-4">
                            Retenue à la source (IRPP + CSS) — dépôt au plus tard le 15 du mois suivant.
                        </p>
                        <div class="flex items-end gap-4 mb-4">
                            <div>
                                <label class="block mb-2">Mois</label>
                                <p-select [(ngModel)]="withholdingMonth" [options]="monthOptions" optionLabel="label" optionValue="value" />
                            </div>
                            <div>
                                <label class="block mb-2">Année</label>
                                <p-inputnumber [(ngModel)]="withholdingYear" [useGrouping]="false" />
                            </div>
                            <p-button label="Consulter" icon="pi pi-search" [loading]="loadingWithholding()" (onClick)="loadWithholdingDeclaration()" />
                        </div>

                        @if (withholdingDeclaration(); as declaration) {
                            <div class="text-sm text-muted-color mb-2">Date limite de dépôt : {{ declaration.dueDate | date: 'dd/MM/yyyy' }}</div>
                            <p-table [value]="declaration.lines" [rows]="10" [paginator]="declaration.lines.length > 10">
                                <ng-template #header>
                                    <tr>
                                        <th>Nom</th>
                                        <th>Prénom</th>
                                        <th>IRPP</th>
                                        <th>CSS</th>
                                    </tr>
                                </ng-template>
                                <ng-template #body let-line>
                                    <tr>
                                        <td>{{ line.employeeLastName }}</td>
                                        <td>{{ line.employeeFirstName }}</td>
                                        <td>{{ line.irppMonthly | number: '1.3-3' }} DT</td>
                                        <td>{{ line.cssMonthly | number: '1.3-3' }} DT</td>
                                    </tr>
                                </ng-template>
                                <ng-template #footer>
                                    <tr>
                                        <td colspan="2" class="font-bold">Total</td>
                                        <td class="font-bold">{{ declaration.totalIrpp | number: '1.3-3' }} DT</td>
                                        <td class="font-bold">{{ declaration.totalCss | number: '1.3-3' }} DT</td>
                                    </tr>
                                </ng-template>
                                <ng-template #empty>
                                    <tr>
                                        <td colspan="4" class="text-center py-4">Aucun bulletin clôturé sur ce mois.</td>
                                    </tr>
                                </ng-template>
                            </p-table>
                        }
                    </p-tabpanel>

                    <p-tabpanel value="3">
                        <p class="text-sm text-muted-color mb-4">
                            Compare deux bulletins déjà clôturés du même salarié, ligne de trace par ligne de
                            trace, et explique l'écart de net à partir des montants réels — rien n'est inventé.
                        </p>
                        <div class="flex flex-wrap items-end gap-4 mb-4">
                            <div>
                                <label class="block mb-2">Salarié</label>
                                <p-select [(ngModel)]="comparisonEmployeeId" [options]="employees()" optionLabel="displayName" optionValue="id" placeholder="Choisir un salarié" style="min-width: 14rem" />
                            </div>
                            <div>
                                <label class="block mb-2">Mois de référence</label>
                                <p-select [(ngModel)]="comparisonMonthBefore" [options]="monthOptions" optionLabel="label" optionValue="value" />
                            </div>
                            <div>
                                <label class="block mb-2">Année</label>
                                <p-inputnumber [(ngModel)]="comparisonYearBefore" [useGrouping]="false" />
                            </div>
                            <div>
                                <label class="block mb-2">Mois comparé</label>
                                <p-select [(ngModel)]="comparisonMonthAfter" [options]="monthOptions" optionLabel="label" optionValue="value" />
                            </div>
                            <div>
                                <label class="block mb-2">Année</label>
                                <p-inputnumber [(ngModel)]="comparisonYearAfter" [useGrouping]="false" />
                            </div>
                            <p-button label="Comparer" icon="pi pi-search" [loading]="comparing()" (onClick)="compare()" />
                        </div>

                        @if (comparison(); as cmp) {
                            <div class="p-4 rounded-border bg-primary-50 dark:bg-primary-950 text-sm mb-4">{{ cmp.narrative }}</div>
                            <p-table [value]="cmp.lines">
                                <ng-template #header>
                                    <tr>
                                        <th>Ligne</th>
                                        <th>Avant</th>
                                        <th>Après</th>
                                        <th>Écart</th>
                                    </tr>
                                </ng-template>
                                <ng-template #body let-line>
                                    <tr>
                                        <td>{{ line.label }}</td>
                                        <td>{{ line.amountBefore !== null ? (line.amountBefore | number: '1.3-3') + ' DT' : '—' }}</td>
                                        <td>{{ line.amountAfter !== null ? (line.amountAfter | number: '1.3-3') + ' DT' : '—' }}</td>
                                        <td [class]="line.delta > 0 ? 'text-green-600' : 'text-red-500'">{{ line.delta > 0 ? '+' : '' }}{{ line.delta | number: '1.3-3' }} DT</td>
                                    </tr>
                                </ng-template>
                                <ng-template #footer>
                                    <tr>
                                        <td class="font-bold">Salaire net à payer</td>
                                        <td class="font-bold">{{ cmp.netBefore | number: '1.3-3' }} DT</td>
                                        <td class="font-bold">{{ cmp.netAfter | number: '1.3-3' }} DT</td>
                                        <td class="font-bold" [class]="cmp.netDelta > 0 ? 'text-green-600' : 'text-red-500'">{{ cmp.netDelta > 0 ? '+' : '' }}{{ cmp.netDelta | number: '1.3-3' }} DT</td>
                                    </tr>
                                </ng-template>
                            </p-table>
                        }
                    </p-tabpanel>
                </p-tabpanels>
            </p-tabs>
        </div>

        <p-toast />
    `
})
export class Declarations implements OnInit {
    private readonly payrollService = inject(PayrollService);
    private readonly employeeService = inject(EmployeeService);
    private readonly messageService = inject(MessageService);

    readonly monthOptions = MONTH_OPTIONS;
    readonly quarterOptions = QUARTER_OPTIONS;

    private readonly now = new Date();

    closeYear = this.now.getFullYear();
    closeMonth = this.now.getMonth() + 1;
    readonly closing = signal(false);
    readonly closeResult = signal<CloseMonthResult | null>(null);

    cnssYear = this.now.getFullYear();
    cnssQuarter = Math.ceil((this.now.getMonth() + 1) / 3);
    readonly loadingCnss = signal(false);
    readonly cnssDeclaration = signal<CnssQuarterlyDeclaration | null>(null);

    withholdingYear = this.now.getFullYear();
    withholdingMonth = this.now.getMonth() + 1;
    readonly loadingWithholding = signal(false);
    readonly withholdingDeclaration = signal<WithholdingDeclaration | null>(null);

    readonly employees = signal<(EmployeeSummary & { displayName: string })[]>([]);
    comparisonEmployeeId: string | null = null;
    comparisonYearBefore = this.now.getFullYear();
    comparisonMonthBefore = this.now.getMonth() === 0 ? 12 : this.now.getMonth();
    comparisonYearAfter = this.now.getFullYear();
    comparisonMonthAfter = this.now.getMonth() + 1;
    readonly comparing = signal(false);
    readonly comparison = signal<PayslipComparison | null>(null);

    doCloseMonth(): void {
        this.closing.set(true);
        this.payrollService.closeMonth(this.closeYear, this.closeMonth).subscribe({
            next: (result) => {
                this.closing.set(false);
                this.closeResult.set(result);
                this.messageService.add({ severity: 'success', summary: 'Mois clôturé', detail: `${result.employeesProcessed} bulletin(s) créé(s).` });
            },
            error: (error) => {
                this.closing.set(false);
                this.messageService.add({ severity: 'error', summary: 'Erreur', detail: error?.error?.message ?? 'La clôture a échoué.' });
            }
        });
    }

    loadCnssDeclaration(): void {
        this.loadingCnss.set(true);
        this.payrollService.getCnssQuarterlyDeclaration(this.cnssYear, this.cnssQuarter).subscribe({
            next: (declaration) => {
                this.loadingCnss.set(false);
                this.cnssDeclaration.set(declaration);
            },
            error: () => {
                this.loadingCnss.set(false);
                this.messageService.add({ severity: 'error', summary: 'Erreur', detail: 'Impossible de charger la déclaration.' });
            }
        });
    }

    loadWithholdingDeclaration(): void {
        this.loadingWithholding.set(true);
        this.payrollService.getWithholdingDeclaration(this.withholdingYear, this.withholdingMonth).subscribe({
            next: (declaration) => {
                this.loadingWithholding.set(false);
                this.withholdingDeclaration.set(declaration);
            },
            error: () => {
                this.loadingWithholding.set(false);
                this.messageService.add({ severity: 'error', summary: 'Erreur', detail: 'Impossible de charger la déclaration.' });
            }
        });
    }

    ngOnInit(): void {
        this.employeeService.list().subscribe((employees) => {
            this.employees.set(employees.map((e) => ({ ...e, displayName: `${e.lastName} ${e.firstName}` })));
        });
    }

    compare(): void {
        if (!this.comparisonEmployeeId) {
            this.messageService.add({ severity: 'warn', summary: 'Champ manquant', detail: 'Choisis un salarié.' });
            return;
        }

        this.comparing.set(true);
        this.payrollService
            .comparePayslips(this.comparisonEmployeeId, this.comparisonYearBefore, this.comparisonMonthBefore, this.comparisonYearAfter, this.comparisonMonthAfter)
            .subscribe({
                next: (comparison) => {
                    this.comparing.set(false);
                    this.comparison.set(comparison);
                },
                error: (error) => {
                    this.comparing.set(false);
                    this.messageService.add({ severity: 'error', summary: 'Erreur', detail: error?.error?.message ?? 'La comparaison a échoué.' });
                }
            });
    }
}
