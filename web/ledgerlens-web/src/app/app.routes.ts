import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'identity-demo',
    loadComponent: () => import('./features/identity-access-demo/identity-access-demo-page')
      .then(module => module.IdentityAccessDemoPage)
  }
];
