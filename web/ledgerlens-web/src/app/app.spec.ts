import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideHttpClient(), provideHttpClientTesting()]
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render the portfolio ledger title', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('Portfolio ledger');
  });

  it('should keep simulation visibly separated from real activity', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const buttons = Array.from((fixture.nativeElement as HTMLElement).querySelectorAll('button'));
    const simulationButton = buttons.find(button => button.textContent?.includes('จำลอง'));
    simulationButton?.click();
    fixture.detectChanges();
    const warning = (fixture.nativeElement as HTMLElement).querySelector('.simulation-warning');
    expect(warning?.textContent).toContain('ไม่มีการส่งคำสั่งซื้อจริง');
    expect(warning?.textContent).toContain('ไม่เปลี่ยนเงินสด');
  });
});
