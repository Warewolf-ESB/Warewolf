import { AfterViewInit, ElementRef, EventEmitter, OnDestroy, OnInit } from '@angular/core';
export declare class NgbModalWindow implements OnInit, AfterViewInit, OnDestroy {
    private _document;
    private _elRef;
    private _elWithFocus;
    ariaLabelledBy: string;
    backdrop: boolean | string;
    centered: string;
    keyboard: boolean;
    size: string;
    windowClass: string;
    dismissEvent: EventEmitter<{}>;
    constructor(_document: any, _elRef: ElementRef<HTMLElement>);
    backdropClick($event: any): void;
    escKey($event: any): void;
    dismiss(reason: any): void;
    ngOnInit(): void;
    ngAfterViewInit(): void;
    ngOnDestroy(): void;
}
