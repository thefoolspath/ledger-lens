import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map, Observable } from 'rxjs';
import { ApiResponse } from '../../api-response';
import { CreateDemoUser, DemoUser } from './identity-access-demo.models';

const demoApiPrefix = '/identity-demo-api/v1/users';

@Injectable()
export class IdentityAccessDemoApi {
  private readonly http = inject(HttpClient);

  create(request: CreateDemoUser): Observable<DemoUser> {
    return this.http.post<ApiResponse<DemoUser>>(`${demoApiPrefix}/create`, {
      requestId: createRequestId(),
      email: request.email.trim(),
      displayName: request.displayName.trim()
    }).pipe(map(response => response.data));
  }

  getOne(id: string): Observable<DemoUser> {
    return this.http.get<ApiResponse<DemoUser>>(`${demoApiPrefix}/get-one/${encodeURIComponent(id.trim())}`)
      .pipe(map(response => response.data));
  }
}

function createRequestId(): string {
  const bytes = crypto.getRandomValues(new Uint8Array(16));
  let timestamp = Date.now();
  for (let index = 5; index >= 0; index--) {
    bytes[index] = timestamp & 0xff;
    timestamp = Math.floor(timestamp / 256);
  }
  bytes[6] = (bytes[6] & 0x0f) | 0x70;
  bytes[8] = (bytes[8] & 0x3f) | 0x80;
  const hex = Array.from(bytes, byte => byte.toString(16).padStart(2, '0')).join('');
  return `${hex.slice(0, 8)}-${hex.slice(8, 12)}-${hex.slice(12, 16)}-${hex.slice(16, 20)}-${hex.slice(20)}`;
}
