import { Component, ComponentFactoryResolver, Injector, Input, OnInit, ViewChild, ViewContainerRef } from '@angular/core';
import { DashboardCard } from '../dashboard-card';

@Component({
  selector: 'app-dashboard-cards-creator',
  templateUrl: './dashboard-cards-creator.component.html',
  styleUrls: ['./dashboard-cards-creator.component.scss']
})
export class DashboardCardsCreatorComponent implements OnInit {
  @ViewChild('create', { read: ViewContainerRef }) container;

  constructor(private resolver: ComponentFactoryResolver) {
  }

  @Input() set card(data: DashboardCard) {
    if (!data) {
      return;
    }
    const inputProviders = Object.keys(data.input).map((inputName) => {
      return { provide: data.input[inputName].key, useValue: data.input[inputName].value, deps: [] };
    });
    const injector = Injector.create(inputProviders, this.container.parentInjector);
    const factory = this.resolver.resolveComponentFactory(data.component);
    const component = factory.create(injector);
    this.container.insert(component.hostView);
  }

  ngOnInit() {
  }

}
