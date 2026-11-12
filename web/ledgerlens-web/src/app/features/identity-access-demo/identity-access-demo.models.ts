export interface DemoUser {
  readonly id: string;
  readonly email: string;
  readonly displayName: string;
  readonly createdAt: string;
}

export interface CreateDemoUser {
  readonly email: string;
  readonly displayName: string;
}
