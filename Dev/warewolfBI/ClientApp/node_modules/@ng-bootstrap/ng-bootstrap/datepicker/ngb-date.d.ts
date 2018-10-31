import { NgbDateStruct } from './ngb-date-struct';
/**
 * Simple class used for a date representation that datepicker also uses internally
 *
 * @since 3.0.0
 */
export declare class NgbDate implements NgbDateStruct {
    /**
     * The year, for example 2016
     */
    year: number;
    /**
     * The month, for example 1=Jan ... 12=Dec as in ISO 8601
     */
    month: number;
    /**
     * The day of month, starting with 1
     */
    day: number;
    /**
     * Static method. Creates a new date object from the NgbDateStruct, ex. NgbDate.from({year: 2000,
     * month: 5, day: 1}). If the 'date' is already of NgbDate, the method will return the same object
     */
    static from(date: NgbDateStruct): NgbDate;
    constructor(year: number, month: number, day: number);
    /**
     * Checks if current date is equal to another date
     */
    equals(other: NgbDate): boolean;
    /**
     * Checks if current date is before another date
     */
    before(other: NgbDate): boolean;
    /**
     * Checks if current date is after another date
     */
    after(other: NgbDate): boolean;
}
