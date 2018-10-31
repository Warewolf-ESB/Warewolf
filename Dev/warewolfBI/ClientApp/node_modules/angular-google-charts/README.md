# angular-google-charts

> A wrapper of the Google Charts library written with Angular 6

## Usage

Import the `GoogleChartsModule` in your `app.module.ts`:

```typescript
import { GoogleChartsModule } from 'angular-google-charts';

@NgModule({
  ...
  imports: [
    ...
    GoogleChartsModule,
    ...
  ],
  ...
})
export class AppModule {}
```

And create a `google-chart` component somewhere in your application:

```html
<google-chart
  [title]="chart.title"
  [type]="chart.type"
  [data]="chart.data"
  [columnNames]="chart.columnNames"
  [options]="chart.options">
</google-chart>
```

## Install

With [npm](https://npmjs.org/) installed, run

```
$ npm install angular-google-charts
```

## Detailed Instructions

For more detailed instructions, a big readme and the source code please go to [GitHub](https://github.com/FERNman/angular-google-charts).

## License

MIT

