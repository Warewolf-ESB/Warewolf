/**
 * @license
 * Copyright Google Inc. All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */
import { logging } from '@angular-devkit/core';
import { CommandContext, Option } from '../models/command';
interface CommandMap {
    [key: string]: string;
}
/**
 * Run a command.
 * @param args Raw unparsed arguments.
 * @param logger The logger to use.
 * @param context Execution context.
 */
export declare function runCommand(args: string[], logger: logging.Logger, context: CommandContext, commandMap?: CommandMap): Promise<number | void>;
export declare function parseOptions(args: string[], optionsAndArguments: Option[]): any;
export {};
