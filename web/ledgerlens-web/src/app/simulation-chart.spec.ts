import { TestBed } from '@angular/core/testing';
import { SimulationChart } from './simulation-chart';

describe('SimulationChart', () => {
  it('provides an accessible chart container before data is available', async () => {
    await TestBed.configureTestingModule({ imports: [SimulationChart] }).compileComponents();
    const fixture = TestBed.createComponent(SimulationChart);
    fixture.componentInstance.description = 'Synthetic accessible simulation chart';
    fixture.detectChanges();
    const container = (fixture.nativeElement as HTMLElement).querySelector('[role="img"]');
    expect(container?.getAttribute('aria-label')).toBe('Synthetic accessible simulation chart');
    fixture.destroy();
  });
});
