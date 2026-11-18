import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

export interface Account {
  id: string;
  name: string;
  balance: number;
}

export interface Transaction {
  id: number;
  kind: 'Deposit' | 'Withdrawal' | 'Transfer';
  amount: number;
  fromAccountId: string | null;
  toAccountId: string | null;
  occurredAt: string;
}

export interface Dashboard {
  accounts: Account[];
  transactions: Transaction[];
}

export type Operation = 'deposit' | 'withdrawal' | 'transfer';

@Injectable({ providedIn: 'root' })
export class AtmApi {
  private readonly http = inject(HttpClient);
  private readonly base = '/api/atm';

  dashboard() {
    return this.http.get<Dashboard>(`${this.base}/dashboard`);
  }

  submit(operation: Operation, accountId: string, amount: number, destinationId: string) {
    if (operation === 'transfer') {
      return this.http.post<void>(`${this.base}/transfers`, {
        fromAccountId: accountId, toAccountId: destinationId, amount,
      });
    }

    const resource = operation === 'deposit' ? 'deposits' : 'withdrawals';
    return this.http.post<void>(`${this.base}/${resource}`, { accountId, amount });
  }
}
