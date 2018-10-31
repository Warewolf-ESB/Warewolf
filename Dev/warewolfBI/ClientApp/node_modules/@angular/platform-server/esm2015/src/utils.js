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
import { ApplicationRef } from '@angular/core';
import { ɵTRANSITION_ID } from '@angular/platform-browser';
import { first } from 'rxjs/operators';
import { PlatformState } from './platform_state';
import { platformDynamicServer, platformServer } from './server';
import { BEFORE_APP_SERIALIZED, INITIAL_CONFIG } from './tokens';
/**
 * @record
 */
function PlatformOptions() { }
/** @type {?|undefined} */
PlatformOptions.prototype.document;
/** @type {?|undefined} */
PlatformOptions.prototype.url;
/** @type {?|undefined} */
PlatformOptions.prototype.extraProviders;
/**
 * @param {?} platformFactory
 * @param {?} options
 * @return {?}
 */
function _getPlatform(platformFactory, options) {
    /** @type {?} */
    const extraProviders = options.extraProviders ? options.extraProviders : [];
    return platformFactory([
        { provide: INITIAL_CONFIG, useValue: { document: options.document, url: options.url } },
        extraProviders
    ]);
}
/**
 * @template T
 * @param {?} platform
 * @param {?} moduleRefPromise
 * @return {?}
 */
function _render(platform, moduleRefPromise) {
    return moduleRefPromise.then((moduleRef) => {
        /** @type {?} */
        const transitionId = moduleRef.injector.get(ɵTRANSITION_ID, null);
        if (!transitionId) {
            throw new Error(`renderModule[Factory]() requires the use of BrowserModule.withServerTransition() to ensure
the server-rendered app can be properly bootstrapped into a client app.`);
        }
        /** @type {?} */
        const applicationRef = moduleRef.injector.get(ApplicationRef);
        return applicationRef.isStable.pipe((first((isStable) => isStable)))
            .toPromise()
            .then(() => {
            /** @type {?} */
            const platformState = platform.injector.get(PlatformState);
            /** @type {?} */
            const callbacks = moduleRef.injector.get(BEFORE_APP_SERIALIZED, null);
            if (callbacks) {
                for (const callback of callbacks) {
                    try {
                        callback();
                    }
                    catch (e) {
                        // Ignore exceptions.
                        console.warn('Ignoring BEFORE_APP_SERIALIZED Exception: ', e);
                    }
                }
            }
            /** @type {?} */
            const output = platformState.renderToString();
            platform.destroy();
            return output;
        });
    });
}
/**
 * Renders a Module to string.
 *
 * `document` is the full document HTML of the page to render, as a string.
 * `url` is the URL for the current render request.
 * `extraProviders` are the platform level providers for the current render request.
 *
 * Do not use this in a production server environment. Use pre-compiled {\@link NgModuleFactory} with
 * {\@link renderModuleFactory} instead.
 *
 * \@experimental
 * @template T
 * @param {?} module
 * @param {?} options
 * @return {?}
 */
export function renderModule(module, options) {
    /** @type {?} */
    const platform = _getPlatform(platformDynamicServer, options);
    return _render(platform, platform.bootstrapModule(module));
}
/**
 * Renders a {\@link NgModuleFactory} to string.
 *
 * `document` is the full document HTML of the page to render, as a string.
 * `url` is the URL for the current render request.
 * `extraProviders` are the platform level providers for the current render request.
 *
 * \@experimental
 * @template T
 * @param {?} moduleFactory
 * @param {?} options
 * @return {?}
 */
