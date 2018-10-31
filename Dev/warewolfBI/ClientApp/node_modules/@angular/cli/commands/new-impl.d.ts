/**
 * @license
 * Copyright Google Inc. All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */
import { SchematicCommand } from '../models/schematic-command';
export declare class NewCommand extends SchematicCommand {
    readonly allowMissingWorkspace: boolean;
    private schematicName;
    private initialized;
    initialize(options: any): Promise<void>;
    run(options: any): Promise<number | void | undefined>;
    private parseCollectionName;
    private removeLocalOptions;
}
