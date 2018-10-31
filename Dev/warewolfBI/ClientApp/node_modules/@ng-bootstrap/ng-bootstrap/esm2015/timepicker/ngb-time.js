/**
 * @fileoverview added by tsickle
 * @suppress {checkTypes,extraRequire,uselessCode} checked by tsc
 */
import { isNumber, toInteger } from '../util/util';
export class NgbTime {
    /**
     * @param {?=} hour
     * @param {?=} minute
     * @param {?=} second
     */
    constructor(hour, minute, second) {
        this.hour = toInteger(hour);
        this.minute = toInteger(minute);
        this.second = toInteger(second);
    }
    /**
     * @param {?=} step
     * @return {?}
     */
    changeHour(step = 1) { this.updateHour((isNaN(this.hour) ? 0 : this.hour) + step); }
    /**
     * @param {?} hour
     * @return {?}
     */
    updateHour(hour) {
        if (isNumber(hour)) {
            this.hour = (hour < 0 ? 24 + hour : hour) % 24;
        }
        else {
            this.hour = NaN;
        }
    }
    /**
     * @param {?=} step
     * @return {?}
     */
    changeMinute(step = 1) { this.updateMinute((isNaN(this.minute) ? 0 : this.minute) + step); }
    /**
     * @param {?} minute
     * @return {?}
     */
    updateMinute(minute) {
        if (isNumber(minute)) {
            this.minute = minute % 60 < 0 ? 60 + minute % 60 : minute % 60;
            this.changeHour(Math.floor(minute / 60));
        }
        else {
            this.minute = NaN;
        }
    }
    /**
     * @param {?=} step
     * @return {?}
     */
    changeSecond(step = 1) { this.updateSecond((isNaN(this.second) ? 0 : this.second) + step); }
    /**
     * @param {?} second
     * @return {?}
     */
    updateSecond(second) {
        if (isNumber(second)) {
            this.second = second < 0 ? 60 + second % 60 : second % 60;
            this.changeMinute(Math.floor(second / 60));
        }
        else {
            this.second = NaN;
        }
    }
    /**
     * @param {?=} checkSecs
     * @return {?}
     */
    isValid(checkSecs = true) {
        return isNumber(this.hour) && isNumber(this.minute) && (checkSecs ? isNumber(this.second) : true);
    }
    /**
     * @return {?}
     */
    toString() { return `${this.hour || 0}:${this.minute || 0}:${this.second || 0}`; }
}
if (false) {
    /** @type {?} */
    NgbTime.prototype.hour;
    /** @type {?} */
    NgbTime.prototype.minute;
    /** @type {?} */
    NgbTime.prototype.second;
}

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoibmdiLXRpbWUuanMiLCJzb3VyY2VSb290Ijoibmc6Ly9AbmctYm9vdHN0cmFwL25nLWJvb3RzdHJhcC8iLCJzb3VyY2VzIjpbInRpbWVwaWNrZXIvbmdiLXRpbWUudHMiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6Ijs7OztBQUFBLE9BQU8sRUFBQyxRQUFRLEVBQUUsU0FBUyxFQUFDLE1BQU0sY0FBYyxDQUFDO0FBRWpELE1BQU07Ozs7OztJQUtKLFlBQVksSUFBYSxFQUFFLE1BQWUsRUFBRSxNQUFlO1FBQ3pELElBQUksQ0FBQyxJQUFJLEdBQUcsU0FBUyxDQUFDLElBQUksQ0FBQyxDQUFDO1FBQzVCLElBQUksQ0FBQyxNQUFNLEdBQUcsU0FBUyxDQUFDLE1BQU0sQ0FBQyxDQUFDO1FBQ2hDLElBQUksQ0FBQyxNQUFNLEdBQUcsU0FBUyxDQUFDLE1BQU0sQ0FBQyxDQUFDO0tBQ2pDOzs7OztJQUVELFVBQVUsQ0FBQyxJQUFJLEdBQUcsQ0FBQyxJQUFJLElBQUksQ0FBQyxVQUFVLENBQUMsQ0FBQyxLQUFLLENBQUMsSUFBSSxDQUFDLElBQUksQ0FBQyxDQUFDLENBQUMsQ0FBQyxDQUFDLENBQUMsQ0FBQyxDQUFDLElBQUksQ0FBQyxJQUFJLENBQUMsR0FBRyxJQUFJLENBQUMsQ0FBQyxFQUFFOzs7OztJQUVwRixVQUFVLENBQUMsSUFBWTtRQUNyQixFQUFFLENBQUMsQ0FBQyxRQUFRLENBQUMsSUFBSSxDQUFDLENBQUMsQ0FBQyxDQUFDO1lBQ25CLElBQUksQ0FBQyxJQUFJLEdBQUcsQ0FBQyxJQUFJLEdBQUcsQ0FBQyxDQUFDLENBQUMsQ0FBQyxFQUFFLEdBQUcsSUFBSSxDQUFDLENBQUMsQ0FBQyxJQUFJLENBQUMsR0FBRyxFQUFFLENBQUM7U0FDaEQ7UUFBQyxJQUFJLENBQUMsQ0FBQztZQUNOLElBQUksQ0FBQyxJQUFJLEdBQUcsR0FBRyxDQUFDO1NBQ2pCO0tBQ0Y7Ozs7O0lBRUQsWUFBWSxDQUFDLElBQUksR0FBRyxDQUFDLElBQUksSUFBSSxDQUFDLFlBQVksQ0FBQyxDQUFDLEtBQUssQ0FBQyxJQUFJLENBQUMsTUFBTSxDQUFDLENBQUMsQ0FBQyxDQUFDLENBQUMsQ0FBQyxDQUFDLENBQUMsSUFBSSxDQUFDLE1BQU0sQ0FBQyxHQUFHLElBQUksQ0FBQyxDQUFDLEVBQUU7Ozs7O0lBRTVGLFlBQVksQ0FBQyxNQUFjO1FBQ3pCLEVBQUUsQ0FBQyxDQUFDLFFBQVEsQ0FBQyxNQUFNLENBQUMsQ0FBQyxDQUFDLENBQUM7WUFDckIsSUFBSSxDQUFDLE1BQU0sR0FBRyxNQUFNLEdBQUcsRUFBRSxHQUFHLENBQUMsQ0FBQyxDQUFDLENBQUMsRUFBRSxHQUFHLE1BQU0sR0FBRyxFQUFFLENBQUMsQ0FBQyxDQUFDLE1BQU0sR0FBRyxFQUFFLENBQUM7WUFDL0QsSUFBSSxDQUFDLFVBQVUsQ0FBQyxJQUFJLENBQUMsS0FBSyxDQUFDLE1BQU0sR0FBRyxFQUFFLENBQUMsQ0FBQyxDQUFDO1NBQzFDO1FBQUMsSUFBSSxDQUFDLENBQUM7WUFDTixJQUFJLENBQUMsTUFBTSxHQUFHLEdBQUcsQ0FBQztTQUNuQjtLQUNGOzs7OztJQUVELFlBQVksQ0FBQyxJQUFJLEdBQUcsQ0FBQyxJQUFJLElBQUksQ0FBQyxZQUFZLENBQUMsQ0FBQyxLQUFLLENBQUMsSUFBSSxDQUFDLE1BQU0sQ0FBQyxDQUFDLENBQUMsQ0FBQyxDQUFDLENBQUMsQ0FBQyxDQUFDLElBQUksQ0FBQyxNQUFNLENBQUMsR0FBRyxJQUFJLENBQUMsQ0FBQyxFQUFFOzs7OztJQUU1RixZQUFZLENBQUMsTUFBYztRQUN6QixFQUFFLENBQUMsQ0FBQyxRQUFRLENBQUMsTUFBTSxDQUFDLENBQUMsQ0FBQyxDQUFDO1lBQ3JCLElBQUksQ0FBQyxNQUFNLEdBQUcsTUFBTSxHQUFHLENBQUMsQ0FBQyxDQUFDLENBQUMsRUFBRSxHQUFHLE1BQU0sR0FBRyxFQUFFLENBQUMsQ0FBQyxDQUFDLE1BQU0sR0FBRyxFQUFFLENBQUM7WUFDMUQsSUFBSSxDQUFDLFlBQVksQ0FBQyxJQUFJLENBQUMsS0FBSyxDQUFDLE1BQU0sR0FBRyxFQUFFLENBQUMsQ0FBQyxDQUFDO1NBQzVDO1FBQUMsSUFBSSxDQUFDLENBQUM7WUFDTixJQUFJLENBQUMsTUFBTSxHQUFHLEdBQUcsQ0FBQztTQUNuQjtLQUNGOzs7OztJQUVELE9BQU8sQ0FBQyxTQUFTLEdBQUcsSUFBSTtRQUN0QixNQUFNLENBQUMsUUFBUSxDQUFDLElBQUksQ0FBQyxJQUFJLENBQUMsSUFBSSxRQUFRLENBQUMsSUFBSSxDQUFDLE1BQU0sQ0FBQyxJQUFJLENBQUMsU0FBUyxDQUFDLENBQUMsQ0FBQyxRQUFRLENBQUMsSUFBSSxDQUFDLE1BQU0sQ0FBQyxDQUFDLENBQUMsQ0FBQyxJQUFJLENBQUMsQ0FBQztLQUNuRzs7OztJQUVELFFBQVEsS0FBSyxNQUFNLENBQUMsR0FBRyxJQUFJLENBQUMsSUFBSSxJQUFJLENBQUMsSUFBSSxJQUFJLENBQUMsTUFBTSxJQUFJLENBQUMsSUFBSSxJQUFJLENBQUMsTUFBTSxJQUFJLENBQUMsRUFBRSxDQUFDLEVBQUU7Q0FDbkYiLCJzb3VyY2VzQ29udGVudCI6WyJpbXBvcnQge2lzTnVtYmVyLCB0b0ludGVnZXJ9IGZyb20gJy4uL3V0aWwvdXRpbCc7XG5cbmV4cG9ydCBjbGFzcyBOZ2JUaW1lIHtcbiAgaG91cjogbnVtYmVyO1xuICBtaW51dGU6IG51bWJlcjtcbiAgc2Vjb25kOiBudW1iZXI7XG5cbiAgY29uc3RydWN0b3IoaG91cj86IG51bWJlciwgbWludXRlPzogbnVtYmVyLCBzZWNvbmQ/OiBudW1iZXIpIHtcbiAgICB0aGlzLmhvdXIgPSB0b0ludGVnZXIoaG91cik7XG4gICAgdGhpcy5taW51dGUgPSB0b0ludGVnZXIobWludXRlKTtcbiAgICB0aGlzLnNlY29uZCA9IHRvSW50ZWdlcihzZWNvbmQpO1xuICB9XG5cbiAgY2hhbmdlSG91cihzdGVwID0gMSkgeyB0aGlzLnVwZGF0ZUhvdXIoKGlzTmFOKHRoaXMuaG91cikgPyAwIDogdGhpcy5ob3VyKSArIHN0ZXApOyB9XG5cbiAgdXBkYXRlSG91cihob3VyOiBudW1iZXIpIHtcbiAgICBpZiAoaXNOdW1iZXIoaG91cikpIHtcbiAgICAgIHRoaXMuaG91ciA9IChob3VyIDwgMCA/IDI0ICsgaG91ciA6IGhvdXIpICUgMjQ7XG4gICAgfSBlbHNlIHtcbiAgICAgIHRoaXMuaG91ciA9IE5hTjtcbiAgICB9XG4gIH1cblxuICBjaGFuZ2VNaW51dGUoc3RlcCA9IDEpIHsgdGhpcy51cGRhdGVNaW51dGUoKGlzTmFOKHRoaXMubWludXRlKSA/IDAgOiB0aGlzLm1pbnV0ZSkgKyBzdGVwKTsgfVxuXG4gIHVwZGF0ZU1pbnV0ZShtaW51dGU6IG51bWJlcikge1xuICAgIGlmIChpc051bWJlcihtaW51dGUpKSB7XG4gICAgICB0aGlzLm1pbnV0ZSA9IG1pbnV0ZSAlIDYwIDwgMCA/IDYwICsgbWludXRlICUgNjAgOiBtaW51dGUgJSA2MDtcbiAgICAgIHRoaXMuY2hhbmdlSG91cihNYXRoLmZsb29yKG1pbnV0ZSAvIDYwKSk7XG4gICAgfSBlbHNlIHtcbiAgICAgIHRoaXMubWludXRlID0gTmFOO1xuICAgIH1cbiAgfVxuXG4gIGNoYW5nZVNlY29uZChzdGVwID0gMSkgeyB0aGlzLnVwZGF0ZVNlY29uZCgoaXNOYU4odGhpcy5zZWNvbmQpID8gMCA6IHRoaXMuc2Vjb25kKSArIHN0ZXApOyB9XG5cbiAgdXBkYXRlU2Vjb25kKHNlY29uZDogbnVtYmVyKSB7XG4gICAgaWYgKGlzTnVtYmVyKHNlY29uZCkpIHtcbiAgICAgIHRoaXMuc2Vjb25kID0gc2Vjb25kIDwgMCA/IDYwICsgc2Vjb25kICUgNjAgOiBzZWNvbmQgJSA2MDtcbiAgICAgIHRoaXMuY2hhbmdlTWludXRlKE1hdGguZmxvb3Ioc2Vjb25kIC8gNjApKTtcbiAgICB9IGVsc2Uge1xuICAgICAgdGhpcy5zZWNvbmQgPSBOYU47XG4gICAgfVxuICB9XG5cbiAgaXNWYWxpZChjaGVja1NlY3MgPSB0cnVlKSB7XG4gICAgcmV0dXJuIGlzTnVtYmVyKHRoaXMuaG91cikgJiYgaXNOdW1iZXIodGhpcy5taW51dGUpICYmIChjaGVja1NlY3MgPyBpc051bWJlcih0aGlzLnNlY29uZCkgOiB0cnVlKTtcbiAgfVxuXG4gIHRvU3RyaW5nKCkgeyByZXR1cm4gYCR7dGhpcy5ob3VyIHx8IDB9OiR7dGhpcy5taW51dGUgfHwgMH06JHt0aGlzLnNlY29uZCB8fCAwfWA7IH1cbn1cbiJdfQ==