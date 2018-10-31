import { NgbDate } from './ngb-date';
import { NgbDatepickerI18n } from './datepicker-i18n';
export declare class NgbDatepickerDayView {
    i18n: NgbDatepickerI18n;
    currentMonth: number;
    date: NgbDate;
    disabled: boolean;
    focused: boolean;
    selected: boolean;
    constructor(i18n: NgbDatepickerI18n);
    isMuted(): boolean;
}
