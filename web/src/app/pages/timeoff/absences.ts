import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { DatePickerModule } from 'primeng/datepicker';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ToolbarModule } from 'primeng/toolbar';
import { AbsenceService } from '@/app/core/timeoff/absence.service';
import { ABSENCE_TYPE_OPTIONS, AbsenceSummary, absenceStatusLabel, absenceStatusSeverity, absenceTypeLabel } from '@/app/core/timeoff/absence.models';
import { EmployeeService } from '@/app/core/personnel/employee.service';
import { EmployeeSummary } from '@/app/core/personnel/employee.models';

interface RequestFormModel {
    employeeId: string | null;
    type: string;
    startDate: Date | null;
    endDate: Date | null;
    comment: string;
}

function emptyForm(): RequestFormModel {
    return { employeeId: null, type: 'CongePaye', startDate: new Date(), endDate: new Date(), comment: '' };
}

@Component({
    selector: 'app-absences',
    standalone: true,
    imports: [CommonModule, FormsModule, TableModule, ButtonModule, ToolbarModule, ToastModule, DialogModule, InputTextModule, SelectModule, DatePickerModule, TagModule],
    providers: [MessageService],
    template: `
        <div class="card">
            <p-toolbar styleClass="mb-6">
                <ng-template #start>
                    <h5 class="m-0">Temps et absences</h5>
                </ng-template>
                <ng-template #end>
                    <p-button label="Nouvelle demande" icon="pi pi-plus" (onClick)="openNew()" />
                </ng-template>
            </p-toolbar>

            <p-table [value]="absences()" [rows]="10" [paginator]="true" [loading]="loading()" dataKey="id">
                <ng-template #header>
                    <tr>
                        <th>Salarié</th>
                        <th>Type</th>
                        <th>Du</th>
                        <th>Au</th>
                        <th>Statut</th>
                        <th style="width: 10rem"></th>
                    </tr>
                </ng-template>
                <ng-template #body let-absence>
                    <tr>
                        <td>{{ absence.employeeLastName }} {{ absence.employeeFirstName }}</td>
                        <td>{{ typeLabel(absence.type) }}</td>
                        <td>{{ absence.startDate | date: 'dd/MM/yyyy' }}</td>
                        <td>{{ absence.endDate | date: 'dd/MM/yyyy' }}</td>
                        <td><p-tag [value]="statusLabel(absence.status)" [severity]="statusSeverity(absence.status)" /></td>
                        <td>
                            @if (absence.status === 'EnAttente') {
                                <p-button icon="pi pi-check" class="mr-2" severity="success" [rounded]="true" [outlined]="true" (click)="decide(absence, true)" />
                                <p-button icon="pi pi-times" severity="danger" [rounded]="true" [outlined]="true" (click)="decide(absence, false)" />
                            }
                        </td>
                    </tr>
                </ng-template>
                <ng-template #empty>
                    <tr>
                        <td colspan="6" class="text-center py-6">Aucune demande d'absence pour l'instant.</td>
                    </tr>
                </ng-template>
            </p-table>
        </div>

        <p-dialog [(visible)]="dialogVisible" [style]="{ width: '480px' }" header="Nouvelle demande d'absence" [modal]="true">
            <ng-template #content>
                <div class="flex flex-col gap-4">
                    <div>
                        <label class="block font-bold mb-2">Salarié</label>
                        <p-select [(ngModel)]="form.employeeId" [options]="employees()" optionLabel="displayName" optionValue="id" placeholder="Choisir un salarié" fluid />
                    </div>
                    <div>
                        <label class="block font-bold mb-2">Type</label>
                        <p-select [(ngModel)]="form.type" [options]="typeOptions" optionLabel="label" optionValue="value" fluid />
                    </div>
                    <div class="grid grid-cols-12 gap-4">
                        <div class="col-span-6">
                            <label class="block font-bold mb-2">Du</label>
                            <p-datepicker [(ngModel)]="form.startDate" dateFormat="dd/mm/yy" fluid />
                        </div>
                        <div class="col-span-6">
                            <label class="block font-bold mb-2">Au</label>
                            <p-datepicker [(ngModel)]="form.endDate" dateFormat="dd/mm/yy" fluid />
                        </div>
                    </div>
                    <div>
                        <label class="block font-bold mb-2">Commentaire</label>
                        <input pInputText [(ngModel)]="form.comment" fluid />
                    </div>
                </div>
            </ng-template>
            <ng-template #footer>
                <p-button label="Annuler" icon="pi pi-times" text (click)="dialogVisible = false" />
                <p-button label="Envoyer la demande" icon="pi pi-check" [loading]="saving()" (click)="submit()" />
            </ng-template>
        </p-dialog>

        <p-toast />
    `
})
export class Absences implements OnInit {
    private readonly absenceService = inject(AbsenceService);
    private readonly employeeService = inject(EmployeeService);
    private readonly messageService = inject(MessageService);

    readonly absences = signal<AbsenceSummary[]>([]);
    readonly employees = signal<(EmployeeSummary & { displayName: string })[]>([]);
    readonly loading = signal(false);
    readonly saving = signal(false);
    readonly typeOptions = ABSENCE_TYPE_OPTIONS;

    typeLabel = absenceTypeLabel;
    statusLabel = absenceStatusLabel;
    statusSeverity = absenceStatusSeverity;

    dialogVisible = false;
    form: RequestFormModel = emptyForm();

    ngOnInit(): void {
        this.reload();
    }

    reload(): void {
        this.loading.set(true);
        this.absenceService.list().subscribe({
            next: (absences) => {
                this.absences.set(absences);
                this.loading.set(false);
            },
            error: () => {
                this.loading.set(false);
                this.messageService.add({ severity: 'error', summary: 'Erreur', detail: 'Impossible de charger les absences.' });
            }
        });
    }

    openNew(): void {
        this.form = emptyForm();
        this.dialogVisible = true;

        if (this.employees().length === 0) {
            this.employeeService.list().subscribe((employees) => {
                this.employees.set(employees.map((e) => ({ ...e, displayName: `${e.lastName} ${e.firstName}` })));
            });
        }
    }

    submit(): void {
        if (!this.form.employeeId || !this.form.startDate || !this.form.endDate) {
            this.messageService.add({ severity: 'warn', summary: 'Champs manquants', detail: 'Salarié, date de début et date de fin sont obligatoires.' });
            return;
        }

        this.saving.set(true);
        this.absenceService
            .request({
                employeeId: this.form.employeeId,
                type: this.form.type,
                startDate: toIsoDate(this.form.startDate),
                endDate: toIsoDate(this.form.endDate),
                comment: this.form.comment || null
            })
            .subscribe({
                next: () => {
                    this.saving.set(false);
                    this.dialogVisible = false;
                    this.messageService.add({ severity: 'success', summary: 'Demande envoyée' });
                    this.reload();
                },
                error: (error) => {
                    this.saving.set(false);
                    this.messageService.add({ severity: 'error', summary: 'Erreur', detail: error?.error?.message ?? 'La demande a échoué.' });
                }
            });
    }

    decide(absence: AbsenceSummary, approve: boolean): void {
        this.absenceService.decide(absence.id, approve).subscribe({
            next: () => {
                this.messageService.add({ severity: 'success', summary: approve ? 'Absence validée' : 'Absence refusée' });
                this.reload();
            },
            error: () => this.messageService.add({ severity: 'error', summary: 'Erreur', detail: 'La décision a échoué.' })
        });
    }
}

function toIsoDate(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
}
