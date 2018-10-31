"use strict";
/**
 * @license
 * Copyright Google Inc. All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */
Object.defineProperty(exports, "__esModule", { value: true });
const core_1 = require("@angular-devkit/core");
const command_1 = require("../models/command");
class EjectCommand extends command_1.Command {
    constructor() {
        super(...arguments);
        this.name = 'eject';
        this.description = 'Temporarily disabled. Ejects your app and output the proper '
            + 'webpack configuration and scripts.';
        this.arguments = [];
        this.options = [];
    }
    run() {
        this.logger.info(core_1.tags.stripIndents `
      The 'eject' command has been temporarily disabled, as it is not yet compatible with the new
      angular.json format. The new configuration format provides further flexibility to modify the
      configuration of your workspace without ejecting. Ejection will be re-enabled in a future
      release of the CLI.

      If you need to eject today, use CLI 1.7 to eject your project.
    `);
    }
}
EjectCommand.aliases = [];
exports.EjectCommand = EjectCommand;
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiZWplY3QtaW1wbC5qcyIsInNvdXJjZVJvb3QiOiIuLyIsInNvdXJjZXMiOlsicGFja2FnZXMvYW5ndWxhci9jbGkvY29tbWFuZHMvZWplY3QtaW1wbC50cyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiO0FBQUE7Ozs7OztHQU1HOztBQUVILCtDQUE0QztBQUM1QywrQ0FBb0Q7QUFHcEQsa0JBQTBCLFNBQVEsaUJBQU87SUFBekM7O1FBQ2tCLFNBQUksR0FBRyxPQUFPLENBQUM7UUFDZixnQkFBVyxHQUFHLDhEQUE4RDtjQUM5RCxvQ0FBb0MsQ0FBQztRQUNuRCxjQUFTLEdBQWEsRUFBRSxDQUFDO1FBQ3pCLFlBQU8sR0FBYSxFQUFFLENBQUM7SUFhekMsQ0FBQztJQVZDLEdBQUc7UUFDRCxJQUFJLENBQUMsTUFBTSxDQUFDLElBQUksQ0FBQyxXQUFJLENBQUMsWUFBWSxDQUFBOzs7Ozs7O0tBT2pDLENBQUMsQ0FBQztJQUNMLENBQUM7O0FBWGEsb0JBQU8sR0FBRyxFQUFFLENBQUM7QUFON0Isb0NBa0JDIiwic291cmNlc0NvbnRlbnQiOlsiLyoqXG4gKiBAbGljZW5zZVxuICogQ29weXJpZ2h0IEdvb2dsZSBJbmMuIEFsbCBSaWdodHMgUmVzZXJ2ZWQuXG4gKlxuICogVXNlIG9mIHRoaXMgc291cmNlIGNvZGUgaXMgZ292ZXJuZWQgYnkgYW4gTUlULXN0eWxlIGxpY2Vuc2UgdGhhdCBjYW4gYmVcbiAqIGZvdW5kIGluIHRoZSBMSUNFTlNFIGZpbGUgYXQgaHR0cHM6Ly9hbmd1bGFyLmlvL2xpY2Vuc2VcbiAqL1xuXG5pbXBvcnQgeyB0YWdzIH0gZnJvbSAnQGFuZ3VsYXItZGV2a2l0L2NvcmUnO1xuaW1wb3J0IHsgQ29tbWFuZCwgT3B0aW9uIH0gZnJvbSAnLi4vbW9kZWxzL2NvbW1hbmQnO1xuXG5cbmV4cG9ydCBjbGFzcyBFamVjdENvbW1hbmQgZXh0ZW5kcyBDb21tYW5kIHtcbiAgcHVibGljIHJlYWRvbmx5IG5hbWUgPSAnZWplY3QnO1xuICBwdWJsaWMgcmVhZG9ubHkgZGVzY3JpcHRpb24gPSAnVGVtcG9yYXJpbHkgZGlzYWJsZWQuIEVqZWN0cyB5b3VyIGFwcCBhbmQgb3V0cHV0IHRoZSBwcm9wZXIgJ1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgKyAnd2VicGFjayBjb25maWd1cmF0aW9uIGFuZCBzY3JpcHRzLic7XG4gIHB1YmxpYyByZWFkb25seSBhcmd1bWVudHM6IHN0cmluZ1tdID0gW107XG4gIHB1YmxpYyByZWFkb25seSBvcHRpb25zOiBPcHRpb25bXSA9IFtdO1xuICBwdWJsaWMgc3RhdGljIGFsaWFzZXMgPSBbXTtcblxuICBydW4oKSB7XG4gICAgdGhpcy5sb2dnZXIuaW5mbyh0YWdzLnN0cmlwSW5kZW50c2BcbiAgICAgIFRoZSAnZWplY3QnIGNvbW1hbmQgaGFzIGJlZW4gdGVtcG9yYXJpbHkgZGlzYWJsZWQsIGFzIGl0IGlzIG5vdCB5ZXQgY29tcGF0aWJsZSB3aXRoIHRoZSBuZXdcbiAgICAgIGFuZ3VsYXIuanNvbiBmb3JtYXQuIFRoZSBuZXcgY29uZmlndXJhdGlvbiBmb3JtYXQgcHJvdmlkZXMgZnVydGhlciBmbGV4aWJpbGl0eSB0byBtb2RpZnkgdGhlXG4gICAgICBjb25maWd1cmF0aW9uIG9mIHlvdXIgd29ya3NwYWNlIHdpdGhvdXQgZWplY3RpbmcuIEVqZWN0aW9uIHdpbGwgYmUgcmUtZW5hYmxlZCBpbiBhIGZ1dHVyZVxuICAgICAgcmVsZWFzZSBvZiB0aGUgQ0xJLlxuXG4gICAgICBJZiB5b3UgbmVlZCB0byBlamVjdCB0b2RheSwgdXNlIENMSSAxLjcgdG8gZWplY3QgeW91ciBwcm9qZWN0LlxuICAgIGApO1xuICB9XG59XG4iXX0=