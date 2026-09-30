import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

/**
 * Protège les écrans connectés. Si le jeton d'accès (en mémoire) est absent — typiquement
 * après un rechargement de page —, tente d'abord de restaurer la session grâce au cookie de
 * rafraîchissement avant de renvoyer vers /login.
 */
export const authGuard: CanActivateFn = async () => {
    const auth = inject(AuthService);
    const router = inject(Router);

    if (!auth.isAuthenticated()) {
        await auth.tryRestoreSession();
    }

    return auth.isAuthenticated() ? true : router.createUrlTree(['/login']);
};
