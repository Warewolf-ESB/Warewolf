/**
 * @license
 * Copyright Google Inc. All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */
import { JsonObject } from '@angular-devkit/core';
import { Option } from './command';
export declare function convertSchemaToOptions(schema: string): Promise<Option[]>;
export declare function parseSchema(schema: string): JsonObject | null;
