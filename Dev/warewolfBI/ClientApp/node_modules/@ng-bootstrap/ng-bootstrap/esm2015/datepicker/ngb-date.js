/**
 * @fileoverview added by tsickle
 * @suppress {checkTypes,extraRequire,uselessCode} checked by tsc
 */
import { isInteger } from '../util/util';
/**
 * Simple class used for a date representation that datepicker also uses internally
 *
 * \@since 3.0.0
 */
export class NgbDate {
    /**
     * Static method. Creates a new date object from the NgbDateStruct, ex. NgbDate.from({year: 2000,
     * month: 5, day: 1}). If the 'date' is already of NgbDate, the method will return the same object
     * @param {?} date
     * @return {?}
     */
    static from(date) {
        if (date instanceof NgbDate) {
            return date;
        }
        return date ? new NgbDate(date.year, date.month, date.day) : null;
    }
    /**
     * @param {?} year
     * @param {?} month
     * @param {?} day
     */
    constructor(year, month, day) {
        this.year = isInteger(year) ? year : null;
        this.month = isInteger(month) ? month : null;
        this.day = isInteger(day) ? day : null;
    }
    /**
     * Checks if current date is equal to another date
     * @param {?} other
     * @return {?}
     */
    equals(other) {
        return other && this.year === other.year && this.month === other.month && this.day === other.day;
    }
    /**
     * Checks if current date is before another date
     * @param {?} other
     * @return {?}
     */
    before(other) {
        if (!other) {
            return false;
        }
        if (this.year === other.year) {
            if (this.month === other.month) {
                return this.day === other.day ? false : this.day < other.day;
            }
            else {
                return this.month < other.month;
            }
        }
        else {
            return this.year < other.year;
        }
    }
    /**
     * Checks if current date is after another date
     * @param {?} other
     * @return {?}
     */
    after(other) {
        if (!other) {
            return false;
        }
        if (this.year === other.year) {
            if (this.month === other.month) {
                return this.day === other.day ? false : this.day > other.day;
            }
            else {
                return this.month > other.month;
            }
        }
        else {
            return this.year > other.year;
        }
    }
}
if (false) {
    /**
     * The year, for example 2016
     * @type {?}
     */
    NgbDate.prototype.year;
    /**
     * The month, for example 1=Jan ... 12=Dec as in ISO 8601
     * @type {?}
     */
    NgbDate.prototype.month;
    /**
     * The day of month, starting with 1
     * @type {?}
     */
    NgbDate.prototype.day;
}

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoibmdiLWRhdGUuanMiLCJzb3VyY2VSb290Ijoibmc6Ly9AbmctYm9vdHN0cmFwL25nLWJvb3RzdHJhcC8iLCJzb3VyY2VzIjpbImRhdGVwaWNrZXIvbmdiLWRhdGUudHMiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6Ijs7OztBQUNBLE9BQU8sRUFBQyxTQUFTLEVBQUMsTUFBTSxjQUFjLENBQUM7Ozs7OztBQU92QyxNQUFNOzs7Ozs7O0lBb0JKLE1BQU0sQ0FBQyxJQUFJLENBQUMsSUFBbUI7UUFDN0IsRUFBRSxDQUFDLENBQUMsSUFBSSxZQUFZLE9BQU8sQ0FBQyxDQUFDLENBQUM7WUFDNUIsTUFBTSxDQUFDLElBQUksQ0FBQztTQUNiO1FBQ0QsTUFBTSxDQUFDLElBQUksQ0FBQyxDQUFDLENBQUMsSUFBSSxPQUFPLENBQUMsSUFBSSxDQUFDLElBQUksRUFBRSxJQUFJLENBQUMsS0FBSyxFQUFFLElBQUksQ0FBQyxHQUFHLENBQUMsQ0FBQyxDQUFDLENBQUMsSUFBSSxDQUFDO0tBQ25FOzs7Ozs7SUFFRCxZQUFZLElBQVksRUFBRSxLQUFhLEVBQUUsR0FBVztRQUNsRCxJQUFJLENBQUMsSUFBSSxHQUFHLFNBQVMsQ0FBQyxJQUFJLENBQUMsQ0FBQyxDQUFDLENBQUMsSUFBSSxDQUFDLENBQUMsQ0FBQyxJQUFJLENBQUM7UUFDMUMsSUFBSSxDQUFDLEtBQUssR0FBRyxTQUFTLENBQUMsS0FBSyxDQUFDLENBQUMsQ0FBQyxDQUFDLEtBQUssQ0FBQyxDQUFDLENBQUMsSUFBSSxDQUFDO1FBQzdDLElBQUksQ0FBQyxHQUFHLEdBQUcsU0FBUyxDQUFDLEdBQUcsQ0FBQyxDQUFDLENBQUMsQ0FBQyxHQUFHLENBQUMsQ0FBQyxDQUFDLElBQUksQ0FBQztLQUN4Qzs7Ozs7O0lBS0QsTUFBTSxDQUFDLEtBQWM7UUFDbkIsTUFBTSxDQUFDLEtBQUssSUFBSSxJQUFJLENBQUMsSUFBSSxLQUFLLEtBQUssQ0FBQyxJQUFJLElBQUksSUFBSSxDQUFDLEtBQUssS0FBSyxLQUFLLENBQUMsS0FBSyxJQUFJLElBQUksQ0FBQyxHQUFHLEtBQUssS0FBSyxDQUFDLEdBQUcsQ0FBQztLQUNsRzs7Ozs7O0lBS0QsTUFBTSxDQUFDLEtBQWM7UUFDbkIsRUFBRSxDQUFDLENBQUMsQ0FBQyxLQUFLLENBQUMsQ0FBQyxDQUFDO1lBQ1gsTUFBTSxDQUFDLEtBQUssQ0FBQztTQUNkO1FBRUQsRUFBRSxDQUFDLENBQUMsSUFBSSxDQUFDLElBQUksS0FBSyxLQUFLLENBQUMsSUFBSSxDQUFDLENBQUMsQ0FBQztZQUM3QixFQUFFLENBQUMsQ0FBQyxJQUFJLENBQUMsS0FBSyxLQUFLLEtBQUssQ0FBQyxLQUFLLENBQUMsQ0FBQyxDQUFDO2dCQUMvQixNQUFNLENBQUMsSUFBSSxDQUFDLEdBQUcsS0FBSyxLQUFLLENBQUMsR0FBRyxDQUFDLENBQUMsQ0FBQyxLQUFLLENBQUMsQ0FBQyxDQUFDLElBQUksQ0FBQyxHQUFHLEdBQUcsS0FBSyxDQUFDLEdBQUcsQ0FBQzthQUM5RDtZQUFDLElBQUksQ0FBQyxDQUFDO2dCQUNOLE1BQU0sQ0FBQyxJQUFJLENBQUMsS0FBSyxHQUFHLEtBQUssQ0FBQyxLQUFLLENBQUM7YUFDakM7U0FDRjtRQUFDLElBQUksQ0FBQyxDQUFDO1lBQ04sTUFBTSxDQUFDLElBQUksQ0FBQyxJQUFJLEdBQUcsS0FBSyxDQUFDLElBQUksQ0FBQztTQUMvQjtLQUNGOzs7Ozs7SUFLRCxLQUFLLENBQUMsS0FBYztRQUNsQixFQUFFLENBQUMsQ0FBQyxDQUFDLEtBQUssQ0FBQyxDQUFDLENBQUM7WUFDWCxNQUFNLENBQUMsS0FBSyxDQUFDO1NBQ2Q7UUFDRCxFQUFFLENBQUMsQ0FBQyxJQUFJLENBQUMsSUFBSSxLQUFLLEtBQUssQ0FBQyxJQUFJLENBQUMsQ0FBQyxDQUFDO1lBQzdCLEVBQUUsQ0FBQyxDQUFDLElBQUksQ0FBQyxLQUFLLEtBQUssS0FBSyxDQUFDLEtBQUssQ0FBQyxDQUFDLENBQUM7Z0JBQy9CLE1BQU0sQ0FBQyxJQUFJLENBQUMsR0FBRyxLQUFLLEtBQUssQ0FBQyxHQUFHLENBQUMsQ0FBQyxDQUFDLEtBQUssQ0FBQyxDQUFDLENBQUMsSUFBSSxDQUFDLEdBQUcsR0FBRyxLQUFLLENBQUMsR0FBRyxDQUFDO2FBQzlEO1lBQUMsSUFBSSxDQUFDLENBQUM7Z0JBQ04sTUFBTSxDQUFDLElBQUksQ0FBQyxLQUFLLEdBQUcsS0FBSyxDQUFDLEtBQUssQ0FBQzthQUNqQztTQUNGO1FBQUMsSUFBSSxDQUFDLENBQUM7WUFDTixNQUFNLENBQUMsSUFBSSxDQUFDLElBQUksR0FBRyxLQUFLLENBQUMsSUFBSSxDQUFDO1NBQy9CO0tBQ0Y7Q0FDRiIsInNvdXJjZXNDb250ZW50IjpbImltcG9ydCB7TmdiRGF0ZVN0cnVjdH0gZnJvbSAnLi9uZ2ItZGF0ZS1zdHJ1Y3QnO1xuaW1wb3J0IHtpc0ludGVnZXJ9IGZyb20gJy4uL3V0aWwvdXRpbCc7XG5cbi8qKlxuICogU2ltcGxlIGNsYXNzIHVzZWQgZm9yIGEgZGF0ZSByZXByZXNlbnRhdGlvbiB0aGF0IGRhdGVwaWNrZXIgYWxzbyB1c2VzIGludGVybmFsbHlcbiAqXG4gKiBAc2luY2UgMy4wLjBcbiAqL1xuZXhwb3J0IGNsYXNzIE5nYkRhdGUgaW1wbGVtZW50cyBOZ2JEYXRlU3RydWN0IHtcbiAgLyoqXG4gICAqIFRoZSB5ZWFyLCBmb3IgZXhhbXBsZSAyMDE2XG4gICAqL1xuICB5ZWFyOiBudW1iZXI7XG5cbiAgLyoqXG4gICAqIFRoZSBtb250aCwgZm9yIGV4YW1wbGUgMT1KYW4gLi4uIDEyPURlYyBhcyBpbiBJU08gODYwMVxuICAgKi9cbiAgbW9udGg6IG51bWJlcjtcblxuICAvKipcbiAgICogVGhlIGRheSBvZiBtb250aCwgc3RhcnRpbmcgd2l0aCAxXG4gICAqL1xuICBkYXk6IG51bWJlcjtcblxuICAvKipcbiAgICogU3RhdGljIG1ldGhvZC4gQ3JlYXRlcyBhIG5ldyBkYXRlIG9iamVjdCBmcm9tIHRoZSBOZ2JEYXRlU3RydWN0LCBleC4gTmdiRGF0ZS5mcm9tKHt5ZWFyOiAyMDAwLFxuICAgKiBtb250aDogNSwgZGF5OiAxfSkuIElmIHRoZSAnZGF0ZScgaXMgYWxyZWFkeSBvZiBOZ2JEYXRlLCB0aGUgbWV0aG9kIHdpbGwgcmV0dXJuIHRoZSBzYW1lIG9iamVjdFxuICAgKi9cbiAgc3RhdGljIGZyb20oZGF0ZTogTmdiRGF0ZVN0cnVjdCk6IE5nYkRhdGUge1xuICAgIGlmIChkYXRlIGluc3RhbmNlb2YgTmdiRGF0ZSkge1xuICAgICAgcmV0dXJuIGRhdGU7XG4gICAgfVxuICAgIHJldHVybiBkYXRlID8gbmV3IE5nYkRhdGUoZGF0ZS55ZWFyLCBkYXRlLm1vbnRoLCBkYXRlLmRheSkgOiBudWxsO1xuICB9XG5cbiAgY29uc3RydWN0b3IoeWVhcjogbnVtYmVyLCBtb250aDogbnVtYmVyLCBkYXk6IG51bWJlcikge1xuICAgIHRoaXMueWVhciA9IGlzSW50ZWdlcih5ZWFyKSA/IHllYXIgOiBudWxsO1xuICAgIHRoaXMubW9udGggPSBpc0ludGVnZXIobW9udGgpID8gbW9udGggOiBudWxsO1xuICAgIHRoaXMuZGF5ID0gaXNJbnRlZ2VyKGRheSkgPyBkYXkgOiBudWxsO1xuICB9XG5cbiAgLyoqXG4gICAqIENoZWNrcyBpZiBjdXJyZW50IGRhdGUgaXMgZXF1YWwgdG8gYW5vdGhlciBkYXRlXG4gICAqL1xuICBlcXVhbHMob3RoZXI6IE5nYkRhdGUpOiBib29sZWFuIHtcbiAgICByZXR1cm4gb3RoZXIgJiYgdGhpcy55ZWFyID09PSBvdGhlci55ZWFyICYmIHRoaXMubW9udGggPT09IG90aGVyLm1vbnRoICYmIHRoaXMuZGF5ID09PSBvdGhlci5kYXk7XG4gIH1cblxuICAvKipcbiAgICogQ2hlY2tzIGlmIGN1cnJlbnQgZGF0ZSBpcyBiZWZvcmUgYW5vdGhlciBkYXRlXG4gICAqL1xuICBiZWZvcmUob3RoZXI6IE5nYkRhdGUpOiBib29sZWFuIHtcbiAgICBpZiAoIW90aGVyKSB7XG4gICAgICByZXR1cm4gZmFsc2U7XG4gICAgfVxuXG4gICAgaWYgKHRoaXMueWVhciA9PT0gb3RoZXIueWVhcikge1xuICAgICAgaWYgKHRoaXMubW9udGggPT09IG90aGVyLm1vbnRoKSB7XG4gICAgICAgIHJldHVybiB0aGlzLmRheSA9PT0gb3RoZXIuZGF5ID8gZmFsc2UgOiB0aGlzLmRheSA8IG90aGVyLmRheTtcbiAgICAgIH0gZWxzZSB7XG4gICAgICAgIHJldHVybiB0aGlzLm1vbnRoIDwgb3RoZXIubW9udGg7XG4gICAgICB9XG4gICAgfSBlbHNlIHtcbiAgICAgIHJldHVybiB0aGlzLnllYXIgPCBvdGhlci55ZWFyO1xuICAgIH1cbiAgfVxuXG4gIC8qKlxuICAgKiBDaGVja3MgaWYgY3VycmVudCBkYXRlIGlzIGFmdGVyIGFub3RoZXIgZGF0ZVxuICAgKi9cbiAgYWZ0ZXIob3RoZXI6IE5nYkRhdGUpOiBib29sZWFuIHtcbiAgICBpZiAoIW90aGVyKSB7XG4gICAgICByZXR1cm4gZmFsc2U7XG4gICAgfVxuICAgIGlmICh0aGlzLnllYXIgPT09IG90aGVyLnllYXIpIHtcbiAgICAgIGlmICh0aGlzLm1vbnRoID09PSBvdGhlci5tb250aCkge1xuICAgICAgICByZXR1cm4gdGhpcy5kYXkgPT09IG90aGVyLmRheSA/IGZhbHNlIDogdGhpcy5kYXkgPiBvdGhlci5kYXk7XG4gICAgICB9IGVsc2Uge1xuICAgICAgICByZXR1cm4gdGhpcy5tb250aCA+IG90aGVyLm1vbnRoO1xuICAgICAgfVxuICAgIH0gZWxzZSB7XG4gICAgICByZXR1cm4gdGhpcy55ZWFyID4gb3RoZXIueWVhcjtcbiAgICB9XG4gIH1cbn1cbiJdfQ==