/**
 * @license
 * Copyright Google Inc. All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */
import { ɵSharedStylesHost as SharedStylesHost } from '@angular/platform-browser';
export declare class ServerStylesHost extends SharedStylesHost {
    private doc;
    private transitionId;
    private head;
    constructor(doc: any, transitionId: string);
    private _addStyle;
    onStylesAdded(additions: Set<string>): void;
}
