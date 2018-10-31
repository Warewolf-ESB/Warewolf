import { EventEmitter } from '@angular/core';
import { NgbDate } from './ngb-date';
import { NgbDatepickerI18n } from './datepicker-i18n';
export declare class NgbDatepickerNavigationSelect {
    i18n: NgbDatepickerI18n;
    date: NgbDate;
    disabled: boolean;
    months: number[];
    years: number[];
    select: EventEmitter<NgbDate>;
    constructor(i18n: NgbDatepickerI18n);
    changeMonth(month: string): void;
    changeYear(year: string): void;
}
