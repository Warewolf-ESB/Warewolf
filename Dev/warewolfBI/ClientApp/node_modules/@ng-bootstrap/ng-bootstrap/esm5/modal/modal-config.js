/**
 * @fileoverview added by tsickle
 * @suppress {checkTypes,extraRequire,uselessCode} checked by tsc
 */
import { Injectable } from '@angular/core';
import * as i0 from "@angular/core";
/**
 * Represent options available when opening new modal windows.
 * @record
 */
export function NgbModalOptions() { }
/**
 * Sets the aria attribute aria-labelledby to a modal window.
 *
 * \@since 2.2.0
 * @type {?|undefined}
 */
NgbModalOptions.prototype.ariaLabelledBy;
/**
 * Whether a backdrop element should be created for a given modal (true by default).
 * Alternatively, specify 'static' for a backdrop which doesn't close the modal on click.
 * @type {?|undefined}
 */
NgbModalOptions.prototype.backdrop;
/**
 * Function called when a modal will be dismissed.
 * If this function returns false, the promise is resolved with false or the promise is rejected, the modal is not
 * dismissed.
 * @type {?|undefined}
 */
NgbModalOptions.prototype.beforeDismiss;
/**
 * To center the modal vertically (false by default).
 *
 * \@since 1.1.0
 * @type {?|undefined}
 */
NgbModalOptions.prototype.centered;
/**
 * An element to which to attach newly opened modal windows.
 * @type {?|undefined}
 */
NgbModalOptions.prototype.container;
/**
 * Injector to use for modal content.
 * @type {?|undefined}
 */
NgbModalOptions.prototype.injector;
/**
 * Whether to close the modal when escape key is pressed (true by default).
 * @type {?|undefined}
 */
NgbModalOptions.prototype.keyboard;
/**
 * Size of a new modal window.
 * @type {?|undefined}
 */
NgbModalOptions.prototype.size;
/**
 * Custom class to append to the modal window
 * @type {?|undefined}
 */
NgbModalOptions.prototype.windowClass;
/**
 * Custom class to append to the modal backdrop
 *
 * \@since 1.1.0
 * @type {?|undefined}
 */
NgbModalOptions.prototype.backdropClass;
/**
 * Configuration object token for the NgbModal service.
 * You can provide this configuration, typically in your root module in order to provide default option values for every
 * modal.
 *
 * \@since 3.1.0
 */
