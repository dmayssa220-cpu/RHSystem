/**
 * Source unique des modules fonctionnels du SIRH.
 * Utilisée par le menu latéral et par le routeur : ajouter un module ici suffit
 * à le voir apparaître (avec une page d'attente) dans l'application.
 */
export interface AppModule {
    path: string;
    label: string;
    icon: string;
    description: string;
}

export const APP_MODULES: AppModule[] = [
    {
        path: 'personnel',
        label: 'Personnel',
        icon: 'pi pi-users',
        description: 'Dossiers salariés, contrats, organisation et documents.'
    },
    {
        path: 'temps',
        label: 'Temps et absences',
        icon: 'pi pi-calendar',
        description: 'Congés, absences, heures supplémentaires et calendriers de travail.'
    },
    {
        path: 'paie',
        label: 'Paie',
        icon: 'pi pi-wallet',
        description: 'Variables, calcul, bulletins de paie et trace de calcul.'
    },
    {
        path: 'declarations',
        label: 'Déclarations sociales',
        icon: 'pi pi-file',
        description: 'Déclarations CNSS, retenue à la source et états réglementaires.'
    },
    {
        path: 'talents',
        label: 'Talents',
        icon: 'pi pi-star',
        description: 'Recrutement, compétences, entretiens et formation.'
    },
    {
        path: 'conformite',
        label: 'Conformité et anomalies',
        icon: 'pi pi-shield',
        description: 'Contrôles de conformité, alertes et paie explicable.'
    },
    {
        path: 'assistant-ia',
        label: 'Assistant IA',
        icon: 'pi pi-comments',
        description: 'Assistant conversationnel et agents d’automatisation.'
    },
    {
        path: 'administration',
        label: 'Administration',
        icon: 'pi pi-cog',
        description: 'Sociétés, utilisateurs, rôles, paramètres et journal d’audit.'
    }
];
