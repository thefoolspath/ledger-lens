import { HttpErrorResponse } from '@angular/common/http';
import { apiProblemMessage } from './api-response';

describe('apiProblemMessage', () => {
  it('prefers the first localized validation message', () => {
    const error = new HttpErrorResponse({
      status: 400,
      error: {
        title: 'ข้อมูลไม่ถูกต้อง',
        detail: 'ค่าหนึ่งค่าหรือมากกว่าในคำขอไม่ถูกต้อง',
        errors: { accountId: ['รหัสทรัพยากร LedgerLens ต้องเป็น UUIDv7'] }
      }
    });

    expect(apiProblemMessage(error, 'fallback')).toBe('รหัสทรัพยากร LedgerLens ต้องเป็น UUIDv7');
  });

  it('uses detail before title and falls back for non-Problem Details errors', () => {
    const problem = new HttpErrorResponse({
      status: 503,
      error: { title: 'Market data unavailable', detail: 'Market data is temporarily unavailable.' }
    });

    expect(apiProblemMessage(problem, 'fallback')).toBe('Market data is temporarily unavailable.');
    expect(apiProblemMessage(new Error('network'), 'fallback')).toBe('fallback');
  });
});
