import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { IdentityAccessDemoApi } from './identity-access-demo.api';
import { DemoUser } from './identity-access-demo.models';

describe('IdentityAccess demo API', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [IdentityAccessDemoApi, provideHttpClient(), provideHttpClientTesting()]
    });
  });

  it('sends UsersCreate to the IdentityAccess backend and reads the response envelope', () => {
    const api = TestBed.inject(IdentityAccessDemoApi);
    const http = TestBed.inject(HttpTestingController);
    const user = { id: crypto.randomUUID(), email: 'synthetic@example.test', displayName: 'Synthetic User', createdAt: '2026-11-11T00:00:00Z' };
    let result: DemoUser | undefined;

    api.create({ email: 'synthetic@example.test', displayName: 'Synthetic User' }).subscribe(value => result = value);

    const request = http.expectOne('/api/identity-demo/v1/users/create');
    expect(request.request.method).toBe('POST');
    expect(request.request.body.requestId).toMatch(/^[0-9a-f]{8}-[0-9a-f]{4}-7[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/);
    expect(request.request.body).toEqual({
      requestId: expect.any(String),
      email: 'synthetic@example.test',
      displayName: 'Synthetic User'
    });
    request.flush({ data: user, meta: { correlationId: crypto.randomUUID() } });
    expect(result).toEqual(user);
    http.verify();
  });

  it('sends UsersGetOne to the IdentityAccess backend', () => {
    const api = TestBed.inject(IdentityAccessDemoApi);
    const http = TestBed.inject(HttpTestingController);
    const userId = crypto.randomUUID();
    const user = { id: userId, email: 'synthetic@example.test', displayName: 'Synthetic User', createdAt: '2026-11-11T00:00:00Z' };
    let result: DemoUser | undefined;

    api.getOne(userId).subscribe(value => result = value);

    const request = http.expectOne(`/api/identity-demo/v1/users/get-one/${userId}`);
    expect(request.request.method).toBe('GET');
    request.flush({ data: user, meta: { correlationId: crypto.randomUUID() } });
    expect(result).toEqual(user);
    http.verify();
  });
});
