import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = async () => {
    const auth = inject(AuthService);
    if (!auth.isAuthenticated()) {
        await auth.tryRestoreSession();
    }

    return auth.isAuthenticated() ? true : inject(Router).createUrlTree(['/login']);
};
