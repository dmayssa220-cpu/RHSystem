import { CurrencyPipe } from '@angular/common';
import { LOCALE_ID, Pipe, PipeTransform, inject } from '@angular/core';

/**
 * Affiche un montant en dinar tunisien avec 3 décimales (millimes).
 * Exemple : {{ 1234.5 | tnd }}  →  « 1 234,500 DT » (selon la locale fr-TN).
 */
@Pipe({ name: 'tnd', standalone: true })
export class TndPipe implements PipeTransform {
    private readonly currency = new CurrencyPipe(inject(LOCALE_ID));

    transform(value: number | string | null | undefined): string | null {
        if (value === null || value === undefined || value === '') {
            return null;
        }
        return this.currency.transform(value, 'TND', 'symbol', '1.3-3');
    }
}
