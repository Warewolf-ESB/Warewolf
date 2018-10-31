import { NgbDatepickerI18n } from '../datepicker-i18n';
import { NgbDateStruct } from '../../index';
/**
 * @since 3.2.0
 */
export declare class NgbDatepickerI18nHebrew extends NgbDatepickerI18n {
    getMonthShortName(month: number, year?: number): string;
    getMonthFullName(month: number, year?: number): string;
    getWeekdayShortName(weekday: number): string;
    getDayAriaLabel(date: NgbDateStruct): string;
    getDayNumerals(date: NgbDateStruct): string;
    getWeekNumerals(weekNumber: number): string;
    getYearNumerals(year: number): string;
}
