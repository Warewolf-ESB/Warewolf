/**
 * @license
 * Copyright Google Inc. All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */
import { SchematicCommand } from '../models/schematic-command';
export declare class GenerateCommand extends SchematicCommand {
    private initialized;
    initialize(options: any): Promise<void>;
    validate(options: any): boolean | Promise<boolean>;
    run(options: any): Promise<number | void> | undefined;
    private parseSchematicInfo;
    printHelp(_name: string, _description: string, options: any): void;
}
