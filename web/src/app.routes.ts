import { Routes } from '@angular/router';
import { AppLayout } from './app/layout/component/app.layout';
import { Dashboard } from './app/pages/dashboard/dashboard';
import { Documentation } from './app/pages/documentation/documentation';
import { Landing } from './app/pages/landing/landing';
import { Notfound } from './app/pages/notfound/notfound';
import { APP_MODULES } from './app/core/modules';
import { ModulePlaceholder } from './app/pages/module/module-placeholder';
import { authGuard } from './app/core/auth/auth.guard';
import { Login } from './app/pages/auth/login';
import { Employees } from './app/pages/personnel/employees';

export const appRoutes: Routes = [
    { path: 'login', component: Login },
    {
        path: '',
        component: AppLayout,
        canActivate: [authGuard],
        children: [
            { path: '', component: Dashboard },
            { path: 'personnel', component: Employees },
            ...APP_MODULES.filter((m) => m.path !== 'personnel').map((m) => ({
                path: m.path,
                component: ModulePlaceholder,
                data: { title: m.label, icon: m.icon, description: m.description }
            })),
            { path: 'uikit', loadChildren: () => import('./app/pages/uikit/uikit.routes') },
            { path: 'documentation', component: Documentation },
            { path: 'pages', loadChildren: () => import('./app/pages/pages.routes') }
        ]
    },
    { path: 'landing', component: Landing },
    { path: 'notfound', component: Notfound },
    { path: 'auth', loadChildren: () => import('./app/pages/auth/auth.routes') },
    { path: '**', redirectTo: '/notfound' }
];
