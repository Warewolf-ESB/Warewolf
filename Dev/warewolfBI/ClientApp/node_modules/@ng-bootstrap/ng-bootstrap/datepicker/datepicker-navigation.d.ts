import { EventEmitter } from '@angular/core';
import { NavigationEvent, MonthViewModel } from './datepicker-view-model';
import { NgbDate } from './ngb-date';
import { NgbDatepickerI18n } from './datepicker-i18n';
export declare class NgbDatepickerNavigation {
    i18n: NgbDatepickerI18n;
    navigation: typeof NavigationEvent;
    date: NgbDate;
    disabled: boolean;
    months: MonthViewModel[];
    showSelect: boolean;
    prevDisabled: boolean;
    nextDisabled: boolean;
    selectBoxes: {
        years: number[];
        months: number[];
    };
    navigate: EventEmitter<NavigationEvent>;
    select: EventEmitter<NgbDate>;
    constructor(i18n: NgbDatepickerI18n);
}
