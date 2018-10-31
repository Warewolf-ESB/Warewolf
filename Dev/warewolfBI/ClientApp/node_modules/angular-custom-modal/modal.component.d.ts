import { OnDestroy, ElementRef, TemplateRef, ChangeDetectorRef } from '@angular/core';
export declare class ModalComponent implements OnDestroy {
    private elementRef;
    private changeDetectorRef;
    header: TemplateRef<any>;
    body: TemplateRef<any>;
    footer: TemplateRef<any>;
    closeOnOutsideClick: boolean;
    visible: boolean;
    visibleAnimate: boolean;
    constructor(elementRef: ElementRef, changeDetectorRef: ChangeDetectorRef);
    ngOnDestroy(): void;
    open(): void;
    close(): void;
    onContainerClicked(event: MouseEvent): void;
    onKeyDownHandler(event: KeyboardEvent): void;
    /**
     * Returns true if this modal is the top most modal.
     */
    isTopMost(): boolean;
}
