import { EventEmitter, OnInit, OnDestroy, Injector, Renderer2, ElementRef, TemplateRef, ViewContainerRef, ComponentFactoryResolver, NgZone } from '@angular/core';
import { Placement, PlacementArray } from '../util/positioning';
import { NgbTooltipConfig } from './tooltip-config';
export declare class NgbTooltipWindow {
    private _element;
    private _renderer;
    placement: Placement;
    id: string;
    tooltipClass: string;
    constructor(_element: ElementRef<HTMLElement>, _renderer: Renderer2);
    applyPlacement(_placement: Placement): void;
    /**
     * Tells whether the event has been triggered from this component's subtree or not.
     *
     * @param event the event to check
     *
     * @return whether the event has been triggered from this component's subtree or not.
     */
    isEventFrom(event: Event): boolean;
}
/**
 * A lightweight, extensible directive for fancy tooltip creation.
 */
export declare class NgbTooltip implements OnInit, OnDestroy {
    private _elementRef;
    private _renderer;
    private _ngZone;
    private _document;
    /**
     * Indicates whether the tooltip should be closed on Escape key and inside/outside clicks.
     *
     * - true (default): closes on both outside and inside clicks as well as Escape presses
     * - false: disables the autoClose feature (NB: triggers still apply)
     * - 'inside': closes on inside clicks as well as Escape presses
     * - 'outside': closes on outside clicks (sometimes also achievable through triggers)
     * as well as Escape presses
     *
     * @since 3.0.0
     */
    autoClose: boolean | 'inside' | 'outside';
    /**
      * Placement of a tooltip accepts:
      *    "top", "top-left", "top-right", "bottom", "bottom-left", "bottom-right",
      *    "left", "left-top", "left-bottom", "right", "right-top", "right-bottom"
      * and array of above values.
      */
    placement: PlacementArray;
    /**
     * Specifies events that should trigger. Supports a space separated list of event names.
     */
    triggers: string;
    /**
     * A selector specifying the element the tooltip should be appended to.
     * Currently only supports "body".
     */
    container: string;
    /**
     * A flag indicating if a given tooltip is disabled and should not be displayed.
     *
     * @since 1.1.0
     */
    disableTooltip: boolean;
    /**
     * An optional class applied to ngb-tooltip-window
     *
     * @since 3.2.0
     */
    tooltipClass: string;
    /**
     * Emits an event when the tooltip is shown
     */
    shown: EventEmitter<{}>;
    /**
     * Emits an event when the tooltip is hidden
     */
    hidden: EventEmitter<{}>;
    private _ngbTooltip;
    private _ngbTooltipWindowId;
    private _popupService;
    private _windowRef;
    private _unregisterListenersFn;
    private _zoneSubscription;
    constructor(_elementRef: ElementRef<HTMLElement>, _renderer: Renderer2, injector: Injector, componentFactoryResolver: ComponentFactoryResolver, viewContainerRef: ViewContainerRef, config: NgbTooltipConfig, _ngZone: NgZone, _document: any);
    /**
     * Content to be displayed as tooltip. If falsy, the tooltip won't open.
     */
    ngbTooltip: string | TemplateRef<any>;
    /**
     * Opens an element’s tooltip. This is considered a “manual” triggering of the tooltip.
     * The context is an optional value to be injected into the tooltip template when it is created.
     */
    open(context?: any): void;
    /**
     * Closes an element’s tooltip. This is considered a “manual” triggering of the tooltip.
     */
    close(): void;
    /**
     * Toggles an element’s tooltip. This is considered a “manual” triggering of the tooltip.
     */
    toggle(): void;
    /**
     * Returns whether or not the tooltip is currently being shown
     */
    isOpen(): boolean;
    ngOnInit(): void;
    ngOnDestroy(): void;
    private _shouldCloseFromClick(event);
    private _isEventFromTooltip(event);
}
