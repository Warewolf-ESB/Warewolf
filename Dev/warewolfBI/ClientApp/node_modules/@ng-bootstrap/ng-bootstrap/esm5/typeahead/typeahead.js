/**
 * @fileoverview added by tsickle
 * @suppress {checkTypes,extraRequire,uselessCode} checked by tsc
 */
import { ComponentFactoryResolver, Directive, ElementRef, EventEmitter, forwardRef, Injector, Input, NgZone, Output, Renderer2, TemplateRef, ViewContainerRef } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { BehaviorSubject, fromEvent } from 'rxjs';
import { positionElements } from '../util/positioning';
import { NgbTypeaheadWindow } from './typeahead-window';
import { PopupService } from '../util/popup';
import { toString, isDefined } from '../util/util';
import { Key } from '../util/key';
import { Live } from '../util/accessibility/live';
import { NgbTypeaheadConfig } from './typeahead-config';
import { map, switchMap, tap } from 'rxjs/operators';
/** @type {?} */
var NGB_TYPEAHEAD_VALUE_ACCESSOR = {
    provide: NG_VALUE_ACCESSOR,
    useExisting: forwardRef(function () { return NgbTypeahead; }),
    multi: true
};
/**
 * Payload of the selectItem event.
 * @record
 */
export function NgbTypeaheadSelectItemEvent() { }
/**
 * An item about to be selected
 * @type {?}
 */
NgbTypeaheadSelectItemEvent.prototype.item;
/**
 * Function that will prevent item selection if called
 * @type {?}
 */
NgbTypeaheadSelectItemEvent.prototype.preventDefault;
/** @type {?} */
var nextWindowId = 0;
/**
 * NgbTypeahead directive provides a simple way of creating powerful typeaheads from any text input
 */
