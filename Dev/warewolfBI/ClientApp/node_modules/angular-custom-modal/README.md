# Angular Custom Modal

[![npm Version](https://img.shields.io/npm/v/angular-custom-modal.svg)](https://www.npmjs.com/package/angular-custom-modal)
[![Build Status](https://travis-ci.org/zurfyx/angular-custom-modal.svg?branch=master)](https://travis-ci.org/zurfyx/angular-custom-modal)

> Angular2+ Modal / Dialog (with inner component support).

A continuation of https://stackoverflow.com/a/46949848

## Demo

[zurfyx.github.io/angular-custom-modal](https://zurfyx.github.io/angular-custom-modal/)

## Install

```
npm install angular-custom-modal
```

## Features

Core:

- Light: no CSS / JS frameworks attached
- [Bootstrap 3 & 4 CSS compatible](#bootstrap)
- Custom modal header, body and header
- [Modal stacking support](#nested-modal)
- Lazy inner component initialization, and `ngOnDestroy`(ed) when modal is closed

Minor:

- Optional CSS animations
- Optional parent scrolling when modal is visible
- Escape or button to close modal

## Usage

app.module.ts

```
imports: [
  ...
  ModalModule,
  ...
],
...
})
```

### Raw HTML

app.component.html

```
<button (click)="htmlInsideModal.open()">Raw HTML inside modal</button>
<modal #htmlInsideModal>
  <ng-template #modalHeader><h2>HTML inside modal</h2></ng-template>
  <ng-template #modalBody>
    <p>HTML content inside modal</p>
  </ng-template>
</modal>
```

### Component inside Modal

my-component.component.ts

```
@Component({
  selector: 'app-my-component',
  templateUrl: 'my-component.component.html',
})
export class AppModalContentComponent { }
```

my-component.component.html

```
<p>My component's HTML</p>
```

app.component.html

```
<button (click)="componentInsideModal.open()">Component inside modal</button>
<modal #componentInsideModal>
  <ng-template #modalHeader><h2>Component inside modal</h2></ng-template>
  <ng-template #modalBody>
    <app-my-component></app-my-component>
  </ng-template>
  <ng-template #modalFooter></ng-template>
</modal>
```

### Nested Modal

app.component.html

```
<modal #nestedModal>
  <ng-template #modalHeader><h2>Nested modal</h2></ng-template>
  <ng-template #modalBody>
    Nested modals can be created by simply adding a &lt;modal&gt; inside a &lt;modal&gt;
    ...
    <button (click)="nestedModalX.open()">Open nested modal</button>
    <modal #nestedModalX>
      <ng-template #modalBody>This is the nested modal content.</ng-template>
    </modal>
  </ng-template>
</modal>
```

See [example source code](https://github.com/zurfyx/angular-custom-modal/tree/master/example/app) for more information.

**Why ng-template?**

ng-template prevents the parent component from initializing the component. Only when the modal library finds it convenient the component will be initialize and visible to the user. Hence, it preserves the natural `ngOnInit()` and `ngOnDestroy()` that we expect.

Similar libraries which make use of `<ng-content>` and its [content transclution strategy](https://scotch.io/tutorials/angular-2-transclusion-using-ng-content#toc-multi-slot-transclusion), do not prevent the component from initializing, but rather just hide it. The component was already initialized in the parent component.

References:<br>
https://angular.io/api/common/NgTemplateOutlet<br>
https://blog.angular-university.io/angular-ng-template-ng-container-ngtemplateoutlet/<br>
https://medium.com/claritydesignsystem/ng-content-the-hidden-docs-96a29d70d11b<br>
https://netbasal.com/understanding-viewchildren-contentchildren-and-querylist-in-angular-896b0c689f6e<br>

## Styles

The library carries the minimum generic styles. Beautifying it is up to you.

### Default styles

You can find the demo copy-paste styles in [modal.css](https://github.com/zurfyx/angular-custom-modal/blob/master/example/app/modal.css).

### Bootstrap

Bootstrap users require no additional CSS other than the Bootstrap library (either version 3 or 4).

## API

### ModalComponent

| Name | Type | Description |
| ----| ----- | ----------- |
| header | @ContentChild('modalHeader'): TemplateRef<any> | Angular Template (`<ng-template>`) to use as header. |
| body | @ContentChild('modalBody'): TemplateRef<any> | Angular Template (`ng-template`) to use as body. |
| footer | @ContentChild('modalFooter'): TemplateRef<any> | Angular Template (`ng-template`) to use as footer. |
| closeOnOutsideClick | @Input(): boolean = true | When set to `true` modal will close when a click is performed outside the modal container. |
open | () => void | Opens the modal.
| close | () => void | Closes the modal. |
| isTopMost | () => boolean | Returns true is the modal is the top most modal, or false if it is underneath another modal. |

## Special thanks

To [Stephen Paul](https://stackoverflow.com/users/1087131/stephen-paul) for the [initial Angular 2 Modal version](https://stackoverflow.com/a/40144809/2013580).

## License

MIT © [Gerard Rovira Sánchez](//zurfyx.com)