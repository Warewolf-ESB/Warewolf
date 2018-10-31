import { ModuleWithProviders } from '@angular/core';
export { NgbModal } from './modal';
export { NgbModalConfig, NgbModalOptions } from './modal-config';
export { NgbModalRef, NgbActiveModal } from './modal-ref';
export { ModalDismissReasons } from './modal-dismiss-reasons';
export declare class NgbModalModule {
    /**
     * Importing with '.forRoot()' is no longer necessary, you can simply import the module.
     * Will be removed in 4.0.0.
     *
     * @deprecated 3.0.0
     */
    static forRoot(): ModuleWithProviders;
}