export function renderModuleFactory(moduleFactory, options) {
    /** @type {?} */
    const platform = _getPlatform(platformServer, options);
    return _render(platform, platform.bootstrapModuleFactory(moduleFactory));
}

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoidXRpbHMuanMiLCJzb3VyY2VSb290IjoiIiwic291cmNlcyI6WyIuLi8uLi8uLi8uLi8uLi8uLi9wYWNrYWdlcy9wbGF0Zm9ybS1zZXJ2ZXIvc3JjL3V0aWxzLnRzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiI7Ozs7Ozs7Ozs7O0FBUUEsT0FBTyxFQUFDLGNBQWMsRUFBa0UsTUFBTSxlQUFlLENBQUM7QUFDOUcsT0FBTyxFQUFDLGNBQWMsRUFBQyxNQUFNLDJCQUEyQixDQUFDO0FBQ3pELE9BQU8sRUFBQyxLQUFLLEVBQUMsTUFBTSxnQkFBZ0IsQ0FBQztBQUVyQyxPQUFPLEVBQUMsYUFBYSxFQUFDLE1BQU0sa0JBQWtCLENBQUM7QUFDL0MsT0FBTyxFQUFDLHFCQUFxQixFQUFFLGNBQWMsRUFBQyxNQUFNLFVBQVUsQ0FBQztBQUMvRCxPQUFPLEVBQUMscUJBQXFCLEVBQUUsY0FBYyxFQUFDLE1BQU0sVUFBVSxDQUFDOzs7Ozs7Ozs7Ozs7Ozs7O0FBUS9ELHNCQUNJLGVBQWtFLEVBQ2xFLE9BQXdCOztJQUMxQixNQUFNLGNBQWMsR0FBRyxPQUFPLENBQUMsY0FBYyxDQUFDLENBQUMsQ0FBQyxPQUFPLENBQUMsY0FBYyxDQUFDLENBQUMsQ0FBQyxFQUFFLENBQUM7SUFDNUUsT0FBTyxlQUFlLENBQUM7UUFDckIsRUFBQyxPQUFPLEVBQUUsY0FBYyxFQUFFLFFBQVEsRUFBRSxFQUFDLFFBQVEsRUFBRSxPQUFPLENBQUMsUUFBUSxFQUFFLEdBQUcsRUFBRSxPQUFPLENBQUMsR0FBRyxFQUFDLEVBQUM7UUFDbkYsY0FBYztLQUNmLENBQUMsQ0FBQztDQUNKOzs7Ozs7O0FBRUQsaUJBQ0ksUUFBcUIsRUFBRSxnQkFBeUM7SUFDbEUsT0FBTyxnQkFBZ0IsQ0FBQyxJQUFJLENBQUMsQ0FBQyxTQUFTLEVBQUUsRUFBRTs7UUFDekMsTUFBTSxZQUFZLEdBQUcsU0FBUyxDQUFDLFFBQVEsQ0FBQyxHQUFHLENBQUMsY0FBYyxFQUFFLElBQUksQ0FBQyxDQUFDO1FBQ2xFLElBQUksQ0FBQyxZQUFZLEVBQUU7WUFDakIsTUFBTSxJQUFJLEtBQUssQ0FDWDt3RUFDOEQsQ0FBQyxDQUFDO1NBQ3JFOztRQUNELE1BQU0sY0FBYyxHQUFtQixTQUFTLENBQUMsUUFBUSxDQUFDLEdBQUcsQ0FBQyxjQUFjLENBQUMsQ0FBQztRQUM5RSxPQUFPLGNBQWMsQ0FBQyxRQUFRLENBQUMsSUFBSSxDQUFDLENBQUMsS0FBSyxDQUFDLENBQUMsUUFBaUIsRUFBRSxFQUFFLENBQUMsUUFBUSxDQUFDLENBQUMsQ0FBQzthQUN4RSxTQUFTLEVBQUU7YUFDWCxJQUFJLENBQUMsR0FBRyxFQUFFOztZQUNULE1BQU0sYUFBYSxHQUFHLFFBQVEsQ0FBQyxRQUFRLENBQUMsR0FBRyxDQUFDLGFBQWEsQ0FBQyxDQUFDOztZQUczRCxNQUFNLFNBQVMsR0FBRyxTQUFTLENBQUMsUUFBUSxDQUFDLEdBQUcsQ0FBQyxxQkFBcUIsRUFBRSxJQUFJLENBQUMsQ0FBQztZQUN0RSxJQUFJLFNBQVMsRUFBRTtnQkFDYixLQUFLLE1BQU0sUUFBUSxJQUFJLFNBQVMsRUFBRTtvQkFDaEMsSUFBSTt3QkFDRixRQUFRLEVBQUUsQ0FBQztxQkFDWjtvQkFBQyxPQUFPLENBQUMsRUFBRTs7d0JBRVYsT0FBTyxDQUFDLElBQUksQ0FBQyw0Q0FBNEMsRUFBRSxDQUFDLENBQUMsQ0FBQztxQkFDL0Q7aUJBQ0Y7YUFDRjs7WUFFRCxNQUFNLE1BQU0sR0FBRyxhQUFhLENBQUMsY0FBYyxFQUFFLENBQUM7WUFDOUMsUUFBUSxDQUFDLE9BQU8sRUFBRSxDQUFDO1lBQ25CLE9BQU8sTUFBTSxDQUFDO1NBQ2YsQ0FBQyxDQUFDO0tBQ1IsQ0FBQyxDQUFDO0NBQ0o7Ozs7Ozs7Ozs7Ozs7Ozs7O0FBY0QsTUFBTSx1QkFDRixNQUFlLEVBQUUsT0FBNkU7O0lBRWhHLE1BQU0sUUFBUSxHQUFHLFlBQVksQ0FBQyxxQkFBcUIsRUFBRSxPQUFPLENBQUMsQ0FBQztJQUM5RCxPQUFPLE9BQU8sQ0FBQyxRQUFRLEVBQUUsUUFBUSxDQUFDLGVBQWUsQ0FBQyxNQUFNLENBQUMsQ0FBQyxDQUFDO0NBQzVEOzs7Ozs7Ozs7Ozs7OztBQVdELE1BQU0sOEJBQ0YsYUFBaUMsRUFDakMsT0FBNkU7O0lBRS9FLE1BQU0sUUFBUSxHQUFHLFlBQVksQ0FBQyxjQUFjLEVBQUUsT0FBTyxDQUFDLENBQUM7SUFDdkQsT0FBTyxPQUFPLENBQUMsUUFBUSxFQUFFLFFBQVEsQ0FBQyxzQkFBc0IsQ0FBQyxhQUFhLENBQUMsQ0FBQyxDQUFDO0NBQzFFIiwic291cmNlc0NvbnRlbnQiOlsiLyoqXG4gKiBAbGljZW5zZVxuICogQ29weXJpZ2h0IEdvb2dsZSBJbmMuIEFsbCBSaWdodHMgUmVzZXJ2ZWQuXG4gKlxuICogVXNlIG9mIHRoaXMgc291cmNlIGNvZGUgaXMgZ292ZXJuZWQgYnkgYW4gTUlULXN0eWxlIGxpY2Vuc2UgdGhhdCBjYW4gYmVcbiAqIGZvdW5kIGluIHRoZSBMSUNFTlNFIGZpbGUgYXQgaHR0cHM6Ly9hbmd1bGFyLmlvL2xpY2Vuc2VcbiAqL1xuXG5pbXBvcnQge0FwcGxpY2F0aW9uUmVmLCBOZ01vZHVsZUZhY3RvcnksIE5nTW9kdWxlUmVmLCBQbGF0Zm9ybVJlZiwgU3RhdGljUHJvdmlkZXIsIFR5cGV9IGZyb20gJ0Bhbmd1bGFyL2NvcmUnO1xuaW1wb3J0IHvJtVRSQU5TSVRJT05fSUR9IGZyb20gJ0Bhbmd1bGFyL3BsYXRmb3JtLWJyb3dzZXInO1xuaW1wb3J0IHtmaXJzdH0gZnJvbSAncnhqcy9vcGVyYXRvcnMnO1xuXG5pbXBvcnQge1BsYXRmb3JtU3RhdGV9IGZyb20gJy4vcGxhdGZvcm1fc3RhdGUnO1xuaW1wb3J0IHtwbGF0Zm9ybUR5bmFtaWNTZXJ2ZXIsIHBsYXRmb3JtU2VydmVyfSBmcm9tICcuL3NlcnZlcic7XG5pbXBvcnQge0JFRk9SRV9BUFBfU0VSSUFMSVpFRCwgSU5JVElBTF9DT05GSUd9IGZyb20gJy4vdG9rZW5zJztcblxuaW50ZXJmYWNlIFBsYXRmb3JtT3B0aW9ucyB7XG4gIGRvY3VtZW50Pzogc3RyaW5nO1xuICB1cmw/OiBzdHJpbmc7XG4gIGV4dHJhUHJvdmlkZXJzPzogU3RhdGljUHJvdmlkZXJbXTtcbn1cblxuZnVuY3Rpb24gX2dldFBsYXRmb3JtKFxuICAgIHBsYXRmb3JtRmFjdG9yeTogKGV4dHJhUHJvdmlkZXJzOiBTdGF0aWNQcm92aWRlcltdKSA9PiBQbGF0Zm9ybVJlZixcbiAgICBvcHRpb25zOiBQbGF0Zm9ybU9wdGlvbnMpOiBQbGF0Zm9ybVJlZiB7XG4gIGNvbnN0IGV4dHJhUHJvdmlkZXJzID0gb3B0aW9ucy5leHRyYVByb3ZpZGVycyA/IG9wdGlvbnMuZXh0cmFQcm92aWRlcnMgOiBbXTtcbiAgcmV0dXJuIHBsYXRmb3JtRmFjdG9yeShbXG4gICAge3Byb3ZpZGU6IElOSVRJQUxfQ09ORklHLCB1c2VWYWx1ZToge2RvY3VtZW50OiBvcHRpb25zLmRvY3VtZW50LCB1cmw6IG9wdGlvbnMudXJsfX0sXG4gICAgZXh0cmFQcm92aWRlcnNcbiAgXSk7XG59XG5cbmZ1bmN0aW9uIF9yZW5kZXI8VD4oXG4gICAgcGxhdGZvcm06IFBsYXRmb3JtUmVmLCBtb2R1bGVSZWZQcm9taXNlOiBQcm9taXNlPE5nTW9kdWxlUmVmPFQ+Pik6IFByb21pc2U8c3RyaW5nPiB7XG4gIHJldHVybiBtb2R1bGVSZWZQcm9taXNlLnRoZW4oKG1vZHVsZVJlZikgPT4ge1xuICAgIGNvbnN0IHRyYW5zaXRpb25JZCA9IG1vZHVsZVJlZi5pbmplY3Rvci5nZXQoybVUUkFOU0lUSU9OX0lELCBudWxsKTtcbiAgICBpZiAoIXRyYW5zaXRpb25JZCkge1xuICAgICAgdGhyb3cgbmV3IEVycm9yKFxuICAgICAgICAgIGByZW5kZXJNb2R1bGVbRmFjdG9yeV0oKSByZXF1aXJlcyB0aGUgdXNlIG9mIEJyb3dzZXJNb2R1bGUud2l0aFNlcnZlclRyYW5zaXRpb24oKSB0byBlbnN1cmVcbnRoZSBzZXJ2ZXItcmVuZGVyZWQgYXBwIGNhbiBiZSBwcm9wZXJseSBib290c3RyYXBwZWQgaW50byBhIGNsaWVudCBhcHAuYCk7XG4gICAgfVxuICAgIGNvbnN0IGFwcGxpY2F0aW9uUmVmOiBBcHBsaWNhdGlvblJlZiA9IG1vZHVsZVJlZi5pbmplY3Rvci5nZXQoQXBwbGljYXRpb25SZWYpO1xuICAgIHJldHVybiBhcHBsaWNhdGlvblJlZi5pc1N0YWJsZS5waXBlKChmaXJzdCgoaXNTdGFibGU6IGJvb2xlYW4pID0+IGlzU3RhYmxlKSkpXG4gICAgICAgIC50b1Byb21pc2UoKVxuICAgICAgICAudGhlbigoKSA9PiB7XG4gICAgICAgICAgY29uc3QgcGxhdGZvcm1TdGF0ZSA9IHBsYXRmb3JtLmluamVjdG9yLmdldChQbGF0Zm9ybVN0YXRlKTtcblxuICAgICAgICAgIC8vIFJ1biBhbnkgQkVGT1JFX0FQUF9TRVJJQUxJWkVEIGNhbGxiYWNrcyBqdXN0IGJlZm9yZSByZW5kZXJpbmcgdG8gc3RyaW5nLlxuICAgICAgICAgIGNvbnN0IGNhbGxiYWNrcyA9IG1vZHVsZVJlZi5pbmplY3Rvci5nZXQoQkVGT1JFX0FQUF9TRVJJQUxJWkVELCBudWxsKTtcbiAgICAgICAgICBpZiAoY2FsbGJhY2tzKSB7XG4gICAgICAgICAgICBmb3IgKGNvbnN0IGNhbGxiYWNrIG9mIGNhbGxiYWNrcykge1xuICAgICAgICAgICAgICB0cnkge1xuICAgICAgICAgICAgICAgIGNhbGxiYWNrKCk7XG4gICAgICAgICAgICAgIH0gY2F0Y2ggKGUpIHtcbiAgICAgICAgICAgICAgICAvLyBJZ25vcmUgZXhjZXB0aW9ucy5cbiAgICAgICAgICAgICAgICBjb25zb2xlLndhcm4oJ0lnbm9yaW5nIEJFRk9SRV9BUFBfU0VSSUFMSVpFRCBFeGNlcHRpb246ICcsIGUpO1xuICAgICAgICAgICAgICB9XG4gICAgICAgICAgICB9XG4gICAgICAgICAgfVxuXG4gICAgICAgICAgY29uc3Qgb3V0cHV0ID0gcGxhdGZvcm1TdGF0ZS5yZW5kZXJUb1N0cmluZygpO1xuICAgICAgICAgIHBsYXRmb3JtLmRlc3Ryb3koKTtcbiAgICAgICAgICByZXR1cm4gb3V0cHV0O1xuICAgICAgICB9KTtcbiAgfSk7XG59XG5cbi8qKlxuICogUmVuZGVycyBhIE1vZHVsZSB0byBzdHJpbmcuXG4gKlxuICogYGRvY3VtZW50YCBpcyB0aGUgZnVsbCBkb2N1bWVudCBIVE1MIG9mIHRoZSBwYWdlIHRvIHJlbmRlciwgYXMgYSBzdHJpbmcuXG4gKiBgdXJsYCBpcyB0aGUgVVJMIGZvciB0aGUgY3VycmVudCByZW5kZXIgcmVxdWVzdC5cbiAqIGBleHRyYVByb3ZpZGVyc2AgYXJlIHRoZSBwbGF0Zm9ybSBsZXZlbCBwcm92aWRlcnMgZm9yIHRoZSBjdXJyZW50IHJlbmRlciByZXF1ZXN0LlxuICpcbiAqIERvIG5vdCB1c2UgdGhpcyBpbiBhIHByb2R1Y3Rpb24gc2VydmVyIGVudmlyb25tZW50LiBVc2UgcHJlLWNvbXBpbGVkIHtAbGluayBOZ01vZHVsZUZhY3Rvcnl9IHdpdGhcbiAqIHtAbGluayByZW5kZXJNb2R1bGVGYWN0b3J5fSBpbnN0ZWFkLlxuICpcbiAqIEBleHBlcmltZW50YWxcbiAqL1xuZXhwb3J0IGZ1bmN0aW9uIHJlbmRlck1vZHVsZTxUPihcbiAgICBtb2R1bGU6IFR5cGU8VD4sIG9wdGlvbnM6IHtkb2N1bWVudD86IHN0cmluZywgdXJsPzogc3RyaW5nLCBleHRyYVByb3ZpZGVycz86IFN0YXRpY1Byb3ZpZGVyW119KTpcbiAgICBQcm9taXNlPHN0cmluZz4ge1xuICBjb25zdCBwbGF0Zm9ybSA9IF9nZXRQbGF0Zm9ybShwbGF0Zm9ybUR5bmFtaWNTZXJ2ZXIsIG9wdGlvbnMpO1xuICByZXR1cm4gX3JlbmRlcihwbGF0Zm9ybSwgcGxhdGZvcm0uYm9vdHN0cmFwTW9kdWxlKG1vZHVsZSkpO1xufVxuXG4vKipcbiAqIFJlbmRlcnMgYSB7QGxpbmsgTmdNb2R1bGVGYWN0b3J5fSB0byBzdHJpbmcuXG4gKlxuICogYGRvY3VtZW50YCBpcyB0aGUgZnVsbCBkb2N1bWVudCBIVE1MIG9mIHRoZSBwYWdlIHRvIHJlbmRlciwgYXMgYSBzdHJpbmcuXG4gKiBgdXJsYCBpcyB0aGUgVVJMIGZvciB0aGUgY3VycmVudCByZW5kZXIgcmVxdWVzdC5cbiAqIGBleHRyYVByb3ZpZGVyc2AgYXJlIHRoZSBwbGF0Zm9ybSBsZXZlbCBwcm92aWRlcnMgZm9yIHRoZSBjdXJyZW50IHJlbmRlciByZXF1ZXN0LlxuICpcbiAqIEBleHBlcmltZW50YWxcbiAqL1xuZXhwb3J0IGZ1bmN0aW9uIHJlbmRlck1vZHVsZUZhY3Rvcnk8VD4oXG4gICAgbW9kdWxlRmFjdG9yeTogTmdNb2R1bGVGYWN0b3J5PFQ+LFxuICAgIG9wdGlvbnM6IHtkb2N1bWVudD86IHN0cmluZywgdXJsPzogc3RyaW5nLCBleHRyYVByb3ZpZGVycz86IFN0YXRpY1Byb3ZpZGVyW119KTpcbiAgICBQcm9taXNlPHN0cmluZz4ge1xuICBjb25zdCBwbGF0Zm9ybSA9IF9nZXRQbGF0Zm9ybShwbGF0Zm9ybVNlcnZlciwgb3B0aW9ucyk7XG4gIHJldHVybiBfcmVuZGVyKHBsYXRmb3JtLCBwbGF0Zm9ybS5ib290c3RyYXBNb2R1bGVGYWN0b3J5KG1vZHVsZUZhY3RvcnkpKTtcbn1cbiJdfQ==