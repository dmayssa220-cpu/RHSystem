import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { AuthService } from '@/app/core/auth/auth.service';

@Component({
    selector: 'app-login',
    standalone: true,
    imports: [CommonModule, FormsModule, ButtonModule, InputTextModule, PasswordModule],
    template: `
        <div class="bg-surface-50 dark:bg-surface-950 flex items-center justify-center min-h-screen min-w-screen">
            <div style="border-radius: 24px; padding: 0.3rem; background: linear-gradient(180deg, var(--primary-color) 10%, rgba(33, 150, 243, 0) 30%)">
                <div class="w-full bg-surface-0 dark:bg-surface-900 py-12 px-8 sm:px-12" style="border-radius: 21px; min-width: 24rem">
                    <div class="text-center mb-8">
                        <div class="text-3xl font-semibold text-primary mb-2">SIRH</div>
                        <span class="text-muted-color font-medium">Connexion</span>
                    </div>

                    <form (ngSubmit)="submit()">
                        <label for="email" class="block font-medium mb-2">Adresse e-mail</label>
                        <input pInputText id="email" type="email" name="email" class="w-full mb-6" [(ngModel)]="email" autocomplete="username" required />

                        <label for="password" class="block font-medium mb-2">Mot de passe</label>
                        <p-password id="password" name="password" [(ngModel)]="password" [toggleMask]="true" [feedback]="false" styleClass="mb-6" inputStyleClass="w-full" autocomplete="current-password" />

                        @if (requiresMfa()) {
                            <label for="mfa" class="block font-medium mb-2">Code d'authentification</label>
                            <input pInputText id="mfa" type="text" name="mfa" inputmode="numeric" class="w-full mb-6" [(ngModel)]="mfaCode" />
                        }

                        @if (errorMessage()) {
                            <div class="text-red-500 text-sm mb-4">{{ errorMessage() }}</div>
                        }

                        <p-button type="submit" label="Se connecter" styleClass="w-full" [loading]="loading()" />
                    </form>
                </div>
            </div>
        </div>
    `
})
export class Login {
    private readonly auth = inject(AuthService);
    private readonly router = inject(Router);

    email = '';
    password = '';
    mfaCode = '';

    readonly requiresMfa = signal(false);
    readonly loading = signal(false);
    readonly errorMessage = signal<string | null>(null);

    submit(): void {
        this.loading.set(true);
        this.errorMessage.set(null);

        this.auth.login(this.email, this.password, this.mfaCode || undefined).subscribe({
            next: (response) => {
                this.loading.set(false);
                if (response.requiresMfa) {
                    this.requiresMfa.set(true);
                    return;
                }
                this.router.navigateByUrl('/');
            },
            error: () => {
                this.loading.set(false);
                this.errorMessage.set("Identifiants invalides, ou code d'authentification incorrect.");
            }
        });
    }
}
