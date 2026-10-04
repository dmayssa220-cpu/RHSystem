import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ToolbarModule } from 'primeng/toolbar';
import { AnomalyService } from '@/app/core/compliance/anomaly.service';
import { AnomalyAlertSummary, anomalyStatusLabel, severityLabel, severitySeverity } from '@/app/core/compliance/anomaly.models';

const STATUS_OPTIONS = [
    { label: 'Détectées', value: 'Detectee' },
    { label: 'Confirmées', value: 'Confirmee' },
    { label: 'Faux positifs', value: 'FauxPositif' },
    { label: 'Toutes', value: '' }
];

@Component({
    selector: 'app-compliance',
    standalone: true,
    imports: [CommonModule, FormsModule, ButtonModule, SelectModule, TableModule, TagModule, ToastModule, ToolbarModule],
    providers: [MessageService],
    template: `
        <div class="card">
            <p-toolbar styleClass="mb-6">
                <ng-template #start>
                    <h5 class="m-0">Conformité et anomalies</h5>
                </ng-template>
                <ng-template #end>
                    <p-select [(ngModel)]="statusFilter" [options]="statusOptions" optionLabel="label" optionValue="value" (onChange)="reload()" />
                </ng-template>
            </p-toolbar>

            <p class="text-sm text-muted-color mb-4">
                Alertes détectées automatiquement à chaque clôture de mois : contrôles à règles (net négatif,
                salaire sous le SMIG) et statistique (variation du brut de plus de 20 % par rapport au mois
                précédent). Pas encore de modèle de machine learning — voir le README pour la suite envisagée.
            </p>

            <p-table [value]="alerts()" [rows]="10" [paginator]="true" [loading]="loading()">
                <ng-template #header>
                    <tr>
                        <th>Salarié</th>
                        <th>Période</th>
                        <th>Règle</th>
                        <th>Gravité</th>
                        <th>Message</th>
                        <th>Statut</th>
                        <th style="width: 10rem"></th>
                    </tr>
                </ng-template>
                <ng-template #body let-alert>
                    <tr>
                        <td>{{ alert.employeeLastName }} {{ alert.employeeFirstName }}</td>
                        <td>{{ alert.periodMonth }}/{{ alert.periodYear }}</td>
                        <td>{{ alert.ruleCode }}</td>
                        <td><p-tag [value]="severityLabel(alert.severity)" [severity]="severitySeverity(alert.severity)" /></td>
                        <td class="max-w-md">{{ alert.message }}</td>
                        <td>{{ statusLabel(alert.status) }}</td>
                        <td>
                            @if (alert.status === 'Detectee') {
                                <p-button icon="pi pi-check" class="mr-2" severity="success" [rounded]="true" [outlined]="true" (click)="decide(alert, true)" />
                                <p-button icon="pi pi-times" severity="secondary" [rounded]="true" [outlined]="true" (click)="decide(alert, false)" />
                            }
                        </td>
                    </tr>
                </ng-template>
                <ng-template #empty>
                    <tr>
                        <td colspan="7" class="text-center py-6">Aucune alerte pour ce filtre.</td>
                    </tr>
                </ng-template>
            </p-table>
        </div>

        <p-toast />
    `
})
export class Compliance implements OnInit {
    private readonly anomalyService = inject(AnomalyService);
    private readonly messageService = inject(MessageService);

    readonly alerts = signal<AnomalyAlertSummary[]>([]);
    readonly loading = signal(false);
    readonly statusOptions = STATUS_OPTIONS;
    statusFilter = 'Detectee';

    severityLabel = severityLabel;
    severitySeverity = severitySeverity;
    statusLabel = anomalyStatusLabel;

    ngOnInit(): void {
        this.reload();
    }

    reload(): void {
        this.loading.set(true);
        this.anomalyService.list(this.statusFilter || undefined).subscribe({
            next: (alerts) => {
                this.alerts.set(alerts);
                this.loading.set(false);
            },
            error: () => {
                this.loading.set(false);
                this.messageService.add({ severity: 'error', summary: 'Erreur', detail: 'Impossible de charger les alertes.' });
            }
        });
    }

    decide(alert: AnomalyAlertSummary, confirm: boolean): void {
        this.anomalyService.decide(alert.id, confirm).subscribe({
            next: () => {
                this.messageService.add({ severity: 'success', summary: confirm ? 'Alerte confirmée' : 'Marquée faux positif' });
                this.reload();
            },
            error: () => this.messageService.add({ severity: 'error', summary: 'Erreur', detail: 'La décision a échoué.' })
        });
    }
}
