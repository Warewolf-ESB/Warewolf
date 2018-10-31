/**
 * @license
 * Copyright Google Inc. All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */
import { Command } from '../models/command';
export declare class HelpCommand extends Command {
    run(options: any): void;
    printHelp(_commandName: string, _description: string, options: any): void;
}
