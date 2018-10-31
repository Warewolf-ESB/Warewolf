import { NgbCalendarIslamicCivil } from './ngb-calendar-islamic-civil';
import { NgbDate } from '../ngb-date';
export declare class NgbCalendarIslamicUmalqura extends NgbCalendarIslamicCivil {
    /**
    * Returns the equivalent islamic(Umalqura) date value for a give input Gregorian date.
    * `gdate` is s JS Date to be converted to Hijri.
    */
    fromGregorian(gDate: Date): NgbDate;
    /**
    * Converts the current Hijri date to Gregorian.
    */
    toGregorian(hDate: NgbDate): Date;
    /**
    * Returns the number of days in a specific Hijri hMonth.
    * `hMonth` is 1 for Muharram, 2 for Safar, etc.
    * `hYear` is any Hijri hYear.
    */
    getDaysPerMonth(hMonth: number, hYear: number): number;
}
