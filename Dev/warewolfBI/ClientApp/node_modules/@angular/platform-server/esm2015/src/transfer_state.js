/**
 * @fileoverview added by tsickle
 * @suppress {checkTypes,extraRequire,uselessCode} checked by tsc
 */
/**
 * @license
 * Copyright Google Inc. All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */
import { APP_ID, NgModule } from '@angular/core';
import { DOCUMENT, TransferState, ɵescapeHtml as escapeHtml } from '@angular/platform-browser';
import { BEFORE_APP_SERIALIZED } from './tokens';
/**
 * @param {?} doc
 * @param {?} appId
 * @param {?} transferStore
 * @return {?}
 */
export function serializeTransferStateFactory(doc, appId, transferStore) {
    return () => {
        /** @type {?} */
        const script = doc.createElement('script');
        script.id = appId + '-state';
        script.setAttribute('type', 'application/json');
        script.textContent = escapeHtml(transferStore.toJson());
        doc.body.appendChild(script);
    };
}
/**
 * NgModule to install on the server side while using the `TransferState` to transfer state from
 * server to client.
 *
 * \@experimental
 */
export class ServerTransferStateModule {
}
ServerTransferStateModule.decorators = [
    { type: NgModule, args: [{
                providers: [
                    TransferState, {
                        provide: BEFORE_APP_SERIALIZED,
                        useFactory: serializeTransferStateFactory,
                        deps: [DOCUMENT, APP_ID, TransferState],
                        multi: true,
                    }
                ]
            },] }
];

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoidHJhbnNmZXJfc3RhdGUuanMiLCJzb3VyY2VSb290IjoiIiwic291cmNlcyI6WyIuLi8uLi8uLi8uLi8uLi8uLi9wYWNrYWdlcy9wbGF0Zm9ybS1zZXJ2ZXIvc3JjL3RyYW5zZmVyX3N0YXRlLnRzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiI7Ozs7Ozs7Ozs7O0FBUUEsT0FBTyxFQUFDLE1BQU0sRUFBRSxRQUFRLEVBQUMsTUFBTSxlQUFlLENBQUM7QUFDL0MsT0FBTyxFQUFDLFFBQVEsRUFBRSxhQUFhLEVBQUUsV0FBVyxJQUFJLFVBQVUsRUFBQyxNQUFNLDJCQUEyQixDQUFDO0FBRTdGLE9BQU8sRUFBQyxxQkFBcUIsRUFBQyxNQUFNLFVBQVUsQ0FBQzs7Ozs7OztBQUUvQyxNQUFNLHdDQUNGLEdBQWEsRUFBRSxLQUFhLEVBQUUsYUFBNEI7SUFDNUQsT0FBTyxHQUFHLEVBQUU7O1FBQ1YsTUFBTSxNQUFNLEdBQUcsR0FBRyxDQUFDLGFBQWEsQ0FBQyxRQUFRLENBQUMsQ0FBQztRQUMzQyxNQUFNLENBQUMsRUFBRSxHQUFHLEtBQUssR0FBRyxRQUFRLENBQUM7UUFDN0IsTUFBTSxDQUFDLFlBQVksQ0FBQyxNQUFNLEVBQUUsa0JBQWtCLENBQUMsQ0FBQztRQUNoRCxNQUFNLENBQUMsV0FBVyxHQUFHLFVBQVUsQ0FBQyxhQUFhLENBQUMsTUFBTSxFQUFFLENBQUMsQ0FBQztRQUN4RCxHQUFHLENBQUMsSUFBSSxDQUFDLFdBQVcsQ0FBQyxNQUFNLENBQUMsQ0FBQztLQUM5QixDQUFDO0NBQ0g7Ozs7Ozs7QUFrQkQsTUFBTTs7O1lBVkwsUUFBUSxTQUFDO2dCQUNSLFNBQVMsRUFBRTtvQkFDVCxhQUFhLEVBQUU7d0JBQ2IsT0FBTyxFQUFFLHFCQUFxQjt3QkFDOUIsVUFBVSxFQUFFLDZCQUE2Qjt3QkFDekMsSUFBSSxFQUFFLENBQUMsUUFBUSxFQUFFLE1BQU0sRUFBRSxhQUFhLENBQUM7d0JBQ3ZDLEtBQUssRUFBRSxJQUFJO3FCQUNaO2lCQUNGO2FBQ0YiLCJzb3VyY2VzQ29udGVudCI6WyIvKipcbiAqIEBsaWNlbnNlXG4gKiBDb3B5cmlnaHQgR29vZ2xlIEluYy4gQWxsIFJpZ2h0cyBSZXNlcnZlZC5cbiAqXG4gKiBVc2Ugb2YgdGhpcyBzb3VyY2UgY29kZSBpcyBnb3Zlcm5lZCBieSBhbiBNSVQtc3R5bGUgbGljZW5zZSB0aGF0IGNhbiBiZVxuICogZm91bmQgaW4gdGhlIExJQ0VOU0UgZmlsZSBhdCBodHRwczovL2FuZ3VsYXIuaW8vbGljZW5zZVxuICovXG5cbmltcG9ydCB7QVBQX0lELCBOZ01vZHVsZX0gZnJvbSAnQGFuZ3VsYXIvY29yZSc7XG5pbXBvcnQge0RPQ1VNRU5ULCBUcmFuc2ZlclN0YXRlLCDJtWVzY2FwZUh0bWwgYXMgZXNjYXBlSHRtbH0gZnJvbSAnQGFuZ3VsYXIvcGxhdGZvcm0tYnJvd3Nlcic7XG5cbmltcG9ydCB7QkVGT1JFX0FQUF9TRVJJQUxJWkVEfSBmcm9tICcuL3Rva2Vucyc7XG5cbmV4cG9ydCBmdW5jdGlvbiBzZXJpYWxpemVUcmFuc2ZlclN0YXRlRmFjdG9yeShcbiAgICBkb2M6IERvY3VtZW50LCBhcHBJZDogc3RyaW5nLCB0cmFuc2ZlclN0b3JlOiBUcmFuc2ZlclN0YXRlKSB7XG4gIHJldHVybiAoKSA9PiB7XG4gICAgY29uc3Qgc2NyaXB0ID0gZG9jLmNyZWF0ZUVsZW1lbnQoJ3NjcmlwdCcpO1xuICAgIHNjcmlwdC5pZCA9IGFwcElkICsgJy1zdGF0ZSc7XG4gICAgc2NyaXB0LnNldEF0dHJpYnV0ZSgndHlwZScsICdhcHBsaWNhdGlvbi9qc29uJyk7XG4gICAgc2NyaXB0LnRleHRDb250ZW50ID0gZXNjYXBlSHRtbCh0cmFuc2ZlclN0b3JlLnRvSnNvbigpKTtcbiAgICBkb2MuYm9keS5hcHBlbmRDaGlsZChzY3JpcHQpO1xuICB9O1xufVxuXG4vKipcbiAqIE5nTW9kdWxlIHRvIGluc3RhbGwgb24gdGhlIHNlcnZlciBzaWRlIHdoaWxlIHVzaW5nIHRoZSBgVHJhbnNmZXJTdGF0ZWAgdG8gdHJhbnNmZXIgc3RhdGUgZnJvbVxuICogc2VydmVyIHRvIGNsaWVudC5cbiAqXG4gKiBAZXhwZXJpbWVudGFsXG4gKi9cbkBOZ01vZHVsZSh7XG4gIHByb3ZpZGVyczogW1xuICAgIFRyYW5zZmVyU3RhdGUsIHtcbiAgICAgIHByb3ZpZGU6IEJFRk9SRV9BUFBfU0VSSUFMSVpFRCxcbiAgICAgIHVzZUZhY3Rvcnk6IHNlcmlhbGl6ZVRyYW5zZmVyU3RhdGVGYWN0b3J5LFxuICAgICAgZGVwczogW0RPQ1VNRU5ULCBBUFBfSUQsIFRyYW5zZmVyU3RhdGVdLFxuICAgICAgbXVsdGk6IHRydWUsXG4gICAgfVxuICBdXG59KVxuZXhwb3J0IGNsYXNzIFNlcnZlclRyYW5zZmVyU3RhdGVNb2R1bGUge1xufVxuIl19