import { EventEmitter, Renderer2, ElementRef, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { NgbAlertConfig } from './alert-config';
/**
 * Alerts can be used to provide feedback messages.
 */
export declare class NgbAlert implements OnInit, OnChanges {
    private _renderer;
    private _element;
    /**
     * A flag indicating if a given alert can be dismissed (closed) by a user. If this flag is set, a close button (in a
     * form of an ×) will be displayed.
     */
    dismissible: boolean;
    /**
     * Alert type (CSS class). Bootstrap 4 recognizes the following types: "success", "info", "warning", "danger",
     * "primary", "secondary", "light", "dark".
     */
    type: string;
    /**
     * An event emitted when the close button is clicked. This event has no payload. Only relevant for dismissible alerts.
     */
    close: EventEmitter<void>;
    constructor(config: NgbAlertConfig, _renderer: Renderer2, _element: ElementRef);
    closeHandler(): void;
    ngOnChanges(changes: SimpleChanges): void;
    ngOnInit(): void;
}
