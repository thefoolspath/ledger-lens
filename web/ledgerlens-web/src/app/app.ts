import { HttpClient } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';

type ConnectivityState = 'checking' | 'connected' | 'unavailable';

interface ServiceStatus {
  readonly name: string;
  readonly endpoint: string;
  readonly state: ConnectivityState;
}

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  private readonly http = inject(HttpClient);
  protected readonly services = signal<readonly ServiceStatus[]>([
    { name: 'Portfolio Core', endpoint: '/api/portfolio/internal/ping', state: 'checking' },
    { name: 'Market Data', endpoint: '/api/market/internal/ping', state: 'checking' },
    { name: 'Slip Import', endpoint: '/api/slips/internal/ping', state: 'checking' },
    { name: 'Research', endpoint: '/api/research/internal/ping', state: 'checking' },
    { name: 'Operations', endpoint: '/api/operations/internal/ping', state: 'checking' }
  ]);

  public constructor() {
    this.refresh();
  }

  protected refresh(): void {
    this.services.update(services => services.map(service => ({ ...service, state: 'checking' })));
    for (const service of this.services()) {
      this.http.get(service.endpoint).subscribe({
        next: () => this.setState(service.endpoint, 'connected'),
        error: () => this.setState(service.endpoint, 'unavailable')
      });
    }
  }

  private setState(endpoint: string, state: ConnectivityState): void {
    this.services.update(services => services.map(service =>
      service.endpoint === endpoint ? { ...service, state } : service));
  }
}
