/**
 * @license
 * Copyright Google LLC All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */
import { ModuleWithProviders, StaticProvider } from '@angular/core';
import { ModuleMap } from './module-map';
/**
 * Helper function for getting the providers object for the MODULE_MAP
 *
 * @param moduleMap Map to use as a value for MODULE_MAP
 */
export declare function provideModuleMap(moduleMap: ModuleMap): StaticProvider;
/**
 * Module for using a NgModuleFactoryLoader which does not lazy load
 */
export declare class ModuleMapLoaderModule {
    /**
     * Returns a ModuleMapLoaderModule along with a MODULE_MAP
     *
     * @param moduleMap Map to use as a value for MODULE_MAP
     */
    static withMap(moduleMap: ModuleMap): ModuleWithProviders;
}
