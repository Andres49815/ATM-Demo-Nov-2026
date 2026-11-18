import assert from 'node:assert/strict';

const base = `${process.argv[2] ?? 'http://localhost:5111'}/api/atm`;

async function dashboard() {
  const response = await fetch(`${base}/dashboard`);
  assert.equal(response.status, 200);
  return response.json();
}

async function post(resource, body, expectedStatus, expectedCode) {
  const response = await fetch(`${base}/${resource}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  });
  assert.equal(response.status, expectedStatus, `${resource}: ${JSON.stringify(body)}`);
  if (expectedCode) assert.equal((await response.json()).code, expectedCode);
}

const balances = ({ accounts }) => Object.fromEntries(accounts.map(({ id, balance }) => [id, balance]));
const initial = await dashboard();
assert.deepEqual(balances(initial), { checking: 1250, savings: 500 }, 'Start a fresh API before testing.');
assert.deepEqual(initial.transactions, [], 'Start a fresh API before testing.');

await post('deposits', { accountId: 'checking', amount: 25.25 }, 204);
await post('withdrawals', { accountId: 'checking', amount: 10.1 }, 204);
await post('transfers', { fromAccountId: 'checking', toAccountId: 'savings', amount: 100.05 }, 204);

const after = await dashboard();
assert.deepEqual(balances(after), { checking: 1165.1, savings: 600.05 });
assert.deepEqual(
  after.transactions.map(t => [t.kind, t.amount, t.fromAccountId, t.toAccountId]),
  [
    ['Transfer', 100.05, 'checking', 'savings'],
    ['Withdrawal', 10.1, 'checking', null],
    ['Deposit', 25.25, null, 'checking'],
  ],
);

await post('withdrawals', { accountId: 'savings', amount: 601 }, 409, 'insufficient_funds');
await post('transfers', { fromAccountId: 'savings', toAccountId: 'checking', amount: 601 }, 409, 'insufficient_funds');
await post('transfers', { fromAccountId: 'checking', toAccountId: 'checking', amount: 1 }, 400, 'same_account');
for (const amount of [0, -1, 0.001]) {
  await post('deposits', { accountId: 'checking', amount }, 400, 'invalid_amount');
}
await post('deposits', { accountId: 'unknown', amount: 1 }, 404, 'account_not_found');
assert.deepEqual(await dashboard(), after, 'Rejected operations must not change balances or history.');

// Each request can afford the withdrawal alone, but only one can succeed.
const responses = await Promise.all([1, 2].map(() => fetch(`${base}/withdrawals`, {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ accountId: 'savings', amount: 600 }),
})));
assert.deepEqual(responses.map(response => response.status).sort(), [204, 409]);
const final = await dashboard();
assert.deepEqual(balances(final), { checking: 1165.1, savings: 0.05 });
assert.equal(final.transactions.length, 4);
assert.deepEqual(final.transactions.slice(1), after.transactions);

console.log('ATM smoke test passed: deposits, withdrawals, transfers, history, validation, and concurrent withdrawals.');
