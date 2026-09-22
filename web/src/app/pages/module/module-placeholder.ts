import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

/** Page d'attente affichée pour un module pas encore développé. */
@Component({
    selector: 'app-module-placeholder',
    standalone: true,
    template: `
        <div class="card">
            <div class="flex items-center gap-3 mb-4">
                <i [class]="icon + ' text-3xl text-primary'"></i>
                <h1 class="text-2xl font-semibold m-0">{{ title }}</h1>
            </div>
            <p class="mb-2">{{ description }}</p>
            <p class="text-muted-color m-0">Ce module est en cours de développement.</p>
        </div>
    `
})
export class ModulePlaceholder {
    private readonly data = inject(ActivatedRoute).snapshot.data;
    readonly title: string = this.data['title'];
    readonly icon: string = this.data['icon'];
    readonly description: string = this.data['description'];
}
