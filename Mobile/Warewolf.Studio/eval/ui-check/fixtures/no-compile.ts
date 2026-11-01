import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { runWorkflow } from '@warewolf/ui';

@Component({
  selector: 'organization-ui',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <form (ngSubmit)="run()">
      <label>City <input name="city" [(ngModel)]="city" required></label>
      <label>Days <input name="days" type="number" [(ngModel)]="days"></label>
      <button type="submit">Get forecast</button>
    </form>
    @if (error) { <p class="error">{{ error }}</p> }
    @if (outputs) { <p>Summary: {{ outputs.summary }}</p> }
  `,
})
export default class ForecastUi {
  broken: = 1;
  city = '';
  days: number | null = null;
  outputs: any = null;
  error = '';

  async run() {
    try {
      this.outputs = await runWorkflow('main', { city: this.city, days: String(this.days ?? '') });
    } catch (e: any) {
      this.error = e.message;
    }
  }
}
