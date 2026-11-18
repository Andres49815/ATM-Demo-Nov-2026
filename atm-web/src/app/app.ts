import { CurrencyPipe, DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { AtmApi, Dashboard, Operation, Transaction } from './atm-api';

@Component({
  selector: 'app-root',
  imports: [CurrencyPipe, DatePipe, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  private readonly api = inject(AtmApi);
  protected readonly snapshot = signal<Dashboard | null>(null);
  protected readonly loading = signal(false);
  protected readonly submitting = signal(false);
  protected readonly loadError = signal('');
  protected readonly submitError = signal('');
  protected readonly success = signal('');

  protected readonly operations: { key: Operation; label: string }[] = [
    { key: 'deposit', label: 'Deposit' },
    { key: 'withdrawal', label: 'Withdraw' },
    { key: 'transfer', label: 'Transfer' },
  ];
  protected readonly operation = signal<Operation>('deposit');
  protected readonly accountId = signal('');
  protected readonly destination = computed(() =>
    this.snapshot()?.accounts.find(account => account.id !== this.accountId()),
  );
  protected readonly actionLabel = computed(() =>
    this.operations.find(item => item.key === this.operation())!.label,
  );
  protected amount = '';

  ngOnInit() {
    void this.load();
  }

  protected async load() {
    if (this.loading()) return;

    this.loading.set(true);
    this.loadError.set('');
    try {
      const dashboard = await firstValueFrom(this.api.dashboard());
      this.snapshot.set(dashboard);
      if (!dashboard.accounts.some(account => account.id === this.accountId())) {
        this.accountId.set(dashboard.accounts[0]?.id ?? '');
      }
    } catch {
      this.loadError.set('Could not load the latest balances and activity.');
    } finally {
      this.loading.set(false);
    }
  }

  protected selectOperation(operation: Operation) {
    this.operation.set(operation);
    this.submitError.set('');
    this.success.set('');
  }

  protected async submit() {
    if (this.submitting() || this.loading() || this.loadError() || !this.accountId()) return;

    this.success.set('');
    this.submitError.set('');
    const input = this.amount.trim();
    const amount = Number(input);
    if (!/^(?:\d+(?:\.\d{1,2})?|\.\d{1,2})$/.test(input) || !Number.isFinite(amount) || amount <= 0) {
      this.submitError.set('Enter a positive amount with at most two decimal places.');
      return;
    }
    this.submitting.set(true);
    try {
      await firstValueFrom(
        this.api.submit(this.operation(), this.accountId(), amount, this.destination()?.id ?? ''),
      );
      this.amount = '';
      this.success.set(`${this.actionLabel()} completed.`);
      await this.load();
    } catch (error) {
      if (error instanceof HttpErrorResponse && error.status >= 400 && error.status < 500) {
        const detail: unknown = error.error?.detail;
        this.submitError.set(typeof detail === 'string' ? detail : 'The ATM rejected the request.');
      } else {
        this.submitError.set('Could not confirm the transaction. Check your activity before retrying.');
        await this.load();
      }
    } finally {
      this.submitting.set(false);
    }
  }

  protected transactionAccounts(transaction: Transaction) {
    const name = (id: string | null) => this.snapshot()?.accounts.find(account => account.id === id)?.name;
    if (transaction.kind === 'Transfer') {
      return `${name(transaction.fromAccountId)} to ${name(transaction.toAccountId)}`;
    }
    return transaction.kind === 'Deposit' ? name(transaction.toAccountId) : name(transaction.fromAccountId);
  }
}
