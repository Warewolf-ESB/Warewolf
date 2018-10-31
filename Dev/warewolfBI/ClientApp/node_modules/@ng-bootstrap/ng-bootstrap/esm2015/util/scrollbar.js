/**
 * @fileoverview added by tsickle
 * @suppress {checkTypes,extraRequire,uselessCode} checked by tsc
 */
import { Injectable, Inject } from '@angular/core';
import { DOCUMENT } from '@angular/common';
import * as i0 from "@angular/core";
import * as i1 from "@angular/common";
/** @type {?} */
const noop = () => { };
const ɵ0 = noop;
/** @typedef {?} */
var CompensationReverter;
export { CompensationReverter };
/**
 * Utility to handle the scrollbar.
 *
 * It allows to compensate the lack of a vertical scrollbar by adding an
 * equivalent padding on the right of the body, and to remove this compensation.
 */
export class ScrollBar {
    /**
     * @param {?} _document
     */
    constructor(_document) {
        this._document = _document;
    }
    /**
     * Detects if a scrollbar is present and if yes, already compensates for its
     * removal by adding an equivalent padding on the right of the body.
     *
     * @return {?} a callback used to revert the compensation (noop if there was none,
     * otherwise a function removing the padding)
     */
    compensate() { return !this._isPresent() ? noop : this._adjustBody(this._getWidth()); }
    /**
     * Adds a padding of the given width on the right of the body.
     *
     * @param {?} width
     * @return {?} a callback used to revert the padding to its previous value
     */
    _adjustBody(width) {
        /** @type {?} */
        const body = this._document.body;
        /** @type {?} */
        const userSetPadding = body.style.paddingRight;
        /** @type {?} */
        const paddingAmount = parseFloat(window.getComputedStyle(body)['padding-right']);
        body.style['padding-right'] = `${paddingAmount + width}px`;
        return () => body.style['padding-right'] = userSetPadding;
    }
    /**
     * Tells whether a scrollbar is currently present on the body.
     *
     * @return {?} true if scrollbar is present, false otherwise
     */
    _isPresent() {
        /** @type {?} */
        const rect = this._document.body.getBoundingClientRect();
        return rect.left + rect.right < window.innerWidth;
    }
    /**
     * Calculates and returns the width of a scrollbar.
     *
     * @return {?} the width of a scrollbar on this page
     */
    _getWidth() {
        /** @type {?} */
        const measurer = this._document.createElement('div');
        measurer.className = 'modal-scrollbar-measure';
        /** @type {?} */
        const body = this._document.body;
        body.appendChild(measurer);
        /** @type {?} */
        const width = measurer.getBoundingClientRect().width - measurer.clientWidth;
        body.removeChild(measurer);
        return width;
    }
}
ScrollBar.decorators = [
    { type: Injectable, args: [{ providedIn: 'root' },] },
];
/** @nocollapse */
ScrollBar.ctorParameters = () => [
    { type: undefined, decorators: [{ type: Inject, args: [DOCUMENT,] }] }
];
/** @nocollapse */ ScrollBar.ngInjectableDef = i0.defineInjectable({ factory: function ScrollBar_Factory() { return new ScrollBar(i0.inject(i1.DOCUMENT)); }, token: ScrollBar, providedIn: "root" });
if (false) {
    /** @type {?} */
    ScrollBar.prototype._document;
}
export { ɵ0 };

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoic2Nyb2xsYmFyLmpzIiwic291cmNlUm9vdCI6Im5nOi8vQG5nLWJvb3RzdHJhcC9uZy1ib290c3RyYXAvIiwic291cmNlcyI6WyJ1dGlsL3Njcm9sbGJhci50cyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiOzs7O0FBQUEsT0FBTyxFQUFDLFVBQVUsRUFBRSxNQUFNLEVBQUMsTUFBTSxlQUFlLENBQUM7QUFDakQsT0FBTyxFQUFDLFFBQVEsRUFBQyxNQUFNLGlCQUFpQixDQUFDOzs7O0FBR3pDLE1BQU0sSUFBSSxHQUFHLEdBQUcsRUFBRSxJQUFHLENBQUM7Ozs7Ozs7Ozs7O0FBZ0J0QixNQUFNOzs7O0lBQ0osWUFBc0MsU0FBYztRQUFkLGNBQVMsR0FBVCxTQUFTLENBQUs7S0FBSTs7Ozs7Ozs7SUFTeEQsVUFBVSxLQUEyQixNQUFNLENBQUMsQ0FBQyxJQUFJLENBQUMsVUFBVSxFQUFFLENBQUMsQ0FBQyxDQUFDLElBQUksQ0FBQyxDQUFDLENBQUMsSUFBSSxDQUFDLFdBQVcsQ0FBQyxJQUFJLENBQUMsU0FBUyxFQUFFLENBQUMsQ0FBQyxFQUFFOzs7Ozs7O0lBT3JHLFdBQVcsQ0FBQyxLQUFhOztRQUMvQixNQUFNLElBQUksR0FBRyxJQUFJLENBQUMsU0FBUyxDQUFDLElBQUksQ0FBQzs7UUFDakMsTUFBTSxjQUFjLEdBQUcsSUFBSSxDQUFDLEtBQUssQ0FBQyxZQUFZLENBQUM7O1FBQy9DLE1BQU0sYUFBYSxHQUFHLFVBQVUsQ0FBQyxNQUFNLENBQUMsZ0JBQWdCLENBQUMsSUFBSSxDQUFDLENBQUMsZUFBZSxDQUFDLENBQUMsQ0FBQztRQUNqRixJQUFJLENBQUMsS0FBSyxDQUFDLGVBQWUsQ0FBQyxHQUFHLEdBQUcsYUFBYSxHQUFHLEtBQUssSUFBSSxDQUFDO1FBQzNELE1BQU0sQ0FBQyxHQUFHLEVBQUUsQ0FBQyxJQUFJLENBQUMsS0FBSyxDQUFDLGVBQWUsQ0FBQyxHQUFHLGNBQWMsQ0FBQzs7Ozs7OztJQVFwRCxVQUFVOztRQUNoQixNQUFNLElBQUksR0FBRyxJQUFJLENBQUMsU0FBUyxDQUFDLElBQUksQ0FBQyxxQkFBcUIsRUFBRSxDQUFDO1FBQ3pELE1BQU0sQ0FBQyxJQUFJLENBQUMsSUFBSSxHQUFHLElBQUksQ0FBQyxLQUFLLEdBQUcsTUFBTSxDQUFDLFVBQVUsQ0FBQzs7Ozs7OztJQVE1QyxTQUFTOztRQUNmLE1BQU0sUUFBUSxHQUFHLElBQUksQ0FBQyxTQUFTLENBQUMsYUFBYSxDQUFDLEtBQUssQ0FBQyxDQUFDO1FBQ3JELFFBQVEsQ0FBQyxTQUFTLEdBQUcseUJBQXlCLENBQUM7O1FBRS9DLE1BQU0sSUFBSSxHQUFHLElBQUksQ0FBQyxTQUFTLENBQUMsSUFBSSxDQUFDO1FBQ2pDLElBQUksQ0FBQyxXQUFXLENBQUMsUUFBUSxDQUFDLENBQUM7O1FBQzNCLE1BQU0sS0FBSyxHQUFHLFFBQVEsQ0FBQyxxQkFBcUIsRUFBRSxDQUFDLEtBQUssR0FBRyxRQUFRLENBQUMsV0FBVyxDQUFDO1FBQzVFLElBQUksQ0FBQyxXQUFXLENBQUMsUUFBUSxDQUFDLENBQUM7UUFFM0IsTUFBTSxDQUFDLEtBQUssQ0FBQzs7OztZQWxEaEIsVUFBVSxTQUFDLEVBQUMsVUFBVSxFQUFFLE1BQU0sRUFBQzs7Ozs0Q0FFakIsTUFBTSxTQUFDLFFBQVEiLCJzb3VyY2VzQ29udGVudCI6WyJpbXBvcnQge0luamVjdGFibGUsIEluamVjdH0gZnJvbSAnQGFuZ3VsYXIvY29yZSc7XG5pbXBvcnQge0RPQ1VNRU5UfSBmcm9tICdAYW5ndWxhci9jb21tb24nO1xuXG5cbmNvbnN0IG5vb3AgPSAoKSA9PiB7fTtcblxuXG5cbi8qKiBUeXBlIGZvciB0aGUgY2FsbGJhY2sgdXNlZCB0byByZXZlcnQgdGhlIHNjcm9sbGJhciBjb21wZW5zYXRpb24uICovXG5leHBvcnQgdHlwZSBDb21wZW5zYXRpb25SZXZlcnRlciA9ICgpID0+IHZvaWQ7XG5cblxuXG4vKipcbiAqIFV0aWxpdHkgdG8gaGFuZGxlIHRoZSBzY3JvbGxiYXIuXG4gKlxuICogSXQgYWxsb3dzIHRvIGNvbXBlbnNhdGUgdGhlIGxhY2sgb2YgYSB2ZXJ0aWNhbCBzY3JvbGxiYXIgYnkgYWRkaW5nIGFuXG4gKiBlcXVpdmFsZW50IHBhZGRpbmcgb24gdGhlIHJpZ2h0IG9mIHRoZSBib2R5LCBhbmQgdG8gcmVtb3ZlIHRoaXMgY29tcGVuc2F0aW9uLlxuICovXG5ASW5qZWN0YWJsZSh7cHJvdmlkZWRJbjogJ3Jvb3QnfSlcbmV4cG9ydCBjbGFzcyBTY3JvbGxCYXIge1xuICBjb25zdHJ1Y3RvcihASW5qZWN0KERPQ1VNRU5UKSBwcml2YXRlIF9kb2N1bWVudDogYW55KSB7fVxuXG4gIC8qKlxuICAgKiBEZXRlY3RzIGlmIGEgc2Nyb2xsYmFyIGlzIHByZXNlbnQgYW5kIGlmIHllcywgYWxyZWFkeSBjb21wZW5zYXRlcyBmb3IgaXRzXG4gICAqIHJlbW92YWwgYnkgYWRkaW5nIGFuIGVxdWl2YWxlbnQgcGFkZGluZyBvbiB0aGUgcmlnaHQgb2YgdGhlIGJvZHkuXG4gICAqXG4gICAqIEByZXR1cm4gYSBjYWxsYmFjayB1c2VkIHRvIHJldmVydCB0aGUgY29tcGVuc2F0aW9uIChub29wIGlmIHRoZXJlIHdhcyBub25lLFxuICAgKiBvdGhlcndpc2UgYSBmdW5jdGlvbiByZW1vdmluZyB0aGUgcGFkZGluZylcbiAgICovXG4gIGNvbXBlbnNhdGUoKTogQ29tcGVuc2F0aW9uUmV2ZXJ0ZXIgeyByZXR1cm4gIXRoaXMuX2lzUHJlc2VudCgpID8gbm9vcCA6IHRoaXMuX2FkanVzdEJvZHkodGhpcy5fZ2V0V2lkdGgoKSk7IH1cblxuICAvKipcbiAgICogQWRkcyBhIHBhZGRpbmcgb2YgdGhlIGdpdmVuIHdpZHRoIG9uIHRoZSByaWdodCBvZiB0aGUgYm9keS5cbiAgICpcbiAgICogQHJldHVybiBhIGNhbGxiYWNrIHVzZWQgdG8gcmV2ZXJ0IHRoZSBwYWRkaW5nIHRvIGl0cyBwcmV2aW91cyB2YWx1ZVxuICAgKi9cbiAgcHJpdmF0ZSBfYWRqdXN0Qm9keSh3aWR0aDogbnVtYmVyKTogQ29tcGVuc2F0aW9uUmV2ZXJ0ZXIge1xuICAgIGNvbnN0IGJvZHkgPSB0aGlzLl9kb2N1bWVudC5ib2R5O1xuICAgIGNvbnN0IHVzZXJTZXRQYWRkaW5nID0gYm9keS5zdHlsZS5wYWRkaW5nUmlnaHQ7XG4gICAgY29uc3QgcGFkZGluZ0Ftb3VudCA9IHBhcnNlRmxvYXQod2luZG93LmdldENvbXB1dGVkU3R5bGUoYm9keSlbJ3BhZGRpbmctcmlnaHQnXSk7XG4gICAgYm9keS5zdHlsZVsncGFkZGluZy1yaWdodCddID0gYCR7cGFkZGluZ0Ftb3VudCArIHdpZHRofXB4YDtcbiAgICByZXR1cm4gKCkgPT4gYm9keS5zdHlsZVsncGFkZGluZy1yaWdodCddID0gdXNlclNldFBhZGRpbmc7XG4gIH1cblxuICAvKipcbiAgICogVGVsbHMgd2hldGhlciBhIHNjcm9sbGJhciBpcyBjdXJyZW50bHkgcHJlc2VudCBvbiB0aGUgYm9keS5cbiAgICpcbiAgICogQHJldHVybiB0cnVlIGlmIHNjcm9sbGJhciBpcyBwcmVzZW50LCBmYWxzZSBvdGhlcndpc2VcbiAgICovXG4gIHByaXZhdGUgX2lzUHJlc2VudCgpOiBib29sZWFuIHtcbiAgICBjb25zdCByZWN0ID0gdGhpcy5fZG9jdW1lbnQuYm9keS5nZXRCb3VuZGluZ0NsaWVudFJlY3QoKTtcbiAgICByZXR1cm4gcmVjdC5sZWZ0ICsgcmVjdC5yaWdodCA8IHdpbmRvdy5pbm5lcldpZHRoO1xuICB9XG5cbiAgLyoqXG4gICAqIENhbGN1bGF0ZXMgYW5kIHJldHVybnMgdGhlIHdpZHRoIG9mIGEgc2Nyb2xsYmFyLlxuICAgKlxuICAgKiBAcmV0dXJuIHRoZSB3aWR0aCBvZiBhIHNjcm9sbGJhciBvbiB0aGlzIHBhZ2VcbiAgICovXG4gIHByaXZhdGUgX2dldFdpZHRoKCk6IG51bWJlciB7XG4gICAgY29uc3QgbWVhc3VyZXIgPSB0aGlzLl9kb2N1bWVudC5jcmVhdGVFbGVtZW50KCdkaXYnKTtcbiAgICBtZWFzdXJlci5jbGFzc05hbWUgPSAnbW9kYWwtc2Nyb2xsYmFyLW1lYXN1cmUnO1xuXG4gICAgY29uc3QgYm9keSA9IHRoaXMuX2RvY3VtZW50LmJvZHk7XG4gICAgYm9keS5hcHBlbmRDaGlsZChtZWFzdXJlcik7XG4gICAgY29uc3Qgd2lkdGggPSBtZWFzdXJlci5nZXRCb3VuZGluZ0NsaWVudFJlY3QoKS53aWR0aCAtIG1lYXN1cmVyLmNsaWVudFdpZHRoO1xuICAgIGJvZHkucmVtb3ZlQ2hpbGQobWVhc3VyZXIpO1xuXG4gICAgcmV0dXJuIHdpZHRoO1xuICB9XG59XG4iXX0=