var NgbTypeahead = /** @class */ (function () {
    function NgbTypeahead(_elementRef, _viewContainerRef, _renderer, _injector, componentFactoryResolver, config, ngZone, _live) {
        var _this = this;
        this._elementRef = _elementRef;
        this._viewContainerRef = _viewContainerRef;
        this._renderer = _renderer;
        this._injector = _injector;
        this._live = _live;
        /**
         * Value for the configurable autocomplete attribute.
         * Defaults to 'off' to disable the native browser autocomplete, but this standard value does not seem
         * to be always correctly taken into account.
         *
         * \@since 2.1.0
         */
        this.autocomplete = 'off';
        /**
         * Placement of a typeahead accepts:
         *    "top", "top-left", "top-right", "bottom", "bottom-left", "bottom-right",
         *    "left", "left-top", "left-bottom", "right", "right-top", "right-bottom"
         * and array of above values.
         */
        this.placement = 'bottom-left';
        /**
         * An event emitted when a match is selected. Event payload is of type NgbTypeaheadSelectItemEvent.
         */
        this.selectItem = new EventEmitter();
        this.popupId = "ngb-typeahead-" + nextWindowId++;
        this._onTouched = function () { };
        this._onChange = function (_) { };
        this.container = config.container;
        this.editable = config.editable;
        this.focusFirst = config.focusFirst;
        this.showHint = config.showHint;
        this.placement = config.placement;
        this._valueChanges = fromEvent(_elementRef.nativeElement, 'input')
            .pipe(map(function ($event) { return (/** @type {?} */ ($event.target)).value; }));
        this._resubscribeTypeahead = new BehaviorSubject(null);
        this._popupService = new PopupService(NgbTypeaheadWindow, _injector, _viewContainerRef, _renderer, componentFactoryResolver);
        this._zoneSubscription = ngZone.onStable.subscribe(function () {
            if (_this.isPopupOpen()) {
                positionElements(_this._elementRef.nativeElement, _this._windowRef.location.nativeElement, _this.placement, _this.container === 'body');
            }
        });
    }
    /**
     * @return {?}
     */
    NgbTypeahead.prototype.ngOnInit = /**
     * @return {?}
     */
    function () {
        var _this = this;
        /** @type {?} */
        var inputValues$ = this._valueChanges.pipe(tap(function (value) {
            _this._inputValueBackup = value;
            if (_this.editable) {
                _this._onChange(value);
            }
        }));
        /** @type {?} */
        var results$ = inputValues$.pipe(this.ngbTypeahead);
        /** @type {?} */
        var processedResults$ = results$.pipe(tap(function () {
            if (!_this.editable) {
                _this._onChange(undefined);
            }
        }));
        /** @type {?} */
        var userInput$ = this._resubscribeTypeahead.pipe(switchMap(function () { return processedResults$; }));
        this._subscription = this._subscribeToUserInput(userInput$);
    };
    /**
     * @return {?}
     */
    NgbTypeahead.prototype.ngOnDestroy = /**
     * @return {?}
     */
    function () {
        this._closePopup();
        this._unsubscribeFromUserInput();
        this._zoneSubscription.unsubscribe();
    };
    /**
     * @param {?} fn
     * @return {?}
     */
    NgbTypeahead.prototype.registerOnChange = /**
     * @param {?} fn
     * @return {?}
     */
    function (fn) { this._onChange = fn; };
    /**
     * @param {?} fn
     * @return {?}
     */
    NgbTypeahead.prototype.registerOnTouched = /**
     * @param {?} fn
     * @return {?}
     */
    function (fn) { this._onTouched = fn; };
    /**
     * @param {?} value
     * @return {?}
     */
    NgbTypeahead.prototype.writeValue = /**
     * @param {?} value
     * @return {?}
     */
    function (value) { this._writeInputValue(this._formatItemForInput(value)); };
    /**
     * @param {?} isDisabled
     * @return {?}
     */
    NgbTypeahead.prototype.setDisabledState = /**
     * @param {?} isDisabled
     * @return {?}
     */
    function (isDisabled) {
        this._renderer.setProperty(this._elementRef.nativeElement, 'disabled', isDisabled);
    };
    /**
     * @param {?} event
     * @return {?}
     */
    NgbTypeahead.prototype.onDocumentClick = /**
     * @param {?} event
     * @return {?}
     */
    function (event) {
        if (event.target !== this._elementRef.nativeElement) {
            this.dismissPopup();
        }
    };
    /**
     * Dismisses typeahead popup window
     */
    /**
     * Dismisses typeahead popup window
     * @return {?}
     */
    NgbTypeahead.prototype.dismissPopup = /**
     * Dismisses typeahead popup window
     * @return {?}
     */
    function () {
        if (this.isPopupOpen()) {
            this._closePopup();
            this._writeInputValue(this._inputValueBackup);
        }
    };
    /**
     * Returns true if the typeahead popup window is displayed
     */
    /**
     * Returns true if the typeahead popup window is displayed
     * @return {?}
     */
    NgbTypeahead.prototype.isPopupOpen = /**
     * Returns true if the typeahead popup window is displayed
     * @return {?}
     */
    function () { return this._windowRef != null; };
    /**
     * @return {?}
     */
    NgbTypeahead.prototype.handleBlur = /**
     * @return {?}
     */
    function () {
        this._resubscribeTypeahead.next(null);
        this._onTouched();
    };
    /**
     * @param {?} event
     * @return {?}
     */
    NgbTypeahead.prototype.handleKeyDown = /**
     * @param {?} event
     * @return {?}
     */
    function (event) {
        if (!this.isPopupOpen()) {
            return;
        }
        if (Key[toString(event.which)]) {
            switch (event.which) {
                case Key.ArrowDown:
                    event.preventDefault();
                    this._windowRef.instance.next();
                    this._showHint();
                    break;
                case Key.ArrowUp:
                    event.preventDefault();
                    this._windowRef.instance.prev();
                    this._showHint();
                    break;
                case Key.Enter:
                case Key.Tab:
                    /** @type {?} */
                    var result = this._windowRef.instance.getActive();
                    if (isDefined(result)) {
                        event.preventDefault();
                        event.stopPropagation();
                        this._selectResult(result);
                    }
                    this._closePopup();
                    break;
                case Key.Escape:
                    event.preventDefault();
                    this._resubscribeTypeahead.next(null);
                    this.dismissPopup();
                    break;
            }
        }
    };
    /**
     * @return {?}
     */
    NgbTypeahead.prototype._openPopup = /**
     * @return {?}
     */
    function () {
        var _this = this;
        if (!this.isPopupOpen()) {
            this._inputValueBackup = this._elementRef.nativeElement.value;
            this._windowRef = this._popupService.open();
            this._windowRef.instance.id = this.popupId;
            this._windowRef.instance.selectEvent.subscribe(function (result) { return _this._selectResultClosePopup(result); });
            this._windowRef.instance.activeChangeEvent.subscribe(function (activeId) { return _this.activeDescendant = activeId; });
            if (this.container === 'body') {
                window.document.querySelector(this.container).appendChild(this._windowRef.location.nativeElement);
            }
        }
    };
    /**
     * @return {?}
     */
    NgbTypeahead.prototype._closePopup = /**
     * @return {?}
     */
    function () {
        this._popupService.close();
        this._windowRef = null;
        this.activeDescendant = undefined;
    };
    /**
     * @param {?} result
     * @return {?}
     */
    NgbTypeahead.prototype._selectResult = /**
     * @param {?} result
     * @return {?}
     */
    function (result) {
        /** @type {?} */
        var defaultPrevented = false;
        this.selectItem.emit({ item: result, preventDefault: function () { defaultPrevented = true; } });
        this._resubscribeTypeahead.next(null);
        if (!defaultPrevented) {
            this.writeValue(result);
            this._onChange(result);
        }
    };
    /**
     * @param {?} result
     * @return {?}
     */
    NgbTypeahead.prototype._selectResultClosePopup = /**
     * @param {?} result
     * @return {?}
     */
    function (result) {
        this._selectResult(result);
        this._closePopup();
    };
    /**
     * @return {?}
     */
    NgbTypeahead.prototype._showHint = /**
     * @return {?}
     */
    function () {
        if (this.showHint && this._windowRef.instance.hasActive() && this._inputValueBackup != null) {
            /** @type {?} */
            var userInputLowerCase = this._inputValueBackup.toLowerCase();
            /** @type {?} */
            var formattedVal = this._formatItemForInput(this._windowRef.instance.getActive());
            if (userInputLowerCase === formattedVal.substr(0, this._inputValueBackup.length).toLowerCase()) {
                this._writeInputValue(this._inputValueBackup + formattedVal.substr(this._inputValueBackup.length));
                this._elementRef.nativeElement['setSelectionRange'].apply(this._elementRef.nativeElement, [this._inputValueBackup.length, formattedVal.length]);
            }
            else {
                this.writeValue(this._windowRef.instance.getActive());
            }
        }
    };
    /**
     * @param {?} item
     * @return {?}
     */
    NgbTypeahead.prototype._formatItemForInput = /**
     * @param {?} item
     * @return {?}
     */
    function (item) {
        return item != null && this.inputFormatter ? this.inputFormatter(item) : toString(item);
    };
    /**
     * @param {?} value
     * @return {?}
     */
    NgbTypeahead.prototype._writeInputValue = /**
     * @param {?} value
     * @return {?}
     */
    function (value) {
        this._renderer.setProperty(this._elementRef.nativeElement, 'value', toString(value));
    };
    /**
     * @param {?} userInput$
     * @return {?}
     */
    NgbTypeahead.prototype._subscribeToUserInput = /**
     * @param {?} userInput$
     * @return {?}
     */
    function (userInput$) {
        var _this = this;
        return userInput$.subscribe(function (results) {
            if (!results || results.length === 0) {
                _this._closePopup();
            }
            else {
                _this._openPopup();
                _this._windowRef.instance.focusFirst = _this.focusFirst;
                _this._windowRef.instance.results = results;
                _this._windowRef.instance.term = _this._elementRef.nativeElement.value;
                if (_this.resultFormatter) {
                    _this._windowRef.instance.formatter = _this.resultFormatter;
                }
                if (_this.resultTemplate) {
                    _this._windowRef.instance.resultTemplate = _this.resultTemplate;
                }
                _this._windowRef.instance.resetActive();
                // The observable stream we are subscribing to might have async steps
                // and if a component containing typeahead is using the OnPush strategy
                // the change detection turn wouldn't be invoked automatically.
                // The observable stream we are subscribing to might have async steps
                // and if a component containing typeahead is using the OnPush strategy
                // the change detection turn wouldn't be invoked automatically.
                _this._windowRef.changeDetectorRef.detectChanges();
                _this._showHint();
            }
            /** @type {?} */
            var count = results ? results.length : 0;
            _this._live.say(count === 0 ? 'No results available' : count + " result" + (count === 1 ? '' : 's') + " available");
        });
    };
    /**
     * @return {?}
     */
    NgbTypeahead.prototype._unsubscribeFromUserInput = /**
     * @return {?}
     */
    function () {
        if (this._subscription) {
            this._subscription.unsubscribe();
        }
        this._subscription = null;
    };
    NgbTypeahead.decorators = [
        { type: Directive, args: [{
                    selector: 'input[ngbTypeahead]',
                    exportAs: 'ngbTypeahead',
                    host: {
                        '(blur)': 'handleBlur()',
                        '[class.open]': 'isPopupOpen()',
                        '(document:click)': 'onDocumentClick($event)',
                        '(keydown)': 'handleKeyDown($event)',
                        '[autocomplete]': 'autocomplete',
                        'autocapitalize': 'off',
                        'autocorrect': 'off',
                        'role': 'combobox',
                        'aria-multiline': 'false',
                        '[attr.aria-autocomplete]': 'showHint ? "both" : "list"',
                        '[attr.aria-activedescendant]': 'activeDescendant',
                        '[attr.aria-owns]': 'isPopupOpen() ? popupId : null',
                        '[attr.aria-expanded]': 'isPopupOpen()'
                    },
                    providers: [NGB_TYPEAHEAD_VALUE_ACCESSOR]
                },] },
    ];
    /** @nocollapse */
    NgbTypeahead.ctorParameters = function () { return [
        { type: ElementRef },
        { type: ViewContainerRef },
        { type: Renderer2 },
        { type: Injector },
        { type: ComponentFactoryResolver },
        { type: NgbTypeaheadConfig },
        { type: NgZone },
        { type: Live }
    ]; };
    NgbTypeahead.propDecorators = {
        autocomplete: [{ type: Input }],
        container: [{ type: Input }],
        editable: [{ type: Input }],
        focusFirst: [{ type: Input }],
        inputFormatter: [{ type: Input }],
        ngbTypeahead: [{ type: Input }],
        resultFormatter: [{ type: Input }],
        resultTemplate: [{ type: Input }],
        showHint: [{ type: Input }],
        placement: [{ type: Input }],
        selectItem: [{ type: Output }]
    };
    return NgbTypeahead;
}());
export { NgbTypeahead };
if (false) {
    /** @type {?} */
    NgbTypeahead.prototype._popupService;
    /** @type {?} */
    NgbTypeahead.prototype._subscription;
    /** @type {?} */
    NgbTypeahead.prototype._inputValueBackup;
    /** @type {?} */
    NgbTypeahead.prototype._valueChanges;
    /** @type {?} */
    NgbTypeahead.prototype._resubscribeTypeahead;
    /** @type {?} */
    NgbTypeahead.prototype._windowRef;
    /** @type {?} */
    NgbTypeahead.prototype._zoneSubscription;
    /**
     * Value for the configurable autocomplete attribute.
     * Defaults to 'off' to disable the native browser autocomplete, but this standard value does not seem
     * to be always correctly taken into account.
     *
     * \@since 2.1.0
     * @type {?}
     */
    NgbTypeahead.prototype.autocomplete;
    /**
     * A selector specifying the element the tooltip should be appended to.
     * Currently only supports "body".
     * @type {?}
     */
    NgbTypeahead.prototype.container;
    /**
     * A flag indicating if model values should be restricted to the ones selected from the popup only.
     * @type {?}
     */
    NgbTypeahead.prototype.editable;
    /**
     * A flag indicating if the first match should automatically be focused as you type.
     * @type {?}
     */
    NgbTypeahead.prototype.focusFirst;
    /**
     * A function to convert a given value into string to display in the input field
     * @type {?}
     */
    NgbTypeahead.prototype.inputFormatter;
    /**
     * A function to transform the provided observable text into the array of results.  Note that the "this" argument
     * is undefined so you need to explicitly bind it to a desired "this" target.
     * @type {?}
     */
    NgbTypeahead.prototype.ngbTypeahead;
    /**
     * A function to format a given result before display. This function should return a formatted string without any
     * HTML markup
     * @type {?}
     */
    NgbTypeahead.prototype.resultFormatter;
    /**
     * A template to override a matching result default display
     * @type {?}
     */
    NgbTypeahead.prototype.resultTemplate;
    /**
     * Show hint when an option in the result list matches.
     * @type {?}
     */
    NgbTypeahead.prototype.showHint;
    /**
     * Placement of a typeahead accepts:
     *    "top", "top-left", "top-right", "bottom", "bottom-left", "bottom-right",
     *    "left", "left-top", "left-bottom", "right", "right-top", "right-bottom"
     * and array of above values.
     * @type {?}
     */
    NgbTypeahead.prototype.placement;
    /**
     * An event emitted when a match is selected. Event payload is of type NgbTypeaheadSelectItemEvent.
     * @type {?}
     */
    NgbTypeahead.prototype.selectItem;
    /** @type {?} */
    NgbTypeahead.prototype.activeDescendant;
    /** @type {?} */
    NgbTypeahead.prototype.popupId;
    /** @type {?} */
    NgbTypeahead.prototype._onTouched;
    /** @type {?} */
    NgbTypeahead.prototype._onChange;
    /** @type {?} */
    NgbTypeahead.prototype._elementRef;
    /** @type {?} */
    NgbTypeahead.prototype._viewContainerRef;
    /** @type {?} */
    NgbTypeahead.prototype._renderer;
    /** @type {?} */
    NgbTypeahead.prototype._injector;
    /** @type {?} */
    NgbTypeahead.prototype._live;
}

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoidHlwZWFoZWFkLmpzIiwic291cmNlUm9vdCI6Im5nOi8vQG5nLWJvb3RzdHJhcC9uZy1ib290c3RyYXAvIiwic291cmNlcyI6WyJ0eXBlYWhlYWQvdHlwZWFoZWFkLnRzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiI7Ozs7QUFBQSxPQUFPLEVBQ0wsd0JBQXdCLEVBRXhCLFNBQVMsRUFDVCxVQUFVLEVBQ1YsWUFBWSxFQUNaLFVBQVUsRUFDVixRQUFRLEVBQ1IsS0FBSyxFQUNMLE1BQU0sRUFHTixNQUFNLEVBQ04sU0FBUyxFQUNULFdBQVcsRUFDWCxnQkFBZ0IsRUFDakIsTUFBTSxlQUFlLENBQUM7QUFDdkIsT0FBTyxFQUF1QixpQkFBaUIsRUFBQyxNQUFNLGdCQUFnQixDQUFDO0FBQ3ZFLE9BQU8sRUFBYSxlQUFlLEVBQWdCLFNBQVMsRUFBQyxNQUFNLE1BQU0sQ0FBQztBQUMxRSxPQUFPLEVBQUMsZ0JBQWdCLEVBQWlCLE1BQU0scUJBQXFCLENBQUM7QUFDckUsT0FBTyxFQUFDLGtCQUFrQixFQUF3QixNQUFNLG9CQUFvQixDQUFDO0FBQzdFLE9BQU8sRUFBQyxZQUFZLEVBQUMsTUFBTSxlQUFlLENBQUM7QUFDM0MsT0FBTyxFQUFDLFFBQVEsRUFBRSxTQUFTLEVBQUMsTUFBTSxjQUFjLENBQUM7QUFDakQsT0FBTyxFQUFDLEdBQUcsRUFBQyxNQUFNLGFBQWEsQ0FBQztBQUNoQyxPQUFPLEVBQUMsSUFBSSxFQUFDLE1BQU0sNEJBQTRCLENBQUM7QUFDaEQsT0FBTyxFQUFDLGtCQUFrQixFQUFDLE1BQU0sb0JBQW9CLENBQUM7QUFDdEQsT0FBTyxFQUFDLEdBQUcsRUFBRSxTQUFTLEVBQUUsR0FBRyxFQUFDLE1BQU0sZ0JBQWdCLENBQUM7O0FBRW5ELElBQU0sNEJBQTRCLEdBQUc7SUFDbkMsT0FBTyxFQUFFLGlCQUFpQjtJQUMxQixXQUFXLEVBQUUsVUFBVSxDQUFDLGNBQU0sT0FBQSxZQUFZLEVBQVosQ0FBWSxDQUFDO0lBQzNDLEtBQUssRUFBRSxJQUFJ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QkFDcEIsTUFBTSxFQUFFLFVBQVU7d0JBQ2xCLGdCQUFnQixFQUFFLE9BQU87d0JBQ3pCLDBCQUEwQixFQUFFLDRCQUE0Qjt3QkFDeEQsOEJBQThCLEVBQUUsa0JBQWtCO3dCQUNsRCxrQkFBa0IsRUFBRSxnQ0FBZ0M7d0JBQ3BELHNCQUFzQixFQUFFLGVBQWU7cUJBQ3hDO29CQUNELFNBQVMsRUFBRSxDQUFDLDRCQUE0QixDQUFDO2lCQUMxQzs7OztnQkFyRUMsVUFBVTtnQkFXVixnQkFBZ0I7Z0JBRmhCLFNBQVM7Z0JBTlQsUUFBUTtnQkFOUix3QkFBd0I7Z0JBd0JsQixrQkFBa0I7Z0JBaEJ4QixNQUFNO2dCQWVBLElBQUk7OzsrQkFtRVQsS0FBSzs0QkFNTCxLQUFLOzJCQUtMLEtBQUs7NkJBS0wsS0FBSztpQ0FLTCxLQUFLOytCQU1MLEtBQUs7a0NBTUwsS0FBSztpQ0FLTCxLQUFLOzJCQUtMLEtBQUs7NEJBT0wsS0FBSzs2QkFLTCxNQUFNOzt1QkFsSlQ7O1NBMEVhLFlBQVkiLCJzb3VyY2VzQ29udGVudCI6WyJpbXBvcnQge1xuICBDb21wb25lbnRGYWN0b3J5UmVzb2x2ZXIsXG4gIENvbXBvbmVudFJlZixcbiAgRGlyZWN0aXZlLFxuICBFbGVtZW50UmVmLFxuICBFdmVudEVtaXR0ZXIsXG4gIGZvcndhcmRSZWYsXG4gIEluamVjdG9yLFxuICBJbnB1dCxcbiAgTmdab25lLFxuICBPbkRlc3Ryb3ksXG4gIE9uSW5pdCxcbiAgT3V0cHV0LFxuICBSZW5kZXJlcjIsXG4gIFRlbXBsYXRlUmVmLFxuICBWaWV3Q29udGFpbmVyUmVmXG59IGZyb20gJ0Bhbmd1bGFyL2NvcmUnO1xuaW1wb3J0IHtDb250cm9sVmFsdWVBY2Nlc3NvciwgTkdfVkFMVUVfQUNDRVNTT1J9IGZyb20gJ0Bhbmd1bGFyL2Zvcm1zJztcbmltcG9ydCB7T2JzZXJ2YWJsZSwgQmVoYXZpb3JTdWJqZWN0LCBTdWJzY3JpcHRpb24sIGZyb21FdmVudH0gZnJvbSAncnhqcyc7XG5pbXBvcnQge3Bvc2l0aW9uRWxlbWVudHMsIFBsYWNlbWVudEFycmF5fSBmcm9tICcuLi91dGlsL3Bvc2l0aW9uaW5nJztcbmltcG9ydCB7TmdiVHlwZWFoZWFkV2luZG93LCBSZXN1bHRUZW1wbGF0ZUNvbnRleHR9IGZyb20gJy4vdHlwZWFoZWFkLXdpbmRvdyc7XG5pbXBvcnQge1BvcHVwU2VydmljZX0gZnJvbSAnLi4vdXRpbC9wb3B1cCc7XG5pbXBvcnQge3RvU3RyaW5nLCBpc0RlZmluZWR9IGZyb20gJy4uL3V0aWwvdXRpbCc7XG5pbXBvcnQge0tleX0gZnJvbSAnLi4vdXRpbC9rZXknO1xuaW1wb3J0IHtMaXZlfSBmcm9tICcuLi91dGlsL2FjY2Vzc2liaWxpdHkvbGl2ZSc7XG5pbXBvcnQge05nYlR5cGVhaGVhZENvbmZpZ30gZnJvbSAnLi90eXBlYWhlYWQtY29uZmlnJztcbmltcG9ydCB7bWFwLCBzd2l0Y2hNYXAsIHRhcH0gZnJvbSAncnhqcy9vcGVyYXRvcnMnO1xuXG5jb25zdCBOR0JfVFlQRUFIRUFEX1ZBTFVFX0FDQ0VTU09SID0ge1xuICBwcm92aWRlOiBOR19WQUxVRV9BQ0NFU1NPUixcbiAgdXNlRXhpc3Rpbmc6IGZvcndhcmRSZWYoKCkgPT4gTmdiVHlwZWFoZWFkKSxcbiAgbXVsdGk6IHRydWVcbn07XG5cbi8qKlxuICogUGF5bG9hZCBvZiB0aGUgc2VsZWN0SXRlbSBldmVudC5cbiAqL1xuZXhwb3J0IGludGVyZmFjZSBOZ2JUeXBlYWhlYWRTZWxlY3RJdGVtRXZlbnQge1xuICAvKipcbiAgICogQW4gaXRlbSBhYm91dCB0byBiZSBzZWxlY3RlZFxuICAgKi9cbiAgaXRlbTogYW55O1xuXG4gIC8qKlxuICAgKiBGdW5jdGlvbiB0aGF0IHdpbGwgcHJldmVudCBpdGVtIHNlbGVjdGlvbiBpZiBjYWxsZWRcbiAgICovXG4gIHByZXZlbnREZWZhdWx0OiAoKSA9PiB2b2lkO1xufVxuXG5sZXQgbmV4dFdpbmRvd0lkID0gMDtcblxuLyoqXG4gKiBOZ2JUeXBlYWhlYWQgZGlyZWN0aXZlIHByb3ZpZGVzIGEgc2ltcGxlIHdheSBvZiBjcmVhdGluZyBwb3dlcmZ1bCB0eXBlYWhlYWRzIGZyb20gYW55IHRleHQgaW5wdXRcbiAqL1xuQERpcmVjdGl2ZSh7XG4gIHNlbGVjdG9yOiAnaW5wdXRbbmdiVHlwZWFoZWFkXScsXG4gIGV4cG9ydEFzOiAnbmdiVHlwZWFoZWFkJyxcbiAgaG9zdDoge1xuICAgICcoYmx1ciknOiAnaGFuZGxlQmx1cigpJyxcbiAgICAnW2NsYXNzLm9wZW5dJzogJ2lzUG9wdXBPcGVuKCknLFxuICAgICcoZG9jdW1lbnQ6Y2xpY2spJzogJ29uRG9jdW1lbnRDbGljaygkZXZlbnQpJyxcbiAgICAnKGtleWRvd24pJzogJ2hhbmRsZUtleURvd24oJGV2ZW50KScsXG4gICAgJ1thdXRvY29tcGxldGVdJzogJ2F1dG9jb21wbGV0ZScsXG4gICAgJ2F1dG9jYXBpdGFsaXplJzogJ29mZicsXG4gICAgJ2F1dG9jb3JyZWN0JzogJ29mZicsXG4gICAgJ3JvbGUnOiAnY29tYm9ib3gnLFxuICAgICdhcmlhLW11bHRpbGluZSc6ICdmYWxzZScsXG4gICAgJ1thdHRyLmFyaWEtYXV0b2NvbXBsZXRlXSc6ICdzaG93SGludCA/IFwiYm90aFwiIDogXCJsaXN0XCInLFxuICAgICdbYXR0ci5hcmlhLWFjdGl2ZWRlc2NlbmRhbnRdJzogJ2FjdGl2ZURlc2NlbmRhbnQnLFxuICAgICdbYXR0ci5hcmlhLW93bnNdJzogJ2lzUG9wdXBPcGVuKCkgPyBwb3B1cElkIDogbnVsbCcsXG4gICAgJ1thdHRyLmFyaWEtZXhwYW5kZWRdJzogJ2lzUG9wdXBPcGVuKCknXG4gIH0sXG4gIHByb3ZpZGVyczogW05HQl9UWVBFQUhFQURfVkFMVUVfQUNDRVNTT1JdXG59KVxuZXhwb3J0IGNsYXNzIE5nYlR5cGVhaGVhZCBpbXBsZW1lbnRzIENvbnRyb2xWYWx1ZUFjY2Vzc29yLFxuICAgIE9uSW5pdCwgT25EZXN0cm95IHtcbiAgcHJpdmF0ZSBfcG9wdXBTZXJ2aWNlOiBQb3B1cFNlcnZpY2U8TmdiVHlwZWFoZWFkV2luZG93PjtcbiAgcHJpdmF0ZSBfc3Vic2NyaXB0aW9uOiBTdWJzY3JpcHRpb247XG4gIHByaXZhdGUgX2lucHV0VmFsdWVCYWNrdXA6IHN0cmluZztcbiAgcHJpdmF0ZSBfdmFsdWVDaGFuZ2VzOiBPYnNlcnZhYmxlPHN0cmluZz47XG4gIHByaXZhdGUgX3Jlc3Vic2NyaWJlVHlwZWFoZWFkOiBCZWhhdmlvclN1YmplY3Q8YW55PjtcbiAgcHJpdmF0ZSBfd2luZG93UmVmOiBDb21wb25lbnRSZWY8TmdiVHlwZWFoZWFkV2luZG93PjtcbiAgcHJpdmF0ZSBfem9uZVN1YnNjcmlwdGlvbjogYW55O1xuXG4gIC8qKlxuICAgKiBWYWx1ZSBmb3IgdGhlIGNvbmZpZ3VyYWJsZSBhdXRvY29tcGxldGUgYXR0cmlidXRlLlxuICAgKiBEZWZhdWx0cyB0byAnb2ZmJyB0byBkaXNhYmxlIHRoZSBuYXRpdmUgYnJvd3NlciBhdXRvY29tcGxldGUsIGJ1dCB0aGlzIHN0YW5kYXJkIHZhbHVlIGRvZXMgbm90IHNlZW1cbiAgICogdG8gYmUgYWx3YXlzIGNvcnJlY3RseSB0YWtlbiBpbnRvIGFjY291bnQuXG4gICAqXG4gICAqIEBzaW5jZSAyLjEuMFxuICAgKi9cbiAgQElucHV0KCkgYXV0b2NvbXBsZXRlID0gJ29mZic7XG5cbiAgLyoqXG4gICAqIEEgc2VsZWN0b3Igc3BlY2lmeWluZyB0aGUgZWxlbWVudCB0aGUgdG9vbHRpcCBzaG91bGQgYmUgYXBwZW5kZWQgdG8uXG4gICAqIEN1cnJlbnRseSBvbmx5IHN1cHBvcnRzIFwiYm9keVwiLlxuICAgKi9cbiAgQElucHV0KCkgY29udGFpbmVyOiBzdHJpbmc7XG5cbiAgLyoqXG4gICAqIEEgZmxhZyBpbmRpY2F0aW5nIGlmIG1vZGVsIHZhbHVlcyBzaG91bGQgYmUgcmVzdHJpY3RlZCB0byB0aGUgb25lcyBzZWxlY3RlZCBmcm9tIHRoZSBwb3B1cCBvbmx5LlxuICAgKi9cbiAgQElucHV0KCkgZWRpdGFibGU6IGJvb2xlYW47XG5cbiAgLyoqXG4gICAqIEEgZmxhZyBpbmRpY2F0aW5nIGlmIHRoZSBmaXJzdCBtYXRjaCBzaG91bGQgYXV0b21hdGljYWxseSBiZSBmb2N1c2VkIGFzIHlvdSB0eXBlLlxuICAgKi9cbiAgQElucHV0KCkgZm9jdXNGaXJzdDogYm9vbGVhbjtcblxuICAvKipcbiAgICogQSBmdW5jdGlvbiB0byBjb252ZXJ0IGEgZ2l2ZW4gdmFsdWUgaW50byBzdHJpbmcgdG8gZGlzcGxheSBpbiB0aGUgaW5wdXQgZmllbGRcbiAgICovXG4gIEBJbnB1dCgpIGlucHV0Rm9ybWF0dGVyOiAodmFsdWU6IGFueSkgPT4gc3RyaW5nO1xuXG4gIC8qKlxuICAgKiBBIGZ1bmN0aW9uIHRvIHRyYW5zZm9ybSB0aGUgcHJvdmlkZWQgb2JzZXJ2YWJsZSB0ZXh0IGludG8gdGhlIGFycmF5IG9mIHJlc3VsdHMuICBOb3RlIHRoYXQgdGhlIFwidGhpc1wiIGFyZ3VtZW50XG4gICAqIGlzIHVuZGVmaW5lZCBzbyB5b3UgbmVlZCB0byBleHBsaWNpdGx5IGJpbmQgaXQgdG8gYSBkZXNpcmVkIFwidGhpc1wiIHRhcmdldC5cbiAgICovXG4gIEBJbnB1dCgpIG5nYlR5cGVhaGVhZDogKHRleHQ6IE9ic2VydmFibGU8c3RyaW5nPikgPT4gT2JzZXJ2YWJsZTxhbnlbXT47XG5cbiAgLyoqXG4gICAqIEEgZnVuY3Rpb24gdG8gZm9ybWF0IGEgZ2l2ZW4gcmVzdWx0IGJlZm9yZSBkaXNwbGF5LiBUaGlzIGZ1bmN0aW9uIHNob3VsZCByZXR1cm4gYSBmb3JtYXR0ZWQgc3RyaW5nIHdpdGhvdXQgYW55XG4gICAqIEhUTUwgbWFya3VwXG4gICAqL1xuICBASW5wdXQoKSByZXN1bHRGb3JtYXR0ZXI6ICh2YWx1ZTogYW55KSA9PiBzdHJpbmc7XG5cbiAgLyoqXG4gICAqIEEgdGVtcGxhdGUgdG8gb3ZlcnJpZGUgYSBtYXRjaGluZyByZXN1bHQgZGVmYXVsdCBkaXNwbGF5XG4gICAqL1xuICBASW5wdXQoKSByZXN1bHRUZW1wbGF0ZTogVGVtcGxhdGVSZWY8UmVzdWx0VGVtcGxhdGVDb250ZXh0PjtcblxuICAvKipcbiAgICogU2hvdyBoaW50IHdoZW4gYW4gb3B0aW9uIGluIHRoZSByZXN1bHQgbGlzdCBtYXRjaGVzLlxuICAgKi9cbiAgQElucHV0KCkgc2hvd0hpbnQ6IGJvb2xlYW47XG5cbiAgLyoqIFBsYWNlbWVudCBvZiBhIHR5cGVhaGVhZCBhY2NlcHRzOlxuICAgKiAgICBcInRvcFwiLCBcInRvcC1sZWZ0XCIsIFwidG9wLXJpZ2h0XCIsIFwiYm90dG9tXCIsIFwiYm90dG9tLWxlZnRcIiwgXCJib3R0b20tcmlnaHRcIixcbiAgICogICAgXCJsZWZ0XCIsIFwibGVmdC10b3BcIiwgXCJsZWZ0LWJvdHRvbVwiLCBcInJpZ2h0XCIsIFwicmlnaHQtdG9wXCIsIFwicmlnaHQtYm90dG9tXCJcbiAgICogYW5kIGFycmF5IG9mIGFib3ZlIHZhbHVlcy5cbiAgKi9cbiAgQElucHV0KCkgcGxhY2VtZW50OiBQbGFjZW1lbnRBcnJheSA9ICdib3R0b20tbGVmdCc7XG5cbiAgLyoqXG4gICAqIEFuIGV2ZW50IGVtaXR0ZWQgd2hlbiBhIG1hdGNoIGlzIHNlbGVjdGVkLiBFdmVudCBwYXlsb2FkIGlzIG9mIHR5cGUgTmdiVHlwZWFoZWFkU2VsZWN0SXRlbUV2ZW50LlxuICAgKi9cbiAgQE91dHB1dCgpIHNlbGVjdEl0ZW0gPSBuZXcgRXZlbnRFbWl0dGVyPE5nYlR5cGVhaGVhZFNlbGVjdEl0ZW1FdmVudD4oKTtcblxuICBhY3RpdmVEZXNjZW5kYW50OiBzdHJpbmc7XG4gIHBvcHVwSWQgPSBgbmdiLXR5cGVhaGVhZC0ke25leHRXaW5kb3dJZCsrfWA7XG5cbiAgcHJpdmF0ZSBfb25Ub3VjaGVkID0gKCkgPT4ge307XG4gIHByaXZhdGUgX29uQ2hhbmdlID0gKF86IGFueSkgPT4ge307XG5cbiAgY29uc3RydWN0b3IoXG4gICAgICBwcml2YXRlIF9lbGVtZW50UmVmOiBFbGVtZW50UmVmPEhUTUxJbnB1dEVsZW1lbnQ+LCBwcml2YXRlIF92aWV3Q29udGFpbmVyUmVmOiBWaWV3Q29udGFpbmVyUmVmLFxuICAgICAgcHJpdmF0ZSBfcmVuZGVyZXI6IFJlbmRlcmVyMiwgcHJpdmF0ZSBfaW5qZWN0b3I6IEluamVjdG9yLCBjb21wb25lbnRGYWN0b3J5UmVzb2x2ZXI6IENvbXBvbmVudEZhY3RvcnlSZXNvbHZlcixcbiAgICAgIGNvbmZpZzogTmdiVHlwZWFoZWFkQ29uZmlnLCBuZ1pvbmU6IE5nWm9uZSwgcHJpdmF0ZSBfbGl2ZTogTGl2ZSkge1xuICAgIHRoaXMuY29udGFpbmVyID0gY29uZmlnLmNvbnRhaW5lcjtcbiAgICB0aGlzLmVkaXRhYmxlID0gY29uZmlnLmVkaXRhYmxlO1xuICAgIHRoaXMuZm9jdXNGaXJzdCA9IGNvbmZpZy5mb2N1c0ZpcnN0O1xuICAgIHRoaXMuc2hvd0hpbnQgPSBjb25maWcuc2hvd0hpbnQ7XG4gICAgdGhpcy5wbGFjZW1lbnQgPSBjb25maWcucGxhY2VtZW50O1xuXG4gICAgdGhpcy5fdmFsdWVDaGFuZ2VzID0gZnJvbUV2ZW50PEV2ZW50PihfZWxlbWVudFJlZi5uYXRpdmVFbGVtZW50LCAnaW5wdXQnKVxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAucGlwZShtYXAoJGV2ZW50ID0+ICgkZXZlbnQudGFyZ2V0IGFzIEhUTUxJbnB1dEVsZW1lbnQpLnZhbHVlKSk7XG5cbiAgICB0aGlzLl9yZXN1YnNjcmliZVR5cGVhaGVhZCA9IG5ldyBCZWhhdmlvclN1YmplY3QobnVsbCk7XG5cbiAgICB0aGlzLl9wb3B1cFNlcnZpY2UgPSBuZXcgUG9wdXBTZXJ2aWNlPE5nYlR5cGVhaGVhZFdpbmRvdz4oXG4gICAgICAgIE5nYlR5cGVhaGVhZFdpbmRvdywgX2luamVjdG9yLCBfdmlld0NvbnRhaW5lclJlZiwgX3JlbmRlcmVyLCBjb21wb25lbnRGYWN0b3J5UmVzb2x2ZXIpO1xuXG4gICAgdGhpcy5fem9uZVN1YnNjcmlwdGlvbiA9IG5nWm9uZS5vblN0YWJsZS5zdWJzY3JpYmUoKCkgPT4ge1xuICAgICAgaWYgKHRoaXMuaXNQb3B1cE9wZW4oKSkge1xuICAgICAgICBwb3NpdGlvbkVsZW1lbnRzKFxuICAgICAgICAgICAgdGhpcy5fZWxlbWVudFJlZi5uYXRpdmVFbGVtZW50LCB0aGlzLl93aW5kb3dSZWYubG9jYXRpb24ubmF0aXZlRWxlbWVudCwgdGhpcy5wbGFjZW1lbnQsXG4gICAgICAgICAgICB0aGlzLmNvbnRhaW5lciA9PT0gJ2JvZHknKTtcbiAgICAgIH1cbiAgICB9KTtcbiAgfVxuXG4gIG5nT25Jbml0KCk6IHZvaWQge1xuICAgIGNvbnN0IGlucHV0VmFsdWVzJCA9IHRoaXMuX3ZhbHVlQ2hhbmdlcy5waXBlKHRhcCh2YWx1ZSA9PiB7XG4gICAgICB0aGlzLl9pbnB1dFZhbHVlQmFja3VwID0gdmFsdWU7XG4gICAgICBpZiAodGhpcy5lZGl0YWJsZSkge1xuICAgICAgICB0aGlzLl9vbkNoYW5nZSh2YWx1ZSk7XG4gICAgICB9XG4gICAgfSkpO1xuICAgIGNvbnN0IHJlc3VsdHMkID0gaW5wdXRWYWx1ZXMkLnBpcGUodGhpcy5uZ2JUeXBlYWhlYWQpO1xuICAgIGNvbnN0IHByb2Nlc3NlZFJlc3VsdHMkID0gcmVzdWx0cyQucGlwZSh0YXAoKCkgPT4ge1xuICAgICAgaWYgKCF0aGlzLmVkaXRhYmxlKSB7XG4gICAgICAgIHRoaXMuX29uQ2hhbmdlKHVuZGVmaW5lZCk7XG4gICAgICB9XG4gICAgfSkpO1xuICAgIGNvbnN0IHVzZXJJbnB1dCQgPSB0aGlzLl9yZXN1YnNjcmliZVR5cGVhaGVhZC5waXBlKHN3aXRjaE1hcCgoKSA9PiBwcm9jZXNzZWRSZXN1bHRzJCkpO1xuICAgIHRoaXMuX3N1YnNjcmlwdGlvbiA9IHRoaXMuX3N1YnNjcmliZVRvVXNlcklucHV0KHVzZXJJbnB1dCQpO1xuICB9XG5cbiAgbmdPbkRlc3Ryb3koKTogdm9pZCB7XG4gICAgdGhpcy5fY2xvc2VQb3B1cCgpO1xuICAgIHRoaXMuX3Vuc3Vic2NyaWJlRnJvbVVzZXJJbnB1dCgpO1xuICAgIHRoaXMuX3pvbmVTdWJzY3JpcHRpb24udW5zdWJzY3JpYmUoKTtcbiAgfVxuXG4gIHJlZ2lzdGVyT25DaGFuZ2UoZm46ICh2YWx1ZTogYW55KSA9PiBhbnkpOiB2b2lkIHsgdGhpcy5fb25DaGFuZ2UgPSBmbjsgfVxuXG4gIHJlZ2lzdGVyT25Ub3VjaGVkKGZuOiAoKSA9PiBhbnkpOiB2b2lkIHsgdGhpcy5fb25Ub3VjaGVkID0gZm47IH1cblxuICB3cml0ZVZhbHVlKHZhbHVlKSB7IHRoaXMuX3dyaXRlSW5wdXRWYWx1ZSh0aGlzLl9mb3JtYXRJdGVtRm9ySW5wdXQodmFsdWUpKTsgfVxuXG4gIHNldERpc2FibGVkU3RhdGUoaXNEaXNhYmxlZDogYm9vbGVhbik6IHZvaWQge1xuICAgIHRoaXMuX3JlbmRlcmVyLnNldFByb3BlcnR5KHRoaXMuX2VsZW1lbnRSZWYubmF0aXZlRWxlbWVudCwgJ2Rpc2FibGVkJywgaXNEaXNhYmxlZCk7XG4gIH1cblxuICBvbkRvY3VtZW50Q2xpY2soZXZlbnQpIHtcbiAgICBpZiAoZXZlbnQudGFyZ2V0ICE9PSB0aGlzLl9lbGVtZW50UmVmLm5hdGl2ZUVsZW1lbnQpIHtcbiAgICAgIHRoaXMuZGlzbWlzc1BvcHVwKCk7XG4gICAgfVxuICB9XG5cbiAgLyoqXG4gICAqIERpc21pc3NlcyB0eXBlYWhlYWQgcG9wdXAgd2luZG93XG4gICAqL1xuICBkaXNtaXNzUG9wdXAoKSB7XG4gICAgaWYgKHRoaXMuaXNQb3B1cE9wZW4oKSkge1xuICAgICAgdGhpcy5fY2xvc2VQb3B1cCgpO1xuICAgICAgdGhpcy5fd3JpdGVJbnB1dFZhbHVlKHRoaXMuX2lucHV0VmFsdWVCYWNrdXApO1xuICAgIH1cbiAgfVxuXG4gIC8qKlxuICAgKiBSZXR1cm5zIHRydWUgaWYgdGhlIHR5cGVhaGVhZCBwb3B1cCB3aW5kb3cgaXMgZGlzcGxheWVkXG4gICAqL1xuICBpc1BvcHVwT3BlbigpIHsgcmV0dXJuIHRoaXMuX3dpbmRvd1JlZiAhPSBudWxsOyB9XG5cbiAgaGFuZGxlQmx1cigpIHtcbiAgICB0aGlzLl9yZXN1YnNjcmliZVR5cGVhaGVhZC5uZXh0KG51bGwpO1xuICAgIHRoaXMuX29uVG91Y2hlZCgpO1xuICB9XG5cbiAgaGFuZGxlS2V5RG93bihldmVudDogS2V5Ym9hcmRFdmVudCkge1xuICAgIGlmICghdGhpcy5pc1BvcHVwT3BlbigpKSB7XG4gICAgICByZXR1cm47XG4gICAgfVxuXG4gICAgaWYgKEtleVt0b1N0cmluZyhldmVudC53aGljaCldKSB7XG4gICAgICBzd2l0Y2ggKGV2ZW50LndoaWNoKSB7XG4gICAgICAgIGNhc2UgS2V5LkFycm93RG93bjpcbiAgICAgICAgICBldmVudC5wcmV2ZW50RGVmYXVsdCgpO1xuICAgICAgICAgIHRoaXMuX3dpbmRvd1JlZi5pbnN0YW5jZS5uZXh0KCk7XG4gICAgICAgICAgdGhpcy5fc2hvd0hpbnQoKTtcbiAgICAgICAgICBicmVhaztcbiAgICAgICAgY2FzZSBLZXkuQXJyb3dVcDpcbiAgICAgICAgICBldmVudC5wcmV2ZW50RGVmYXVsdCgpO1xuICAgICAgICAgIHRoaXMuX3dpbmRvd1JlZi5pbnN0YW5jZS5wcmV2KCk7XG4gICAgICAgICAgdGhpcy5fc2hvd0hpbnQoKTtcbiAgICAgICAgICBicmVhaztcbiAgICAgICAgY2FzZSBLZXkuRW50ZXI6XG4gICAgICAgIGNhc2UgS2V5LlRhYjpcbiAgICAgICAgICBjb25zdCByZXN1bHQgPSB0aGlzLl93aW5kb3dSZWYuaW5zdGFuY2UuZ2V0QWN0aXZlKCk7XG4gICAgICAgICAgaWYgKGlzRGVmaW5lZChyZXN1bHQpKSB7XG4gICAgICAgICAgICBldmVudC5wcmV2ZW50RGVmYXVsdCgpO1xuICAgICAgICAgICAgZXZlbnQuc3RvcFByb3BhZ2F0aW9uKCk7XG4gICAgICAgICAgICB0aGlzLl9zZWxlY3RSZXN1bHQocmVzdWx0KTtcbiAgICAgICAgICB9XG4gICAgICAgICAgdGhpcy5fY2xvc2VQb3B1cCgpO1xuICAgICAgICAgIGJyZWFrO1xuICAgICAgICBjYXNlIEtleS5Fc2NhcGU6XG4gICAgICAgICAgZXZlbnQucHJldmVudERlZmF1bHQoKTtcbiAgICAgICAgICB0aGlzLl9yZXN1YnNjcmliZVR5cGVhaGVhZC5uZXh0KG51bGwpO1xuICAgICAgICAgIHRoaXMuZGlzbWlzc1BvcHVwKCk7XG4gICAgICAgICAgYnJlYWs7XG4gICAgICB9XG4gICAgfVxuICB9XG5cbiAgcHJpdmF0ZSBfb3BlblBvcHVwKCkge1xuICAgIGlmICghdGhpcy5pc1BvcHVwT3BlbigpKSB7XG4gICAgICB0aGlzLl9pbnB1dFZhbHVlQmFja3VwID0gdGhpcy5fZWxlbWVudFJlZi5uYXRpdmVFbGVtZW50LnZhbHVlO1xuICAgICAgdGhpcy5fd2luZG93UmVmID0gdGhpcy5fcG9wdXBTZXJ2aWNlLm9wZW4oKTtcbiAgICAgIHRoaXMuX3dpbmRvd1JlZi5pbnN0YW5jZS5pZCA9IHRoaXMucG9wdXBJZDtcbiAgICAgIHRoaXMuX3dpbmRvd1JlZi5pbnN0YW5jZS5zZWxlY3RFdmVudC5zdWJzY3JpYmUoKHJlc3VsdDogYW55KSA9PiB0aGlzLl9zZWxlY3RSZXN1bHRDbG9zZVBvcHVwKHJlc3VsdCkpO1xuICAgICAgdGhpcy5fd2luZG93UmVmLmluc3RhbmNlLmFjdGl2ZUNoYW5nZUV2ZW50LnN1YnNjcmliZSgoYWN0aXZlSWQ6IHN0cmluZykgPT4gdGhpcy5hY3RpdmVEZXNjZW5kYW50ID0gYWN0aXZlSWQpO1xuXG4gICAgICBpZiAodGhpcy5jb250YWluZXIgPT09ICdib2R5Jykge1xuICAgICAgICB3aW5kb3cuZG9jdW1lbnQucXVlcnlTZWxlY3Rvcih0aGlzLmNvbnRhaW5lcikuYXBwZW5kQ2hpbGQodGhpcy5fd2luZG93UmVmLmxvY2F0aW9uLm5hdGl2ZUVsZW1lbnQpO1xuICAgICAgfVxuICAgIH1cbiAgfVxuXG4gIHByaXZhdGUgX2Nsb3NlUG9wdXAoKSB7XG4gICAgdGhpcy5fcG9wdXBTZXJ2aWNlLmNsb3NlKCk7XG4gICAgdGhpcy5fd2luZG93UmVmID0gbnVsbDtcbiAgICB0aGlzLmFjdGl2ZURlc2NlbmRhbnQgPSB1bmRlZmluZWQ7XG4gIH1cblxuICBwcml2YXRlIF9zZWxlY3RSZXN1bHQocmVzdWx0OiBhbnkpIHtcbiAgICBsZXQgZGVmYXVsdFByZXZlbnRlZCA9IGZhbHNlO1xuICAgIHRoaXMuc2VsZWN0SXRlbS5lbWl0KHtpdGVtOiByZXN1bHQsIHByZXZlbnREZWZhdWx0OiAoKSA9PiB7IGRlZmF1bHRQcmV2ZW50ZWQgPSB0cnVlOyB9fSk7XG4gICAgdGhpcy5fcmVzdWJzY3JpYmVUeXBlYWhlYWQubmV4dChudWxsKTtcblxuICAgIGlmICghZGVmYXVsdFByZXZlbnRlZCkge1xuICAgICAgdGhpcy53cml0ZVZhbHVlKHJlc3VsdCk7XG4gICAgICB0aGlzLl9vbkNoYW5nZShyZXN1bHQpO1xuICAgIH1cbiAgfVxuXG4gIHByaXZhdGUgX3NlbGVjdFJlc3VsdENsb3NlUG9wdXAocmVzdWx0OiBhbnkpIHtcbiAgICB0aGlzLl9zZWxlY3RSZXN1bHQocmVzdWx0KTtcbiAgICB0aGlzLl9jbG9zZVBvcHVwKCk7XG4gIH1cblxuICBwcml2YXRlIF9zaG93SGludCgpIHtcbiAgICBpZiAodGhpcy5zaG93SGludCAmJiB0aGlzLl93aW5kb3dSZWYuaW5zdGFuY2UuaGFzQWN0aXZlKCkgJiYgdGhpcy5faW5wdXRWYWx1ZUJhY2t1cCAhPSBudWxsKSB7XG4gICAgICBjb25zdCB1c2VySW5wdXRMb3dlckNhc2UgPSB0aGlzLl9pbnB1dFZhbHVlQmFja3VwLnRvTG93ZXJDYXNlKCk7XG4gICAgICBjb25zdCBmb3JtYXR0ZWRWYWwgPSB0aGlzLl9mb3JtYXRJdGVtRm9ySW5wdXQodGhpcy5fd2luZG93UmVmLmluc3RhbmNlLmdldEFjdGl2ZSgpKTtcblxuICAgICAgaWYgKHVzZXJJbnB1dExvd2VyQ2FzZSA9PT0gZm9ybWF0dGVkVmFsLnN1YnN0cigwLCB0aGlzLl9pbnB1dFZhbHVlQmFja3VwLmxlbmd0aCkudG9Mb3dlckNhc2UoKSkge1xuICAgICAgICB0aGlzLl93cml0ZUlucHV0VmFsdWUodGhpcy5faW5wdXRWYWx1ZUJhY2t1cCArIGZvcm1hdHRlZFZhbC5zdWJzdHIodGhpcy5faW5wdXRWYWx1ZUJhY2t1cC5sZW5ndGgpKTtcbiAgICAgICAgdGhpcy5fZWxlbWVudFJlZi5uYXRpdmVFbGVtZW50WydzZXRTZWxlY3Rpb25SYW5nZSddLmFwcGx5KFxuICAgICAgICAgICAgdGhpcy5fZWxlbWVudFJlZi5uYXRpdmVFbGVtZW50LCBbdGhpcy5faW5wdXRWYWx1ZUJhY2t1cC5sZW5ndGgsIGZvcm1hdHRlZFZhbC5sZW5ndGhdKTtcbiAgICAgIH0gZWxzZSB7XG4gICAgICAgIHRoaXMud3JpdGVWYWx1ZSh0aGlzLl93aW5kb3dSZWYuaW5zdGFuY2UuZ2V0QWN0aXZlKCkpO1xuICAgICAgfVxuICAgIH1cbiAgfVxuXG4gIHByaXZhdGUgX2Zvcm1hdEl0ZW1Gb3JJbnB1dChpdGVtOiBhbnkpOiBzdHJpbmcge1xuICAgIHJldHVybiBpdGVtICE9IG51bGwgJiYgdGhpcy5pbnB1dEZvcm1hdHRlciA/IHRoaXMuaW5wdXRGb3JtYXR0ZXIoaXRlbSkgOiB0b1N0cmluZyhpdGVtKTtcbiAgfVxuXG4gIHByaXZhdGUgX3dyaXRlSW5wdXRWYWx1ZSh2YWx1ZTogc3RyaW5nKTogdm9pZCB7XG4gICAgdGhpcy5fcmVuZGVyZXIuc2V0UHJvcGVydHkodGhpcy5fZWxlbWVudFJlZi5uYXRpdmVFbGVtZW50LCAndmFsdWUnLCB0b1N0cmluZyh2YWx1ZSkpO1xuICB9XG5cbiAgcHJpdmF0ZSBfc3Vic2NyaWJlVG9Vc2VySW5wdXQodXNlcklucHV0JDogT2JzZXJ2YWJsZTxhbnlbXT4pOiBTdWJzY3JpcHRpb24ge1xuICAgIHJldHVybiB1c2VySW5wdXQkLnN1YnNjcmliZSgocmVzdWx0cykgPT4ge1xuICAgICAgaWYgKCFyZXN1bHRzIHx8IHJlc3VsdHMubGVuZ3RoID09PSAwKSB7XG4gICAgICAgIHRoaXMuX2Nsb3NlUG9wdXAoKTtcbiAgICAgIH0gZWxzZSB7XG4gICAgICAgIHRoaXMuX29wZW5Qb3B1cCgpO1xuICAgICAgICB0aGlzLl93aW5kb3dSZWYuaW5zdGFuY2UuZm9jdXNGaXJzdCA9IHRoaXMuZm9jdXNGaXJzdDtcbiAgICAgICAgdGhpcy5fd2luZG93UmVmLmluc3RhbmNlLnJlc3VsdHMgPSByZXN1bHRzO1xuICAgICAgICB0aGlzLl93aW5kb3dSZWYuaW5zdGFuY2UudGVybSA9IHRoaXMuX2VsZW1lbnRSZWYubmF0aXZlRWxlbWVudC52YWx1ZTtcbiAgICAgICAgaWYgKHRoaXMucmVzdWx0Rm9ybWF0dGVyKSB7XG4gICAgICAgICAgdGhpcy5fd2luZG93UmVmLmluc3RhbmNlLmZvcm1hdHRlciA9IHRoaXMucmVzdWx0Rm9ybWF0dGVyO1xuICAgICAgICB9XG4gICAgICAgIGlmICh0aGlzLnJlc3VsdFRlbXBsYXRlKSB7XG4gICAgICAgICAgdGhpcy5fd2luZG93UmVmLmluc3RhbmNlLnJlc3VsdFRlbXBsYXRlID0gdGhpcy5yZXN1bHRUZW1wbGF0ZTtcbiAgICAgICAgfVxuICAgICAgICB0aGlzLl93aW5kb3dSZWYuaW5zdGFuY2UucmVzZXRBY3RpdmUoKTtcblxuICAgICAgICAvLyBUaGUgb2JzZXJ2YWJsZSBzdHJlYW0gd2UgYXJlIHN1YnNjcmliaW5nIHRvIG1pZ2h0IGhhdmUgYXN5bmMgc3RlcHNcbiAgICAgICAgLy8gYW5kIGlmIGEgY29tcG9uZW50IGNvbnRhaW5pbmcgdHlwZWFoZWFkIGlzIHVzaW5nIHRoZSBPblB1c2ggc3RyYXRlZ3lcbiAgICAgICAgLy8gdGhlIGNoYW5nZSBkZXRlY3Rpb24gdHVybiB3b3VsZG4ndCBiZSBpbnZva2VkIGF1dG9tYXRpY2FsbHkuXG4gICAgICAgIHRoaXMuX3dpbmRvd1JlZi5jaGFuZ2VEZXRlY3RvclJlZi5kZXRlY3RDaGFuZ2VzKCk7XG5cbiAgICAgICAgdGhpcy5fc2hvd0hpbnQoKTtcbiAgICAgIH1cblxuICAgICAgLy8gbGl2ZSBhbm5vdW5jZXJcbiAgICAgIGNvbnN0IGNvdW50ID0gcmVzdWx0cyA/IHJlc3VsdHMubGVuZ3RoIDogMDtcbiAgICAgIHRoaXMuX2xpdmUuc2F5KGNvdW50ID09PSAwID8gJ05vIHJlc3VsdHMgYXZhaWxhYmxlJyA6IGAke2NvdW50fSByZXN1bHQke2NvdW50ID09PSAxID8gJycgOiAncyd9IGF2YWlsYWJsZWApO1xuICAgIH0pO1xuICB9XG5cbiAgcHJpdmF0ZSBfdW5zdWJzY3JpYmVGcm9tVXNlcklucHV0KCkge1xuICAgIGlmICh0aGlzLl9zdWJzY3JpcHRpb24pIHtcbiAgICAgIHRoaXMuX3N1YnNjcmlwdGlvbi51bnN1YnNjcmliZSgpO1xuICAgIH1cbiAgICB0aGlzLl9zdWJzY3JpcHRpb24gPSBudWxsO1xuICB9XG59XG4iXX0=