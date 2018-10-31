import { NgbDate } from '../ngb-date';
import { NgbDateStruct } from '../ngb-date-struct';
export declare function isHebrewLeapYear(year: number): boolean;
/**
 * Returns the number of days in a specific Hebrew month.
 * `month` is 1 for Nisan, 2 for Iyar etc. Note: Hebrew leap year contains 13 months.
 * `year` is any Hebrew year.
 */
export declare function getDaysInHebrewMonth(month: number, year: number): number;
export declare function getDayNumberInHebrewYear(date: NgbDate): number;
export declare function setHebrewMonth(date: NgbDate, val: number): NgbDate;
export declare function setHebrewDay(date: NgbDate, val: number): NgbDate;
/**
 * Returns the equivalent Hebrew date value for a give input Gregorian date.
 * `gdate` is a JS Date to be converted to Hebrew date.
 */
export declare function fromGregorian(gdate: Date): NgbDate;
/**
 * Returns the equivalent JS date value for a given Hebrew date.
 * `hebrewDate` is an Hebrew date to be converted to Gregorian.
 */
export declare function toGregorian(hebrewDate: NgbDateStruct | NgbDate): Date;
export declare function hebrewNumerals(numerals: number): string;
