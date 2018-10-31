/**
 * @license
 * Copyright Google Inc. All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */
import { Command } from '../models/command';
export interface ConfigOptions {
    jsonPath: string;
    value?: string;
    global?: boolean;
}
export declare class ConfigCommand extends Command {
    run(options: ConfigOptions): 1 | undefined;
    private get;
    private set;
}
