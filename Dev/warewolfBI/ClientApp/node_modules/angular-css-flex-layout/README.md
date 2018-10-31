# angular-css-flex-layout
A (temporary) solution for [@angular/flex-layout][flex] problems ([#373][issue]) in 
server-side rendering.


The awesome angular flex-layout module, currently doesn't work correctly on server platform. 
Because responsive api is built purely in js and there are inherent non-trivial problems 
in determining media size in server. While this pure js approach has [certain advantages][jslAdvantages],
one big advantage of [old css based approach][ng1flex] (with media queries) is not having
any issue with SSR.

If you are using flex-layout directives throughout your application and you want to render
your app on server **now**, and you can't wait until the SSR issue is resolved, you can
give angular-css-flex-layout a shot. Assuming the SSR problems will be fixed in the original 
repo sooner or later, angular-css-flex-layout aims to have as less footprint as
possible. Directives are implemented with selectors identical to `FlexLayoutModule` 
directives, and all they do is to add corresponding css classes of 
[AngularJS Material Flex Layout][ng1flex]. So you only need to import `CssFlexLayoutModule`
instead of `FlexLayoutModule`, and add **angular-material.layouts.css** to your styles.

# Installation
```bash
npm i angular-css-flex-layout --save
```

# Usage Example:

In your module:
```ts
import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';

// import {FlexLayoutModule} from '@angular/flex-layout';
import {CssFlexLayoutModule} from 'angular-css-flex-layout';

@NgModule({
  imports: [
    CommonModule,
    CssFlexLayoutModule // instead of FlexLayoutModule
  ]
})
export class AppModule { }
```

And in your styles (**styles.scss** if you are using angular cli):
```scss
@import '~angular-material/modules/layouts/angular-material.layouts.css';
```

If you are importing FlexLayoutModule in several places in your code, you 
can create a **flex-layout-module.ts** like below and import `FlexLayoutModule`
from it.

```ts
export {CssFlexLayoutModule as FlexLayoutModule} from 'angular-css-flex-layout';
// export {FlexLayoutModule} from '@angular/flex-layout';
```
This way, you will be able to switch between original FlexLayoutModule and
CssFlexLayoutModule with a switch of comments.

# Known issues:
- layout-wrap-* doesn't exist in AngularJS Material layout css.
- fxLayoutWrap="reverse" and fxLayoutWrap="wrap-reverse" are not supported
  in AngularJS Material layout css.
- fxShrink and fxGrow are not supported now.
- fxLayoutGap is not supported, as it's not in AngularJS Material layout css.


[flex]: https://github.com/angular/flex-layout
[issue]: https://github.com/angular/flex-layout/issues/373
[ng1flex]: https://material.angularjs.org/latest/layout/introduction
[jslAdvantages]: https://github.com/angular/flex-layout/wiki/Why-use-Flex-Layout#advantages
