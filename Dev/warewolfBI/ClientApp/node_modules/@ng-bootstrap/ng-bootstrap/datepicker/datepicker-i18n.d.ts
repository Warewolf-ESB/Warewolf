import { NgbDateStruct } from './ngb-date-struct';
export declare function NGB_DATEPICKER_18N_FACTORY(locale: any): NgbDatepickerI18nDefault;
/**
 * Type of the service supplying month and weekday names to to NgbDatepicker component.
 * The default implementation of this service honors the Angular locale, and uses the registered locale data,
 * as explained in the Angular i18n guide.
 * See the i18n demo for how to extend this class and define a custom provider for i18n.
 */
export declare abstract class NgbDatepickerI18n {
    /**
     * Returns the short weekday name to display in the heading of the month view.
     * With default calendar we use ISO 8601: 'weekday' is 1=Mon ... 7=Sun
     */
    abstract getWeekdayShortName(weekday: number): string;
    /**
     * Returns the short month name to display in the date picker navigation.
     * With default calendar we use ISO 8601: 'month' is 1=Jan ... 12=Dec
     */
    abstract getMonthShortName(month: number, year?: number): string;
    /**
     * Returns the full month name to display in the date picker navigation.
     * With default calendar we use ISO 8601: 'month' is 1=January ... 12=December
     */
    abstract getMonthFullName(month: number, year?: number): string;
    /**
     * Returns the value of the 'aria-label' attribute for a specific date
     *
     * @since 2.0.0
     */
    abstract getDayAriaLabel(date: NgbDateStruct): string;
    /**
     * Returns the textual representation of a day that is rendered in a day cell
     *
     * @since 3.0.0
     */
    getDayNumerals(date: NgbDateStruct): string;
    /**
     * Returns the textual representation of a week number rendered by date picker
     *
     * @since 3.0.0
     */
    getWeekNumerals(weekNumber: number): string;
    /**
     * Returns the textual representation of a year that is rendered
     * in date picker year select box
     *
     * @since 3.0.0
     */
    getYearNumerals(year: number): string;
}
export declare class NgbDatepickerI18nDefault extends NgbDatepickerI18n {
    private _locale;
    private _weekdaysShort;
    private _monthsShort;
    private _monthsFull;
    constructor(_locale: string);
    getWeekdayShortName(weekday: number): string;
    getMonthShortName(month: number): string;
    getMonthFullName(month: number): string;
    getDayAriaLabel(date: NgbDateStruct): string;
}
