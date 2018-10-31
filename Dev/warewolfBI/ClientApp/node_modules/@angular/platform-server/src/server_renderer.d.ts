/**
 * @license
 * Copyright Google Inc. All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */
import { NgZone, Renderer2, RendererFactory2, RendererType2 } from '@angular/core';
import { EventManager, ɵSharedStylesHost as SharedStylesHost } from '@angular/platform-browser';
export declare class ServerRendererFactory2 implements RendererFactory2 {
    private eventManager;
    private ngZone;
    private document;
    private sharedStylesHost;
    private rendererByCompId;
    private defaultRenderer;
    private schema;
    constructor(eventManager: EventManager, ngZone: NgZone, document: any, sharedStylesHost: SharedStylesHost);
    createRenderer(element: any, type: RendererType2 | null): Renderer2;
    begin(): void;
    end(): void;
}
