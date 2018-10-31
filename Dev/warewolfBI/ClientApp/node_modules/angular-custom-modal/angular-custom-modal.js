import { ChangeDetectorRef, Component, ContentChild, ElementRef, HostListener, Input, NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

/* tslint:disable:component-selector */
class ModalComponent {
    /**
     * @param {?} elementRef
     * @param {?} changeDetectorRef
     */
    constructor(elementRef, changeDetectorRef) {
        this.elementRef = elementRef;
        this.changeDetectorRef = changeDetectorRef;
        this.closeOnOutsideClick = true;
        this.visible = false;
        this.visibleAnimate = false;
    }
    /**
     * @return {?}
     */
    ngOnDestroy() {
        // Prevent modal from not executing its closing actions if the user navigated away (for example,
        // through a link).
        this.close();
    }
    /**
     * @return {?}
     */
    open() {
        document.body.classList.add('modal-open');
        this.visible = true;
        setTimeout(() => {
            this.visibleAnimate = true;
        });
    }
    /**
     * @return {?}
     */
    close() {
        document.body.classList.remove('modal-open');
        this.visibleAnimate = false;
        setTimeout(() => {
            this.visible = false;
            this.changeDetectorRef.markForCheck();
        }, 200);
    }
    /**
     * @param {?} event
     * @return {?}
     */
    onContainerClicked(event) {
        if (((event.target)).classList.contains('modal') && this.isTopMost() && this.closeOnOutsideClick) {
            this.close();
        }
    }
    /**
     * @param {?} event
     * @return {?}
     */
    onKeyDownHandler(event) {
        // If ESC key and TOP MOST modal, close it.
        if (event.key === 'Escape' && this.isTopMost()) {
            this.close();
        }
    }
    /**
     * Returns true if this modal is the top most modal.
     * @return {?}
     */
    isTopMost() {
        return !this.elementRef.nativeElement.querySelector(':scope modal > .modal');
    }
}
ModalComponent.decorators = [
    { type: Component, args: [{
                selector: 'modal',
                template: `
    <div 
      class="modal fade"
      role="dialog"
      tabindex="-1"
      [class.in]="visibleAnimate"
      *ngIf="visible">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <ng-container *ngTemplateOutlet="header"></ng-container>
            <button class="close" data-dismiss="modal" type="button" aria-label="Close" (click)="close()">×</button>
          </div>
          <div class="modal-body">
            <ng-container *ngTemplateOutlet="body"></ng-container>
          </div>
          <div class="modal-footer">
            <ng-container *ngTemplateOutlet="footer"></ng-container>
          </div>
        </div>
      </div>
    </div>
  `,
                styles: [`
    /**
     * A more specific selector overwrites bootstrap display properties, but they still enable users
     * to overwite them on their apps.
     */
    /deep/ modal .modal {
      display: -webkit-box;
      display: -ms-flexbox;
      display: flex;
      -webkit-box-flex: 1;
          -ms-flex: 1;
              flex: 1;
      -webkit-box-align: center;
          -ms-flex-align: center;
              align-items: center;
      -webkit-box-pack: center;
          -ms-flex-pack: center;
              justify-content: center; }

    /deep/ .modal {
      position: fixed;
      top: 0;
      left: 0;
      width: 100%;
      min-height: 100%;
      background-color: rgba(0, 0, 0, 0.15);
      z-index: 42; }

    /deep/ .modal.in {
      opacity: 1; }
  `],
            },] },
];
/**
 * @nocollapse
 */
ModalComponent.ctorParameters = () => [
    { type: ElementRef, },
    { type: ChangeDetectorRef, },
];
ModalComponent.propDecorators = {
    'header': [{ type: ContentChild, args: ['modalHeader',] },],
    'body': [{ type: ContentChild, args: ['modalBody',] },],
    'footer': [{ type: ContentChild, args: ['modalFooter',] },],
    'closeOnOutsideClick': [{ type: Input },],
    'onContainerClicked': [{ type: HostListener, args: ['click', ['$event'],] },],
    'onKeyDownHandler': [{ type: HostListener, args: ['document:keydown', ['$event'],] },],
};

class ModalModule {
}
ModalModule.decorators = [
    { type: NgModule, args: [{
                imports: [
                    CommonModule,
                ],
                exports: [ModalComponent],
                declarations: [ModalComponent],
                providers: [],
            },] },
];
/**
 * @nocollapse
 */
ModalModule.ctorParameters = () => [];

/**
 * Generated bundle index. Do not edit.
 */

export { ModalComponent, ModalModule };
//# sourceMappingURL=angular-custom-modal.js.map
