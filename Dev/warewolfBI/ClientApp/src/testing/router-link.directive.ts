import { Directive, Input } from '@angular/core';
export { RouterLink} from '@angular/router';

@Directive({
  selector: '[routerLink]',
  host: { '(click)': 'onClick()' }
})
export class RouterLinkDirective {
  @Input('routerLink') linkParams: any;
  navigatedTo: any = null;

  onClick() {
    this.navigatedTo = this.linkParams;
  }
}

import { NgModule } from '@angular/core';

@NgModule({
  declarations: [
    RouterLinkDirective
  ]
})
export class RouterStubsModule {}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    
