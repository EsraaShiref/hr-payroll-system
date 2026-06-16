import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { DashboardService } from '../../core/services/dashboard.service';
import { LeaveRequestService } from '../../core/services/leave-request.service';
import { AuthService } from '../../core/services/auth.service';
import {
  DashboardAttendanceSummary, PendingLeaveRequest, PayrollBudgetSummary,
  HeadcountTrend, UpcomingContractRenewal, MonthlyHeadcount,
} from '../../models/dashboard';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule, FormsModule, RouterModule,
    MatCardModule, MatTableModule, MatButtonModule, MatIconModule,
    MatChipsModule, MatSelectModule, MatProgressBarModule, MatDividerModule, MatSnackBarModule,
  ],
  template: `
    <div class="dashboard">
      <div class="page-header">
        <h1>Dashboard</h1>
      </div>

      <div class="widgets">
        <!-- Widget 1: Attendance Summary -->
        <mat-card class="widget" appearance="outlined">
          <mat-card-header><mat-card-title>Today's Attendance</mat-card-title></mat-card-header>
          <mat-card-content>
            @if (attendance(); as a) {
              <div class="stat-row">
                <div class="stat-chip present">{{ a.totalPresent }}</div>
                <div class="stat-chip absent">{{ a.totalAbsent }}</div>
                <div class="stat-chip late">{{ a.totalLate }}</div>
                <div class="stat-chip leave">{{ a.totalOnLeave }}</div>
                <div class="stat-chip pending">{{ a.totalPendingReview }}</div>
              </div>
              <div class="stat-labels">
                <span>Present</span><span>Absent</span><span>Late</span><span>Leave</span><span>Pending</span>
              </div>

              @if (a.departmentBreakdown.length > 0) {
                <mat-divider class="divider" />
                <table class="mini-table">
                  <tr><th>Dept</th><th>Present</th><th>Absent</th><th>Late</th><th>Leave</th></tr>
                  @for (d of a.departmentBreakdown; track d.departmentName) {
                    <tr>
                      <td>{{ d.departmentName }}</td>
                      <td>{{ d.present }}</td>
                      <td>{{ d.absent }}</td>
                      <td>{{ d.late }}</td>
                      <td>{{ d.onLeave }}</td>
                    </tr>
                  }
                </table>
              }
            }
          </mat-card-content>
        </mat-card>

        <!-- Widget 2: Pending Leave -->
        <mat-card class="widget" appearance="outlined">
          <mat-card-header><mat-card-title>Pending Leave</mat-card-title></mat-card-header>
          <mat-card-content>
            @if (pendingLeave().length === 0) {
              <div class="empty">No pending requests</div>
            }
            @for (r of pendingLeave(); track r.leaveRequestId) {
              <div class="leave-row">
                <div class="leave-info">
                  <strong>{{ r.employeeName }}</strong>
                  <span class="leave-detail">{{ r.leaveType }} &middot; {{ r.startDate }} &middot; {{ r.totalDays }}d</span>
                  @if (r.reason) { <span class="leave-reason">{{ r.reason }}</span> }
                </div>
                <div class="leave-actions">
                  <button mat-icon-button color="primary" (click)="approveLeave(r.leaveRequestId)" matTooltip="Approve">
                    <mat-icon>check_circle</mat-icon>
                  </button>
                  <button mat-icon-button color="warn" (click)="rejectLeave(r.leaveRequestId)" matTooltip="Reject">
                    <mat-icon>cancel</mat-icon>
                  </button>
                </div>
              </div>
              @if (!$last) { <mat-divider /> }
            }
          </mat-card-content>
        </mat-card>

        <!-- Widget 3: Payroll Budget -->
        <mat-card class="widget" appearance="outlined">
          <mat-card-header>
            <mat-card-title>Payroll Budget</mat-card-title>
            <div class="widget-controls">
              <mat-select [(ngModel)]="budgetMonth" (selectionChange)="loadBudget()" subscriptSizing="dynamic">
                @for (m of months; track m.value) {
                  <mat-option [value]="m.value">{{ m.label }}</mat-option>
                }
              </mat-select>
              <mat-select [(ngModel)]="budgetYear" (selectionChange)="loadBudget()" subscriptSizing="dynamic">
                @for (y of budgetYears; track y) {
                  <mat-option [value]="y">{{ y }}</mat-option>
                }
              </mat-select>
            </div>
          </mat-card-header>
          <mat-card-content>
            @if (budget(); as b) {
              <div class="budget-row">
                <div class="budget-item">
                  <span class="budget-value">{{ b.projectedGrossPay | number:'1.0-0' }}</span>
                  <span class="budget-label">Projected</span>
                </div>
                <div class="budget-item">
                  <span class="budget-value actual">{{ b.actualGrossPay | number:'1.0-0' }}</span>
                  <span class="budget-label">Actual Gross</span>
                </div>
                <div class="budget-item">
                  <span class="budget-value net">{{ b.actualNetPay | number:'1.0-0' }}</span>
                  <span class="budget-label">Net Pay</span>
                </div>
              </div>
              <mat-progress-bar mode="determinate" [value]="budgetPct(b)" />
              <div class="budget-employees">{{ b.actualEmployeeCount }} employees</div>
            } @else {
              <div class="empty">No finalized payroll for this period</div>
            }
          </mat-card-content>
        </mat-card>

        <!-- Widget 4: Headcount Trend (MatTable) -->
        <mat-card class="widget" appearance="outlined">
          <mat-card-header><mat-card-title>Headcount Trend</mat-card-title></mat-card-header>
          <mat-card-content>
            <div class="current-hc">Current: <strong>{{ headcount().currentHeadcount }}</strong></div>
            <table class="mini-table">
              <tr><th>Month</th><th>Count</th><th>Change</th></tr>
              @for (m of headcount().months; track m.label) {
                <tr>
                  <td>{{ m.label }}</td>
                  <td>{{ m.count }}</td>
                  <td>
                    @if (m.changeFromPrevious > 0) {
                      <span class="up">▲ {{ m.changeFromPrevious }}</span>
                    } @else if (m.changeFromPrevious < 0) {
                      <span class="down">▼ {{ -m.changeFromPrevious }}</span>
                    } @else {
                      —
                    }
                  </td>
                </tr>
              }
            </table>
          </mat-card-content>
        </mat-card>

        <!-- Widget 5: Contract Renewals -->
        <mat-card class="widget" appearance="outlined">
          <mat-card-header><mat-card-title>Contract Renewals</mat-card-title></mat-card-header>
          <mat-card-content>
            @if (renewals().length === 0) {
              <div class="empty">No renewals due</div>
            }
            @for (r of renewals(); track r.contractId) {
              <div class="renewal-row">
                <div>
                  <strong>{{ r.employeeName }}</strong>
                  <span class="renewal-detail">{{ r.contractType }} &middot; expires {{ r.expiryDate }}</span>
                </div>
                <div class="days-badge" [class.urgent]="r.daysRemaining <= 7">
                  {{ r.daysRemaining }}d
                </div>
              </div>
            }
          </mat-card-content>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .dashboard { padding: 1.5rem; }
    .page-header { margin-bottom: 1rem; }
    .page-header h1 { margin: 0; font-size: 1.5rem; font-weight: 400; }
    .widgets { display: grid; grid-template-columns: repeat(auto-fill, minmax(380px, 1fr)); gap: 1rem; }
    .widget mat-card-header { display: flex; justify-content: space-between; align-items: center; }
    .widget-controls { display: flex; gap: 0.5rem; }
    .stat-row { display: flex; gap: 0.5rem; }
    .stat-chip { width: 48px; height: 48px; border-radius: 8px; display: flex; align-items: center; justify-content: center; font-size: 1.2rem; font-weight: 600; color: #fff; }
    .stat-chip.present { background: #4caf50; }
    .stat-chip.absent { background: #f44336; }
    .stat-chip.late { background: #ff9800; }
    .stat-chip.leave { background: #2196f3; }
    .stat-chip.pending { background: #9e9e9e; }
    .stat-labels { display: flex; gap: 0.5rem; font-size: 0.7rem; color: var(--mat-sys-on-surface-variant, #666); margin-top: 0.25rem; }
    .stat-labels span { width: 48px; text-align: center; }
    .divider { margin: 0.75rem 0; }
    .mini-table { width: 100%; font-size: 0.85rem; border-collapse: collapse; }
    .mini-table th { text-align: left; font-weight: 500; padding: 0.25rem 0.5rem; color: var(--mat-sys-on-surface-variant, #666); border-bottom: 1px solid var(--mat-sys-outline-variant, #e0e0e0); }
    .mini-table td { padding: 0.25rem 0.5rem; }
    .empty { text-align: center; padding: 1.5rem; color: var(--mat-sys-on-surface-variant, #666); font-size: 0.9rem; }
    .leave-row { display: flex; justify-content: space-between; align-items: center; padding: 0.5rem 0; }
    .leave-info { display: flex; flex-direction: column; }
    .leave-detail { font-size: 0.8rem; color: var(--mat-sys-on-surface-variant, #666); }
    .leave-reason { font-size: 0.8rem; font-style: italic; }
    .leave-actions { display: flex; gap: 0.25rem; }
    .budget-row { display: flex; gap: 1rem; margin-bottom: 0.75rem; }
    .budget-item { display: flex; flex-direction: column; align-items: center; flex: 1; }
    .budget-value { font-size: 1.3rem; font-weight: 600; }
    .budget-value.actual { color: #2196f3; }
    .budget-value.net { color: #4caf50; }
    .budget-label { font-size: 0.7rem; text-transform: uppercase; color: var(--mat-sys-on-surface-variant, #666); }
    .budget-employees { font-size: 0.8rem; color: var(--mat-sys-on-surface-variant, #666); margin-top: 0.5rem; }
    .current-hc { margin-bottom: 0.5rem; font-size: 0.95rem; }
    .up { color: #4caf50; }
    .down { color: #f44336; }
    .renewal-row { display: flex; justify-content: space-between; align-items: center; padding: 0.5rem 0; border-bottom: 1px solid var(--mat-sys-outline-variant, #e0e0e0); }
    .renewal-detail { font-size: 0.8rem; color: var(--mat-sys-on-surface-variant, #666); display: block; }
    .days-badge { background: var(--mat-sys-primary, #1976d2); color: #fff; border-radius: 12px; padding: 0.2rem 0.6rem; font-size: 0.85rem; font-weight: 600; }
    .days-badge.urgent { background: #f44336; }
  `],
})
export class AdminDashboardComponent implements OnInit {
  private dashboardService = inject(DashboardService);
  private leaveService = inject(LeaveRequestService);
  private auth = inject(AuthService);
  private snackBar = inject(MatSnackBar);

