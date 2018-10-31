import { NgbDate } from '../ngb-date';
/**
 * Returns the equivalent JS date value for a give input Jalali date.
 * `jalaliDate` is an Jalali date to be converted to Gregorian.
 */
export declare function toGregorian(jalaliDate: NgbDate): Date;
/**
 * Returns the equivalent jalali date value for a give input Gregorian date.
 * `gdate` is a JS Date to be converted to jalali.
 * utc to local
 */
export declare function fromGregorian(gdate: Date): NgbDate;
export declare function setJalaliYear(date: NgbDate, yearValue: number): NgbDate;
export declare function setJalaliMonth(date: NgbDate, month: number): NgbDate;
export declare function setJalaliDay(date: NgbDate, day: number): NgbDate;
