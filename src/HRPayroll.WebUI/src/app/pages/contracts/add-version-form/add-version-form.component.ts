import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule, NgForm } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { provideNativeDateAdapter } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { ContractService, AddContractVersionRequest, AllowanceAssignmentInput } from '../../../core/services/contract.service';
import { AllowanceDto } from '../../../models/hr';

@Component({
  selector: 'app-add-version-form',
  standalone: true,
  providers: [provideNativeDateAdapter()],
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <button mat-icon-button (click)="goBack()"><mat-icon>arrow_back</mat-icon></button>
        <h1>Add Contract Version</h1>
        <span class="contract-badge">Contract v{{ currentVersionNumber() }}</span>
        <span class="currency-badge">{{ currency() }}</span>
      </div>

      <mat-card appearance="outlined" class="form-card">
        <mat-card-content>
          <form #f="ngForm" (ngSubmit)="onSubmit(f)">
            <div class="form-row">
              <mat-form-field appearance="outline">
                <mat-label>New Base Salary ({{ currency() }})</mat-label>
                <input matInput type="number" name="newBaseSalaryAmount"
                       [(ngModel)]="model.newBaseSalaryAmount" required min="0.01" step="0.01" />
              </mat-form-field>
              <mat-form-field appearance="outline">
                <mat-label>Effective From</mat-label>
                <input matInput [matDatepicker]="picker" name="effectiveFrom"
                       [(ngModel)]="effectiveFromDate" required />
                <mat-datepicker-toggle matSuffix [for]="picker" />
                <mat-datepicker #picker />
              </mat-form-field>
            </div>

            <mat-divider />
            <h3 class="section-title">Allowance Assignments</h3>
            <p class="section-hint">Leave override fields empty to use the allowance default value.</p>

            <div class="allowance-list">
              @for (a of allowances(); track a.id; let i = $index) {
                <div class="allowance-row">
                  <span class="allowance-name">{{ a.name }}</span>
                  <mat-form-field appearance="outline" subscriptSizing="dynamic">
                    <mat-label>Override Amount</mat-label>
                    <input matInput type="number" [(ngModel)]="allowanceModels[i].overrideAmount"
                           [name]="'amt_' + a.id" step="0.01" />
                  </mat-form-field>
                  <mat-form-field appearance="outline" subscriptSizing="dynamic">
                    <mat-label>Override %</mat-label>
                    <input matInput type="number" [(ngModel)]="allowanceModels[i].overridePercentage"
                           [name]="'pct_' + a.id" step="0.01" />
                  </mat-form-field>
                </div>
              }
            </div>

            @if (error()) {
              <div class="error-banner">{{ error() }}</div>
            }

            <div class="actions">
              <button mat-stroked-button type="button" (click)="goBack()">Cancel</button>
              <button mat-raised-button color="primary" type="submit" [disabled]="f.invalid || submitting()">
                {{ submitting() ? 'Saving...' : 'Add Version' }}
              </button>
            </div>
          </form>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .page-container { padding: 1.5rem; max-width: 700px; }
    .page-header { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 1.5rem; }
    .page-header h1 { margin: 0; font-size: 1.5rem; font-weight: 400; flex: 1; }
    .contract-badge { background: #e3f2fd; color: #1565c0; padding: 0.25rem 0.75rem; border-radius: 12px; font-size: 0.8rem; font-weight: 500; }
    .currency-badge { background: #f3e5f5; color: #7b1fa2; padding: 0.25rem 0.75rem; border-radius: 12px; font-size: 0.8rem; font-weight: 500; }
    .form-card { padding: 1rem; }
    .form-row { display: flex; gap: 1rem; margin-bottom: 0.5rem; }
    .form-row mat-form-field { flex: 1; }
    .section-title { margin: 1.5rem 0 0.25rem; font-size: 1rem; font-weight: 500; }
    .section-hint { color: var(--mat-sys-on-surface-variant); font-size: 0.8rem; margin: 0 0 1rem; }
    .allowance-list { display: flex; flex-direction: column; gap: 0.5rem; }
    .allowance-row { display: flex; align-items: center; gap: 1rem; }
    .allowance-name { min-width: 150px; font-size: 0.9rem; }
    .allowance-row mat-form-field { width: 150px; }
    .error-banner { background: #fef2f2; border: 1px solid #fecaca; border-radius: 6px; padding: 0.75rem; margin: 1rem 0; color: #dc2626; font-size: 0.875rem; }
    .actions { display: flex; justify-content: flex-end; gap: 0.75rem; margin-top: 1.5rem; }
  `],
})
export class AddVersionFormComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private contractService = inject(ContractService);

  contractId = '';
  currentVersionNumber = signal(0);
  currency = signal('USD');

  allowances = signal<AllowanceDto[]>([]);
  allowanceModels: { overrideAmount: number | null; overridePercentage: number | null }[] = [];

  submitting = signal(false);
  error = signal('');
  effectiveFromDate: Date | null = null;

  model: AddContractVersionRequest = {
    newBaseSalaryAmount: 0,
    newBaseSalaryCurrency: 'USD',
    effectiveFrom: '',
    taxBracketSetId: null,
    socialInsuranceConfigId: null,
    allowanceAssignments: null,
  };

  ngOnInit(): void {
    this.contractId = this.route.snapshot.paramMap.get('id')!;

    // Fetch current version info for context
    this.contractService.getById(this.contractId).subscribe({
      next: c => {
        this.currentVersionNumber.set(c.currentVersion.versionNumber);
        this.currency.set(c.currentVersion.baseSalaryCurrency);
        this.model.newBaseSalaryCurrency = c.currentVersion.baseSalaryCurrency;
        this.model.newBaseSalaryAmount = c.currentVersion.baseSalaryAmount;
      },
    });

    this.loadAllowances();
  }

  private loadAllowances(): void {
    this.allowances.set([]);
  }

  private formatDate(value: Date | string): string {
    if (!value) return '';
    const d = typeof value === 'string' ? new Date(value) : value;
    return d.toISOString().split('T')[0];
  }

  goBack(): void {
    this.router.navigate(['/contracts', this.contractId]);
  }

  onSubmit(f: NgForm): void {
    if (f.invalid) return;
    this.submitting.set(true);
    this.error.set('');

    const assignments: AllowanceAssignmentInput[] = this.allowances()
      .map((a, i) => ({
        allowanceId: a.id,
        overrideAmount: this.allowanceModels[i]?.overrideAmount ?? null,
        overridePercentage: this.allowanceModels[i]?.overridePercentage ?? null,
      }))
      .filter(a => a.overrideAmount !== null || a.overridePercentage !== null);

    const payload: AddContractVersionRequest = {
      newBaseSalaryAmount: this.model.newBaseSalaryAmount,
      newBaseSalaryCurrency: this.model.newBaseSalaryCurrency,
      effectiveFrom: this.formatDate(this.effectiveFromDate ?? ''),
      taxBracketSetId: null,
      socialInsuranceConfigId: null,
      allowanceAssignments: assignments.length > 0 ? assignments : null,
    };

    this.contractService.addVersion(this.contractId, payload).subscribe({
      next: () => this.goBack(),
      error: err => {
        this.submitting.set(false);
        this.error.set(err.error?.detail || 'Failed to add version.');
      },
    });
  }
}
