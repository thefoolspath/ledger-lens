import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { apiProblemMessage } from '../../api-response';
import { IdentityAccessDemoApi } from './identity-access-demo.api';
import { DemoUser } from './identity-access-demo.models';

@Component({
  selector: 'll-identity-access-demo-page',
  imports: [FormsModule],
  providers: [IdentityAccessDemoApi],
  templateUrl: './identity-access-demo-page.html',
  styleUrl: './identity-access-demo-page.scss'
})
export class IdentityAccessDemoPage {
  private readonly api = inject(IdentityAccessDemoApi);
  protected readonly selectedUser = signal<DemoUser | null>(null);
  protected readonly message = signal('');
  protected readonly pending = signal(false);
  protected email = '';
  protected displayName = '';
  protected userId = '';

  protected create(): void {
    if (this.pending()) return;
    this.pending.set(true);
    this.message.set('');
    this.api.create({ email: this.email, displayName: this.displayName }).subscribe({
      next: user => {
        this.selectedUser.set(user);
        this.userId = user.id;
        this.message.set('User saved to the IdentityAccess database. Use the ID to query it.');
        this.pending.set(false);
      },
      error: error => {
        this.selectedUser.set(null);
        this.message.set(apiProblemMessage(error, 'The user could not be created.'));
        this.pending.set(false);
      }
    });
  }

  protected getOne(): void {
    if (this.pending()) return;
    this.pending.set(true);
    this.message.set('');
    this.api.getOne(this.userId).subscribe({
      next: user => {
        this.selectedUser.set(user);
        this.message.set('User loaded from the IdentityAccess database.');
        this.pending.set(false);
      },
      error: error => {
        this.selectedUser.set(null);
        this.message.set(apiProblemMessage(error, 'The user could not be found.'));
        this.pending.set(false);
      }
    });
  }
}