  attendance = signal<DashboardAttendanceSummary>({} as DashboardAttendanceSummary);
  pendingLeave = signal<PendingLeaveRequest[]>([]);
  budget = signal<PayrollBudgetSummary | null>(null);
  headcount = signal<HeadcountTrend>({ months: [], currentHeadcount: 0 });
  renewals = signal<UpcomingContractRenewal[]>([]);

  now = new Date();
  budgetMonth = this.now.getMonth() + 1;
  budgetYear = this.now.getFullYear();
  months = Array.from({ length: 12 }, (_, i) => ({ value: i + 1, label: new Date(0, i).toLocaleString('en', { month: 'short' }) }));
  budgetYears = Array.from({ length: 4 }, (_, i) => this.now.getFullYear() - 1 + i);

  ngOnInit(): void {
    this.loadAll();
  }

  loadAll(): void {
    this.dashboardService.getAttendanceSummary().subscribe(a => this.attendance.set(a));
    this.dashboardService.getPendingLeave().subscribe(p => this.pendingLeave.set(p));
    this.loadBudget();
    this.dashboardService.getHeadcountTrend().subscribe(h => this.headcount.set(h));
    this.dashboardService.getContractRenewals().subscribe(r => this.renewals.set(r));
  }

  loadBudget(): void {
    this.dashboardService.getPayrollBudget(this.budgetYear, this.budgetMonth)
      .subscribe(b => this.budget.set(b));
  }

  budgetPct(b: PayrollBudgetSummary): number {
    return b.projectedGrossPay > 0 ? Math.round((b.actualGrossPay / b.projectedGrossPay) * 100) : 0;
  }

  approveLeave(id: string): void {
    const uid = this.auth.userId();
    if (!uid) return;
    this.leaveService.approve(id, uid).subscribe({
      next: () => {
        this.snackBar.open('Leave approved', 'Dismiss', { duration: 3000 });
        this.pendingLeave.set(this.pendingLeave().filter(r => r.leaveRequestId !== id));
      },
      error: () => this.snackBar.open('Failed to approve', 'Dismiss', { duration: 3000 }),
    });
  }

  rejectLeave(id: string): void {
    const uid = this.auth.userId();
    if (!uid) return;
    this.leaveService.reject(id, uid, 'Declined').subscribe({
      next: () => {
        this.snackBar.open('Leave rejected', 'Dismiss', { duration: 3000 });
        this.pendingLeave.set(this.pendingLeave().filter(r => r.leaveRequestId !== id));
      },
      error: () => this.snackBar.open('Failed to reject', 'Dismiss', { duration: 3000 }),
    });
  }
}
