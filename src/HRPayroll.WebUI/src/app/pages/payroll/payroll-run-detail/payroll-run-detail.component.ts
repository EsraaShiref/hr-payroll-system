import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { PayrollService } from '../../../core/services/payroll.service';
import { PayrollRunDetail } from '../../../models/hr';

@Component({
  selector: 'app-payroll-run-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatChipsModule,
    MatProgressSpinnerModule,
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <div>
          <a [routerLink]="['/payroll', runId]" class="back-link">&larr; Back to Run</a>
          @if (detail(); as d) {
            <h1>{{ d.employeeName }}</h1>
            <p class="subtitle">Code: {{ d.employeeCode }}</p>
          }
        </div>
        <mat-chip [class]="detail()?.status === 'Calculated' ? 'status-calculated' : 'status-skipped'" [disabled]="true">
          {{ detail()?.status }}
        </mat-chip>
      </div>

      @if (loading()) {
        <div class="spinner-container">
          <mat-progress-spinner mode="indeterminate" diameter="40" />
        </div>
      }

      @if (detail(); as d) {
        <!-- Skip / Failure Message -->
        @if (d.status !== 'Calculated') {
          <mat-card class="message-card">
            <mat-card-content>
              <p><strong>{{ d.status === 'Skipped' ? 'Skipped' : 'Failed' }}:</strong> {{ d.skipReason || d.failureMessage }}</p>
            </mat-card-content>
          </mat-card>
        }

        <div class="detail-grid">
          <!-- Attendance -->
          <mat-card>
            <mat-card-header><mat-card-title>Attendance</mat-card-title></mat-card-header>
            <mat-card-content>
              <div class="stat-row"><span>Scheduled Days</span><span>{{ d.totalScheduledDays }}</span></div>
              <div class="stat-row"><span>Present Days</span><span class="positive">{{ d.totalPresentDays }}</span></div>
              <div class="stat-row"><span>Absent Days</span><span class="negative">{{ d.totalAbsentDays }}</span></div>
              <div class="stat-row"><span>Leave Days</span><span>{{ d.totalLeaveDays }}</span></div>
              <div class="stat-row"><span>Overtime (minutes)</span><span>{{ d.totalOvertimeMinutes }}</span></div>
              <div class="stat-row"><span>Late Occurrences</span><span>{{ d.lateOccurrenceCount }}</span></div>
              <div class="stat-row"><span>Late Penalty Units</span><span>{{ d.latePenaltyUnits }}</span></div>
            </mat-card-content>
          </mat-card>

          <!-- Earnings -->
          <mat-card>
            <mat-card-header><mat-card-title>Earnings</mat-card-title></mat-card-header>
            <mat-card-content>
              <div class="stat-row"><span>Base Salary</span><span>{{ d.baseSalary | currency:'USD':'symbol':'1.2-2' }}</span></div>
              <div class="stat-row"><span>Total Allowances</span><span>{{ d.totalAllowances | currency:'USD':'symbol':'1.2-2' }}</span></div>
              <div class="stat-row"><span>Overtime Pay</span><span>{{ d.overtimePay | currency:'USD':'symbol':'1.2-2' }}</span></div>
              <div class="stat-row total"><span>Gross Pay</span><span>{{ d.grossPay | currency:'USD':'symbol':'1.2-2' }}</span></div>
            </mat-card-content>
          </mat-card>

          <!-- Deductions -->
          <mat-card>
            <mat-card-header><mat-card-title>Deductions</mat-card-title></mat-card-header>
            <mat-card-content>
              <div class="stat-row"><span>Leave Deduction</span><span class="negative">{{ d.leaveDeduction | currency:'USD':'symbol':'1.2-2' }}</span></div>
              <div class="stat-row"><span>Late Penalty</span><span class="negative">{{ d.latePenaltyDeduction | currency:'USD':'symbol':'1.2-2' }}</span></div>
              <div class="stat-row"><span>Social Insurance (EE)</span><span class="negative">{{ d.socialInsuranceEmployeeShare | currency:'USD':'symbol':'1.2-2' }}</span></div>
              <div class="stat-row"><span>Income Tax</span><span class="negative">{{ d.taxAmount | currency:'USD':'symbol':'1.2-2' }}</span></div>
              <div class="stat-row total"><span>Total Deductions</span><span class="negative">{{ d.totalDeductions | currency:'USD':'symbol':'1.2-2' }}</span></div>
            </mat-card-content>
          </mat-card>

          <!-- Net Pay -->
          <mat-card class="net-pay-card">
            <mat-card-header><mat-card-title>Net Pay</mat-card-title></mat-card-header>
            <mat-card-content>
              <div class="net-amount">{{ d.netPay | currency:'USD':'symbol':'1.2-2' }}</div>
              <div class="tax-info">Taxable Income: {{ d.taxableIncome | currency:'USD':'symbol':'1.2-2' }}</div>
            </mat-card-content>
          </mat-card>
        </div>
      }
    </div>
  `,
  styles: [`
    .page-container { padding: 1.5rem; max-width: 900px; }
    .page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 1.5rem; }
    .page-header h1 { margin: 0.25rem 0 0; font-size: 1.5rem; font-weight: 400; }
    .subtitle { margin: 0; font-size: 0.875rem; color: var(--mat-sys-on-surface-variant, #666); }
    .back-link { font-size: 0.875rem; color: var(--mat-sys-primary, #1976d2); text-decoration: none; }
    .back-link:hover { text-decoration: underline; }
    .spinner-container { display: flex; justify-content: center; padding: 3rem; }

    .message-card { margin-bottom: 1rem; }
    .message-card p { margin: 0; }

    .detail-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
    .detail-grid > :last-child:nth-child(odd) { grid-column: 1 / -1; }

    .stat-row { display: flex; justify-content: space-between; padding: 0.5rem 0; border-bottom: 1px solid var(--mat-sys-outline-variant, #e0e0e0); font-size: 0.9rem; }
    .stat-row.total { font-weight: 600; border-bottom: none; padding-top: 0.75rem; }
    .positive { color: #16a34a; }
    .negative { color: #dc2626; }

    .net-pay-card { border-left: 4px solid var(--mat-sys-primary, #1976d2); }
    .net-amount { font-size: 2rem; font-weight: 600; color: var(--mat-sys-primary, #1976d2); }
    .tax-info { font-size: 0.85rem; color: var(--mat-sys-on-surface-variant, #666); margin-top: 0.5rem; }

    .status-calculated { background: #dcfce7 !important; color: #16a34a !important; }
    .status-skipped { background: #fef9c3 !important; color: #a16207 !important; }
  `],
})
export class PayrollRunDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private payrollService = inject(PayrollService);

  runId = '';
  employeeId = '';
  detail = signal<PayrollRunDetail | null>(null);
  loading = signal(false);

  ngOnInit(): void {
    this.runId = this.route.snapshot.paramMap.get('runId') ?? '';
    this.employeeId = this.route.snapshot.paramMap.get('employeeId') ?? '';
    this.loadDetail();
  }

  loadDetail(): void {
    this.loading.set(true);
    this.payrollService.getDetail(this.runId, this.employeeId).subscribe({
      next: d => { this.detail.set(d); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }
}
