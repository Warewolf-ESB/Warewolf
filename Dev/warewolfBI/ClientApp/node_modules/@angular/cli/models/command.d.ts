/**
 * @license
 * Copyright Google Inc. All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */
import { JsonValue, logging } from '@angular-devkit/core';
export interface CommandConstructor {
    new (context: CommandContext, logger: logging.Logger): Command;
    readonly name: string;
    aliases: string[];
    scope: CommandScope;
}
export declare enum CommandScope {
    everywhere = 0,
    inProject = 1,
    outsideProject = 2
}
export declare enum ArgumentStrategy {
    MapToOptions = 0,
    Nothing = 1
}
export declare abstract class Command<T = any> {
    protected _rawArgs: string[];
    allowMissingWorkspace: boolean;
    constructor(context: CommandContext, logger: logging.Logger);
    addOptions(options: Option[]): void;
    initializeRaw(args: string[]): Promise<any>;
    initialize(_options: any): Promise<void>;
    validate(_options: T): boolean | Promise<boolean>;
    printHelp(commandName: string, description: string, options: any): void;
    private _getArguments;
    protected printHelpUsage(name: string, options: Option[]): void;
    protected isArgument(option: Option): boolean;
    protected printHelpOptions(options: Option[]): void;
    abstract run(options: T): number | void | Promise<number | void>;
    options: Option[];
    additionalSchemas: string[];
    protected readonly logger: logging.Logger;
    protected readonly project: any;
}
export interface CommandContext {
    project: any;
}
export interface Option {
    name: string;
    description: string;
    type: string;
    default?: string | number | boolean;
    required?: boolean;
    aliases?: string[];
    format?: string;
    hidden?: boolean;
    $default?: OptionSmartDefault;
}
export interface OptionSmartDefault {
    $source: string;
    [key: string]: JsonValue;
}
