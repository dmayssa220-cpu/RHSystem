import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { AiService } from '@/app/core/ai/ai.service';
import { PayrollReadinessReport, WorkCertificateResult } from '@/app/core/ai/ai.models';
import { EmployeeService } from '@/app/core/personnel/employee.service';
import { EmployeeSummary } from '@/app/core/personnel/employee.models';

@Component({
    selector: 'app-assistant',
    standalone: true,
    imports: [CommonModule, FormsModule, ButtonModule, SelectModule, InputNumberModule, TagModule, ToastModule],
    providers: [MessageService],
    template: `
        <div class="card mb-6">
            <div class="flex items-center gap-3 mb-2">
                <i class="pi pi-sparkles text-3xl text-primary"></i>
                <h1 class="text-2xl font-semibold m-0">Assistant IA</h1>
            </div>
            <p class="text-muted-color mb-0">
                L'IA ne calcule jamais rien ici : elle reformule des textes déjà exacts (gabarits) et
                signale des points à vérifier. Si une reformulation change un montant ou une date, elle
                est rejetée automatiquement et le texte d'origine s'affiche à la place.
            </p>
            <div class="mt-4">
                @if (aiConfigured() === null) {
                    <p-tag value="Vérification…" severity="secondary" />
                } @else if (aiConfigured()) {
                    <p-tag value="Fournisseur IA configuré" severity="success" />
                } @else {
                    <p-tag value="Aucun fournisseur IA configuré — réponses par gabarit" severity="warn" />
                }
            </div>
        </div>

        <div class="card mb-6">
            <h2 class="text-lg font-semibold mb-2">Agent de préparation de paie</h2>
            <p class="text-sm text-muted-color mb-4">
                Vérifie, avant de clôturer un mois, les salariés sans contrat et les demandes
                d'absence pas encore validées sur la période.
            </p>
            <div class="flex items-end gap-4 mb-4">
                <div>
                    <label class="block mb-2">Mois</label>
                    <p-select [(ngModel)]="readinessMonth" [options]="monthOptions" optionLabel="label" optionValue="value" />
                </div>
                <div>
                    <label class="block mb-2">Année</label>
                    <p-inputnumber [(ngModel)]="readinessYear" [useGrouping]="false" />
                </div>
                <p-button label="Vérifier" icon="pi pi-search" [loading]="checkingReadiness()" (onClick)="checkReadiness()" />
            </div>

            @if (readinessReport(); as report) {
                <div class="p-4 rounded-border bg-primary-50 dark:bg-primary-950 text-sm mb-3">
                    {{ report.summary }}
                    @if (report.summaryReformulated) {
                        <span class="text-xs text-muted-color ml-2">(reformulé par l'IA)</span>
                    }
                </div>
                <div class="text-sm">{{ report.readyEmployeeCount }} / {{ report.activeEmployeeCount }} salarié(s) prêt(s)</div>
                @for (issue of report.issues; track issue.message) {
                    <div class="text-sm text-muted-color py-1">• {{ issue.message }}</div>
                }
            }
        </div>

        <div class="card">
            <h2 class="text-lg font-semibold mb-2">Attestation de travail</h2>
            <p class="text-sm text-muted-color mb-4">
                Les faits (identité, dates, contrat) viennent toujours du dossier salarié ; seule la
                formulation peut être confiée à l'IA, jamais les faits eux-mêmes.
            </p>
            <div class="flex items-end gap-4 mb-4">
                <div class="flex-1 max-w-sm">
                    <label class="block mb-2">Salarié</label>
                    <p-select [(ngModel)]="certificateEmployeeId" [options]="employees()" optionLabel="displayName" optionValue="id" placeholder="Choisir un salarié" fluid />
                </div>
                <p-button label="Générer" icon="pi pi-file" [loading]="generatingCertificate()" (onClick)="generateCertificate()" />
            </div>

            @if (certificateResult(); as result) {
                <pre class="whitespace-pre-wrap text-sm p-4 rounded-border bg-surface-50 dark:bg-surface-900">{{ result.content }}</pre>
                @if (result.reformulated) {
                    <div class="text-xs text-muted-color mt-2">Formulation affinée par l'IA ; dates et identité vérifiées identiques à l'original.</div>
                }
            }
        </div>

        <p-toast />
    `
})
export class Assistant implements OnInit {
    private readonly aiService = inject(AiService);
    private readonly employeeService = inject(EmployeeService);
    private readonly messageService = inject(MessageService);

    readonly aiConfigured = signal<boolean | null>(null);
    readonly employees = signal<(EmployeeSummary & { displayName: string })[]>([]);

    private readonly now = new Date();
    readonly monthOptions = [
        { label: 'Janvier', value: 1 }, { label: 'Février', value: 2 }, { label: 'Mars', value: 3 },
        { label: 'Avril', value: 4 }, { label: 'Mai', value: 5 }, { label: 'Juin', value: 6 },
        { label: 'Juillet', value: 7 }, { label: 'Août', value: 8 }, { label: 'Septembre', value: 9 },
        { label: 'Octobre', value: 10 }, { label: 'Novembre', value: 11 }, { label: 'Décembre', value: 12 }
    ];

    readinessYear = this.now.getFullYear();
    readinessMonth = this.now.getMonth() + 1;
    readonly checkingReadiness = signal(false);
    readonly readinessReport = signal<PayrollReadinessReport | null>(null);

    certificateEmployeeId: string | null = null;
    readonly generatingCertificate = signal(false);
    readonly certificateResult = signal<WorkCertificateResult | null>(null);

    ngOnInit(): void {
        this.aiService.status().subscribe({
            next: (status) => this.aiConfigured.set(status.configured),
            error: () => this.aiConfigured.set(false)
        });

        this.employeeService.list().subscribe((employees) => {
            this.employees.set(employees.map((e) => ({ ...e, displayName: `${e.lastName} ${e.firstName}` })));
        });
    }

    checkReadiness(): void {
        this.checkingReadiness.set(true);
        this.aiService.checkPayrollReadiness(this.readinessYear, this.readinessMonth).subscribe({
            next: (report) => {
                this.checkingReadiness.set(false);
                this.readinessReport.set(report);
            },
            error: () => {
                this.checkingReadiness.set(false);
                this.messageService.add({ severity: 'error', summary: 'Erreur', detail: 'La vérification a échoué.' });
            }
        });
    }

    generateCertificate(): void {
        if (!this.certificateEmployeeId) {
            this.messageService.add({ severity: 'warn', summary: 'Champ manquant', detail: 'Choisis un salarié.' });
            return;
        }

        this.generatingCertificate.set(true);
        this.aiService.generateWorkCertificate(this.certificateEmployeeId).subscribe({
            next: (result) => {
                this.generatingCertificate.set(false);
                this.certificateResult.set(result);
            },
            error: (error) => {
                this.generatingCertificate.set(false);
                this.messageService.add({ severity: 'error', summary: 'Erreur', detail: error?.error?.message ?? 'La génération a échoué.' });
            }
        });
    }
}
