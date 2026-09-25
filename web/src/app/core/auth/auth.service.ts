import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, catchError, firstValueFrom, of, tap } from 'rxjs';
import { AuthenticatedUser, LoginResponse } from './auth.models';

/**
 * Session côté navigateur. Le jeton d'accès ne vit qu'en mémoire (jamais dans le stockage
 * du navigateur) ; le jeton de rafraîchissement est un cookie HttpOnly que le navigateur
 * envoie tout seul, l'application ne le voit jamais.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
    private readonly http = inject(HttpClient);
    private readonly router = inject(Router);

    private readonly accessTokenSignal = signal<string | null>(null);
    private readonly userSignal = signal<AuthenticatedUser | null>(null);

    readonly isAuthenticated = computed(() => this.accessTokenSignal() !== null);
    readonly user = this.userSignal.asReadonly();

    get accessToken(): string | null {
        return this.accessTokenSignal();
    }

    login(email: string, password: string, mfaCode?: string): Observable<LoginResponse> {
        return this.http.post<LoginResponse>('/api/auth/connexion', { email, password, mfaCode: mfaCode ?? null }, { withCredentials: true }).pipe(tap((response) => this.applySession(response)));
    }

    refresh(): Observable<LoginResponse> {
        return this.http.post<LoginResponse>('/api/auth/rafraichir', {}, { withCredentials: true }).pipe(tap((response) => this.applySession(response)));
    }

    logout(): void {
        this.http.post('/api/auth/deconnexion', {}, { withCredentials: true }).subscribe({
            complete: () => this.clearSession(true),
            error: () => this.clearSession(true)
        });
    }

    /** Tenté une seule fois au chargement de l'application : restaure la session si le cookie de rafraîchissement est encore valide. */
    async tryRestoreSession(): Promise<void> {
        await firstValueFrom(
            this.refresh().pipe(
                catchError(() => {
                    this.clearSession(false);
                    return of(null);
                })
            )
        );
    }

    /** Utilisé par l'intercepteur quand un rafraîchissement échoue en cours de navigation : la session est morte, inutile de rappeler l'API de déconnexion. */
    forceLogout(): void {
        this.clearSession(true);
    }

    private applySession(response: LoginResponse): void {
        if (response.accessToken) {
            this.accessTokenSignal.set(response.accessToken);
            this.userSignal.set(response.user ?? null);
        }
    }

    private clearSession(redirect: boolean): void {
        this.accessTokenSignal.set(null);
        this.userSignal.set(null);
        if (redirect) {
            this.router.navigateByUrl('/login');
        }
    }
}