var NgbModalConfig = /** @class */ (function () {
    function NgbModalConfig() {
        this.backdrop = true;
        this.keyboard = true;
    }
    NgbModalConfig.decorators = [
        { type: Injectable, args: [{ providedIn: 'root' },] },
    ];
    /** @nocollapse */ NgbModalConfig.ngInjectableDef = i0.defineInjectable({ factory: function NgbModalConfig_Factory() { return new NgbModalConfig(); }, token: NgbModalConfig, providedIn: "root" });
    return NgbModalConfig;
}());
export { NgbModalConfig };
if (false) {
    /** @type {?} */
    NgbModalConfig.prototype.backdrop;
    /** @type {?} */
    NgbModalConfig.prototype.keyboard;
}

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoibW9kYWwtY29uZmlnLmpzIiwic291cmNlUm9vdCI6Im5nOi8vQG5nLWJvb3RzdHJhcC9uZy1ib290c3RyYXAvIiwic291cmNlcyI6WyJtb2RhbC9tb2RhbC1jb25maWcudHMiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6Ijs7OztBQUFBLE9BQU8sRUFBQyxVQUFVLEVBQVcsTUFBTSxlQUFlLENBQUM7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozt3QkEyRWxCLElBQUk7d0JBQ3hCLElBQUk7OztnQkFIaEIsVUFBVSxTQUFDLEVBQUMsVUFBVSxFQUFFLE1BQU0sRUFBQzs7O3lCQXpFaEM7O1NBMEVhLGNBQWMiLCJzb3VyY2VzQ29udGVudCI6WyJpbXBvcnQge0luamVjdGFibGUsIEluamVjdG9yfSBmcm9tICdAYW5ndWxhci9jb3JlJztcblxuLyoqXG4gKiBSZXByZXNlbnQgb3B0aW9ucyBhdmFpbGFibGUgd2hlbiBvcGVuaW5nIG5ldyBtb2RhbCB3aW5kb3dzLlxuICovXG5leHBvcnQgaW50ZXJmYWNlIE5nYk1vZGFsT3B0aW9ucyB7XG4gIC8qKlxuICAgKiBTZXRzIHRoZSBhcmlhIGF0dHJpYnV0ZSBhcmlhLWxhYmVsbGVkYnkgdG8gYSBtb2RhbCB3aW5kb3cuXG4gICAqXG4gICAqIEBzaW5jZSAyLjIuMFxuICAgKi9cbiAgYXJpYUxhYmVsbGVkQnk/OiBzdHJpbmc7XG5cbiAgLyoqXG4gICAqIFdoZXRoZXIgYSBiYWNrZHJvcCBlbGVtZW50IHNob3VsZCBiZSBjcmVhdGVkIGZvciBhIGdpdmVuIG1vZGFsICh0cnVlIGJ5IGRlZmF1bHQpLlxuICAgKiBBbHRlcm5hdGl2ZWx5LCBzcGVjaWZ5ICdzdGF0aWMnIGZvciBhIGJhY2tkcm9wIHdoaWNoIGRvZXNuJ3QgY2xvc2UgdGhlIG1vZGFsIG9uIGNsaWNrLlxuICAgKi9cbiAgYmFja2Ryb3A/OiBib29sZWFuIHwgJ3N0YXRpYyc7XG5cbiAgLyoqXG4gICAqIEZ1bmN0aW9uIGNhbGxlZCB3aGVuIGEgbW9kYWwgd2lsbCBiZSBkaXNtaXNzZWQuXG4gICAqIElmIHRoaXMgZnVuY3Rpb24gcmV0dXJucyBmYWxzZSwgdGhlIHByb21pc2UgaXMgcmVzb2x2ZWQgd2l0aCBmYWxzZSBvciB0aGUgcHJvbWlzZSBpcyByZWplY3RlZCwgdGhlIG1vZGFsIGlzIG5vdFxuICAgKiBkaXNtaXNzZWQuXG4gICAqL1xuICBiZWZvcmVEaXNtaXNzPzogKCkgPT4gYm9vbGVhbiB8IFByb21pc2U8Ym9vbGVhbj47XG5cbiAgLyoqXG4gICAqIFRvIGNlbnRlciB0aGUgbW9kYWwgdmVydGljYWxseSAoZmFsc2UgYnkgZGVmYXVsdCkuXG4gICAqXG4gICAqIEBzaW5jZSAxLjEuMFxuICAgKi9cbiAgY2VudGVyZWQ/OiBib29sZWFuO1xuXG4gIC8qKlxuICAgKiBBbiBlbGVtZW50IHRvIHdoaWNoIHRvIGF0dGFjaCBuZXdseSBvcGVuZWQgbW9kYWwgd2luZG93cy5cbiAgICovXG4gIGNvbnRhaW5lcj86IHN0cmluZztcblxuICAvKipcbiAgICogSW5qZWN0b3IgdG8gdXNlIGZvciBtb2RhbCBjb250ZW50LlxuICAgKi9cbiAgaW5qZWN0b3I/OiBJbmplY3RvcjtcblxuICAvKipcbiAgICogV2hldGhlciB0byBjbG9zZSB0aGUgbW9kYWwgd2hlbiBlc2NhcGUga2V5IGlzIHByZXNzZWQgKHRydWUgYnkgZGVmYXVsdCkuXG4gICAqL1xuICBrZXlib2FyZD86IGJvb2xlYW47XG5cbiAgLyoqXG4gICAqIFNpemUgb2YgYSBuZXcgbW9kYWwgd2luZG93LlxuICAgKi9cbiAgc2l6ZT86ICdzbScgfCAnbGcnO1xuXG4gIC8qKlxuICAgKiBDdXN0b20gY2xhc3MgdG8gYXBwZW5kIHRvIHRoZSBtb2RhbCB3aW5kb3dcbiAgICovXG4gIHdpbmRvd0NsYXNzPzogc3RyaW5nO1xuXG4gIC8qKlxuICAgKiBDdXN0b20gY2xhc3MgdG8gYXBwZW5kIHRvIHRoZSBtb2RhbCBiYWNrZHJvcFxuICAgKlxuICAgKiBAc2luY2UgMS4xLjBcbiAgICovXG4gIGJhY2tkcm9wQ2xhc3M/OiBzdHJpbmc7XG59XG5cbi8qKlxuKiBDb25maWd1cmF0aW9uIG9iamVjdCB0b2tlbiBmb3IgdGhlIE5nYk1vZGFsIHNlcnZpY2UuXG4qIFlvdSBjYW4gcHJvdmlkZSB0aGlzIGNvbmZpZ3VyYXRpb24sIHR5cGljYWxseSBpbiB5b3VyIHJvb3QgbW9kdWxlIGluIG9yZGVyIHRvIHByb3ZpZGUgZGVmYXVsdCBvcHRpb24gdmFsdWVzIGZvciBldmVyeVxuKiBtb2RhbC5cbipcbiogQHNpbmNlIDMuMS4wXG4qL1xuQEluamVjdGFibGUoe3Byb3ZpZGVkSW46ICdyb290J30pXG5leHBvcnQgY2xhc3MgTmdiTW9kYWxDb25maWcgaW1wbGVtZW50cyBOZ2JNb2RhbE9wdGlvbnMge1xuICBiYWNrZHJvcDogYm9vbGVhbiB8ICdzdGF0aWMnID0gdHJ1ZTtcbiAga2V5Ym9hcmQgPSB0cnVlO1xufVxuIl19