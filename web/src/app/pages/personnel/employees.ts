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
import { EmployeeService } from '@/app/core/personnel/employee.service';
import { EstablishmentService } from '@/app/core/personnel/establishment.service';
import { EmployeeDetail, EmployeeSummary, GENDER_OPTIONS, EMPLOYEE_STATUS_OPTIONS, employeeStatusLabel, employeeStatusSeverity } from '@/app/core/personnel/employee.models';

interface EstablishmentOption {
    id: string;
    name: string;
}

/** Formulaire de création (les champs d'édition, plus restreints, se saisissent au même endroit ; voir saveEmployee). */
interface EmployeeFormModel {
    establishmentId: string | null;
    firstName: string;
    lastName: string;
    gender: string;
    dateOfBirth: Date | null;
    nationalId: string;
    personalEmail: string;
    personalPhone: string;
    hireDate: Date | null;
    status: string;
}

function emptyForm(): EmployeeFormModel {
    return {
        establishmentId: null,
        firstName: '',
        lastName: '',
        gender: 'Femme',
        dateOfBirth: null,
        nationalId: '',
        personalEmail: '',
        personalPhone: '',
        hireDate: new Date(),
        status: 'Actif'
    };
}

@Component({
    selector: 'app-employees',
    standalone: true,
    imports: [CommonModule, FormsModule, TableModule, ButtonModule, ToolbarModule, ToastModule, DialogModule, InputTextModule, SelectModule, DatePickerModule, TagModule],
    providers: [MessageService],
    template: `
        <div class="card">
            <p-toolbar styleClass="mb-6">
                <ng-template #start>
                    <h5 class="m-0">Salariés</h5>
                </ng-template>
                <ng-template #end>
                    <p-button label="Nouveau salarié" icon="pi pi-plus" (onClick)="openNew()" />
                </ng-template>
            </p-toolbar>

            <p-table [value]="employees()" [rows]="10" [paginator]="true" [loading]="loading()" dataKey="id" [rowHover]="true" currentPageReportTemplate="{first} à {last} sur {totalRecords} salariés" [showCurrentPageReport]="true">
                <ng-template #header>
                    <tr>
                        <th>Nom</th>
                        <th>Prénom</th>
                        <th>Date d'embauche</th>
                        <th>Statut</th>
                        <th style="width: 6rem"></th>
                    </tr>
                </ng-template>
                <ng-template #body let-employee>
                    <tr>
                        <td>{{ employee.lastName }}</td>
                        <td>{{ employee.firstName }}</td>
                        <td>{{ employee.hireDate | date: 'dd/MM/yyyy' }}</td>
                        <td><p-tag [value]="statusLabel(employee.status)" [severity]="statusSeverity(employee.status)" /></td>
                        <td>
                            <p-button icon="pi pi-pencil" [rounded]="true" [outlined]="true" (click)="openEdit(employee)" />
                        </td>
                    </tr>
                </ng-template>
                <ng-template #empty>
                    <tr>
                        <td colspan="5" class="text-center py-6">Aucun salarié pour l'instant.</td>
                    </tr>
                </ng-template>
            </p-table>
        </div>

        <p-dialog [(visible)]="dialogVisible" [style]="{ width: '480px' }" [header]="editingId ? 'Modifier le salarié' : 'Nouveau salarié'" [modal]="true">
            <ng-template #content>
                <div class="flex flex-col gap-4">
                    @if (!editingId) {
                        <div>
                            <label class="block font-bold mb-2">Établissement</label>
                            <p-select [(ngModel)]="form.establishmentId" [options]="establishments()" optionLabel="name" optionValue="id" placeholder="Choisir un établissement" fluid />
                        </div>
                        <div class="grid grid-cols-12 gap-4">
                            <div class="col-span-6">
                                <label class="block font-bold mb-2">Prénom</label>
                                <input pInputText [(ngModel)]="form.firstName" fluid />
                            </div>
                            <div class="col-span-6">
                                <label class="block font-bold mb-2">Nom</label>
                                <input pInputText [(ngModel)]="form.lastName" fluid />
                            </div>
                        </div>
                        <div class="grid grid-cols-12 gap-4">
                            <div class="col-span-6">
                                <label class="block font-bold mb-2">Genre</label>
                                <p-select [(ngModel)]="form.gender" [options]="genderOptions" optionLabel="label" optionValue="value" fluid />
                            </div>
                            <div class="col-span-6">
                                <label class="block font-bold mb-2">Date de naissance</label>
                                <p-datepicker [(ngModel)]="form.dateOfBirth" dateFormat="dd/mm/yy" fluid />
                            </div>
                        </div>
                        <div>
                            <label class="block font-bold mb-2">Numéro de CIN</label>
                            <input pInputText [(ngModel)]="form.nationalId" fluid />
                        </div>
                        <div>
                            <label class="block font-bold mb-2">Date d'embauche</label>
                            <p-datepicker [(ngModel)]="form.hireDate" dateFormat="dd/mm/yy" fluid />
                        </div>
                    } @else {
                        <div>
                            <label class="block font-bold mb-2">Statut</label>
                            <p-select [(ngModel)]="form.status" [options]="statusOptions" optionLabel="label" optionValue="value" fluid />
                        </div>
                    }

                    <div>
                        <label class="block font-bold mb-2">E-mail personnel</label>
                        <input pInputText [(ngModel)]="form.personalEmail" fluid />
                    </div>
                    <div>
                        <label class="block font-bold mb-2">Téléphone personnel</label>
                        <input pInputText [(ngModel)]="form.personalPhone" fluid />
                    </div>
                </div>
            </ng-template>
            <ng-template #footer>
                <p-button label="Annuler" icon="pi pi-times" text (click)="dialogVisible = false" />
                <p-button label="Enregistrer" icon="pi pi-check" [loading]="saving()" (click)="saveEmployee()" />
            </ng-template>
        </p-dialog>

        <p-toast />
    `
})
export class Employees implements OnInit {
    private readonly employeeService = inject(EmployeeService);
    private readonly establishmentService = inject(EstablishmentService);
    private readonly messageService = inject(MessageService);

