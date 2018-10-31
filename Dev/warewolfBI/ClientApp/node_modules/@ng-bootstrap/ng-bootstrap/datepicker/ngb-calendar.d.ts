import { NgbDate } from './ngb-date';
export declare type NgbPeriod = 'y' | 'm' | 'd';
export declare function NGB_DATEPICKER_CALENDAR_FACTORY(): NgbCalendarGregorian;
/**
 * Calendar used by the datepicker.
 * Default implementation uses Gregorian calendar.
 */
export declare abstract class NgbCalendar {
    /**
     * Returns number of days per week.
     */
    abstract getDaysPerWeek(): number;
    /**
     * Returns an array of months per year.
     * With default calendar we use ISO 8601 and return [1, 2, ..., 12];
     */
    abstract getMonths(year?: number): number[];
    /**
     * Returns number of weeks per month.
     */
    abstract getWeeksPerMonth(): number;
    /**
     * Returns weekday number for a given day.
     * With default calendar we use ISO 8601: 'weekday' is 1=Mon ... 7=Sun
     */
    abstract getWeekday(date: NgbDate): number;
    /**
     * Adds a number of years, months or days to a given date.
     * Period can be 'y', 'm' or 'd' and defaults to day.
     * Number defaults to 1.
     */
    abstract getNext(date: NgbDate, period?: NgbPeriod, number?: number): NgbDate;
    /**
     * Subtracts a number of years, months or days from a given date.
     * Period can be 'y', 'm' or 'd' and defaults to day.
     * Number defaults to 1.
     */
    abstract getPrev(date: NgbDate, period?: NgbPeriod, number?: number): NgbDate;
    /**
     * Returns week number for a given week.
     */
    abstract getWeekNumber(week: NgbDate[], firstDayOfWeek: number): number;
    /**
     * Returns today's date.
     */
    abstract getToday(): NgbDate;
    /**
     * Checks if a date is valid for a current calendar.
     */
    abstract isValid(date: NgbDate): boolean;
}
export declare class NgbCalendarGregorian extends NgbCalendar {
    getDaysPerWeek(): number;
    getMonths(): number[];
    getWeeksPerMonth(): number;
    getNext(date: NgbDate, period?: NgbPeriod, number?: number): NgbDate;
    getPrev(date: NgbDate, period?: NgbPeriod, number?: number): NgbDate;
    getWeekday(date: NgbDate): number;
    getWeekNumber(week: NgbDate[], firstDayOfWeek: number): number;
    getToday(): NgbDate;
    isValid(date: NgbDate): boolean;
}
