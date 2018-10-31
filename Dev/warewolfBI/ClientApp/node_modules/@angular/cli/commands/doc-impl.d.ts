/**
 * @license
 * Copyright Google Inc. All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */
import { Command } from '../models/command';
export interface Options {
    keyword: string;
    search?: boolean;
}
export declare class DocCommand extends Command {
    validate(options: Options): boolean;
    run(options: Options): Promise<any>;
}
