/**
 * @license
 * Copyright Google Inc. All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */
import { SchematicCommand } from '../models/schematic-command';
export declare class AddCommand extends SchematicCommand {
    readonly allowPrivateSchematics: boolean;
    private _parseSchematicOptions;
    validate(options: any): boolean;
    run(options: any): Promise<number | void>;
}