    readonly employees = signal<EmployeeSummary[]>([]);
    readonly establishments = signal<EstablishmentOption[]>([]);
    readonly loading = signal(false);
    readonly saving = signal(false);

    readonly genderOptions = GENDER_OPTIONS;
    readonly statusOptions = EMPLOYEE_STATUS_OPTIONS;

    dialogVisible = false;
    editingId: string | null = null;
    form: EmployeeFormModel = emptyForm();

    ngOnInit(): void {
        this.reload();
    }

    statusLabel = employeeStatusLabel;
    statusSeverity = employeeStatusSeverity;

    reload(): void {
        this.loading.set(true);
        this.employeeService.list().subscribe({
            next: (employees) => {
                this.employees.set(employees);
                this.loading.set(false);
            },
            error: () => {
                this.loading.set(false);
                this.messageService.add({ severity: 'error', summary: 'Erreur', detail: 'Impossible de charger les salariés.' });
            }
        });
    }

    openNew(): void {
        this.editingId = null;
        this.form = emptyForm();
        this.dialogVisible = true;

        if (this.establishments().length === 0) {
            this.establishmentService.list().subscribe((establishments) => this.establishments.set(establishments));
        }
    }

    openEdit(employee: EmployeeSummary): void {
        this.editingId = employee.id;
        this.saving.set(true);
        this.employeeService.get(employee.id).subscribe({
            next: (detail: EmployeeDetail) => {
                this.form = {
                    ...emptyForm(),
                    personalEmail: detail.personalEmail ?? '',
                    personalPhone: detail.personalPhone ?? '',
                    status: detail.status
                };
                this.saving.set(false);
                this.dialogVisible = true;
            },
            error: () => {
                this.saving.set(false);
                this.messageService.add({ severity: 'error', summary: 'Erreur', detail: 'Impossible de charger ce dossier.' });
            }
        });
    }

    saveEmployee(): void {
        this.saving.set(true);

        if (this.editingId) {
            this.employeeService
                .update(this.editingId, {
                    departmentId: null,
                    jobPositionId: null,
                    personalEmail: this.form.personalEmail || null,
                    personalPhone: this.form.personalPhone || null,
                    status: this.form.status
                })
                .subscribe({
                    next: () => this.onSaved('Dossier mis à jour.'),
                    error: () => this.onSaveError()
                });
            return;
        }

        if (!this.form.establishmentId || !this.form.dateOfBirth || !this.form.hireDate) {
            this.saving.set(false);
            this.messageService.add({ severity: 'warn', summary: 'Champs manquants', detail: 'Établissement, date de naissance et date d\u2019embauche sont obligatoires.' });
            return;
        }

        this.employeeService
            .create({
                establishmentId: this.form.establishmentId,
                departmentId: null,
                jobPositionId: null,
                firstName: this.form.firstName,
                lastName: this.form.lastName,
                gender: this.form.gender,
                dateOfBirth: toIsoDate(this.form.dateOfBirth),
                nationalId: this.form.nationalId,
                personalEmail: this.form.personalEmail || null,
                personalPhone: this.form.personalPhone || null,
                hireDate: toIsoDate(this.form.hireDate)
            })
            .subscribe({
                next: () => this.onSaved('Salarié créé.'),
                error: () => this.onSaveError()
            });
    }

    private onSaved(message: string): void {
        this.saving.set(false);
        this.dialogVisible = false;
        this.messageService.add({ severity: 'success', summary: 'Enregistré', detail: message });
        this.reload();
    }

    private onSaveError(): void {
        this.saving.set(false);
        this.messageService.add({ severity: 'error', summary: 'Erreur', detail: "L'enregistrement a échoué." });
    }

}

function toIsoDate(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
}
