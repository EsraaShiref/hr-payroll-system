import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatTableModule } from '@angular/material/table';
import { MatDialog } from '@angular/material/dialog';
import { PayrollService } from '../../../core/services/payroll.service';
import {
  PayrollRunSummary,
  PayrollRunStatus,
  SkippedEmployeeInfo,
  FailedEmployeeInfo,
} from '../../../models/hr';
import { RejectPayrollDialog, PatchPayrollDialog } from './payroll-dialogs';
import { Subscription, interval } from 'rxjs';

@Component({
  selector: 'app-payroll-run-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    MatCardModule,
    MatChipsModule,
    MatTableModule,
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <div>
          <a routerLink="/payroll" class="back-link">&larr; Payroll Runs</a>
          <h1>{{ monthNames[runSummary().month - 1] }} {{ runSummary().year }} Run</h1>
        </div>
        <div class="status-badge">
          <mat-chip [class]="statusClass(runSummary().status)" [disabled]="true">{{ runSummary().status }}</mat-chip>
        </div>
      </div>

      <!-- Progress -->
      @if (runSummary().status === 'Draft' || runSummary().status === 'Processing') {
        <mat-card class="progress-card">
          <mat-card-content>
            <div class="progress-header">
              <span>Processing payroll...</span>
              <span>{{ pollStatus().calculatedCount }} / {{ pollStatus().totalEmployees }} employees</span>
            </div>
            <mat-progress-bar
              mode="determinate"
              [value]="progressPercent()"
              color="primary"
            />
            @if (pollStatus().failedCount > 0) {
              <p class="failed-warning">{{ pollStatus().failedCount }} employee(s) failed — check details below</p>
            }
          </mat-card-content>
        </mat-card>
      }

      <!-- Summary Cards -->
      <div class="summary-cards">
        <mat-card class="summary-card">
          <mat-card-content>
            <div class="card-label">Gross Pay</div>
            <div class="card-value">{{ runSummary().totalGrossPay | currency:'USD':'symbol':'1.2-2' }}</div>
          </mat-card-content>
        </mat-card>
        <mat-card class="summary-card">
          <mat-card-content>
            <div class="card-label">Total Deductions</div>
            <div class="card-value deduction">{{ runSummary().totalDeductions | currency:'USD':'symbol':'1.2-2' }}</div>
          </mat-card-content>
        </mat-card>
        <mat-card class="summary-card net-pay">
          <mat-card-content>
            <div class="card-label">Net Pay</div>
            <div class="card-value">{{ runSummary().totalNetPay | currency:'USD':'symbol':'1.2-2' }}</div>
          </mat-card-content>
        </mat-card>
        <mat-card class="summary-card">
          <mat-card-content>
            <div class="card-label">Employees</div>
            <div class="card-value">
              {{ runSummary().calculatedCount }} / {{ runSummary().totalEmployees }}
              <span class="sub-text">({{ runSummary().skippedCount }} skipped, {{ runSummary().failedCount }} failed)</span>
            </div>
          </mat-card-content>
        </mat-card>
      </div>

      <!-- Action Buttons -->
      @if (showApprove()) {
        <div class="action-bar">
          <button mat-raised-button color="primary" (click)="approve()" [disabled]="actionLoading()">
            <mat-icon>check_circle</mat-icon> Approve
          </button>
          <button mat-raised-button color="warn" (click)="openRejectDialog()" [disabled]="actionLoading()">
            <mat-icon>cancel</mat-icon> Reject
          </button>
        </div>
      }
      @if (showFinalize()) {
        <div class="action-bar">
          <button mat-raised-button color="primary" (click)="finalize()" [disabled]="actionLoading()">
            <mat-icon>lock</mat-icon> Finalize
          </button>
          <button mat-raised-button (click)="openPatchDialog()">
            <mat-icon>refresh</mat-icon> Patch
          </button>
        </div>
      }
      @if (showPatchFinalized()) {
        <div class="action-bar">
          <button mat-raised-button (click)="openPatchDialog()">
            <mat-icon>refresh</mat-icon> Patch
          </button>
          <button mat-raised-button color="accent" (click)="exportCsv()">
            <mat-icon>download</mat-icon> Export CSV
          </button>
          <button mat-raised-button color="accent" (click)="exportPayslips()">
            <mat-icon>description</mat-icon> Export Payslips
          </button>
        </div>
      }

      <!-- Skipped Employees -->
      @if (runSummary().skippedEmployees.length > 0) {
        <mat-card class="section-card">
          <mat-card-header><mat-card-title>Skipped Employees ({{ runSummary().skippedEmployees.length }})</mat-card-title></mat-card-header>
          <mat-card-content>
            <table mat-table [dataSource]="runSummary().skippedEmployees" class="full-width">
              <ng-container matColumnDef="employeeName">
                <th mat-header-cell *matHeaderCellDef>Employee</th>
                <td mat-cell *matCellDef="let s">
                  <a [routerLink]="['/payroll', runId, 'details', s.employeeId]">{{ s.employeeName || s.employeeId }}</a>
                </td>
              </ng-container>
              <ng-container matColumnDef="skipReason">
                <th mat-header-cell *matHeaderCellDef>Reason</th>
                <td mat-cell *matCellDef="let s">{{ s.skipReason }}</td>
              </ng-container>
              <tr mat-header-row *matHeaderRowDef="['employeeName', 'skipReason']"></tr>
              <tr mat-row *matRowDef="let row; columns: ['employeeName', 'skipReason']"></tr>
            </table>
          </mat-card-content>
        </mat-card>
      }

      <!-- Failed Employees -->
      @if (runSummary().failedEmployees.length > 0) {
        <mat-card class="section-card">
          <mat-card-header><mat-card-title>Failed Employees ({{ runSummary().failedEmployees.length }})</mat-card-title></mat-card-header>
          <mat-card-content>
            <table mat-table [dataSource]="runSummary().failedEmployees" class="full-width">
              <ng-container matColumnDef="employeeName">
                <th mat-header-cell *matHeaderCellDef>Employee</th>
                <td mat-cell *matCellDef="let f">
                  <a [routerLink]="['/payroll', runId, 'details', f.employeeId]">{{ f.employeeName || f.employeeId }}</a>
                </td>
              </ng-container>
              <ng-container matColumnDef="failureMessage">
                <th mat-header-cell *matHeaderCellDef>Error</th>
                <td mat-cell *matCellDef="let f">{{ f.failureMessage }}</td>
              </ng-container>
              <tr mat-header-row *matHeaderRowDef="['employeeName', 'failureMessage']"></tr>
              <tr mat-row *matRowDef="let row; columns: ['employeeName', 'failureMessage']"></tr>
            </table>
          </mat-card-content>
        </mat-card>
      }
    </div>
  `,
  styles: [`
    .page-container { padding: 1.5rem; max-width: 1200px; }
    .page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 1.5rem; }
    .page-header h1 { margin: 0.25rem 0 0; font-size: 1.5rem; font-weight: 400; }
    .back-link { font-size: 0.875rem; color: var(--mat-sys-primary, #1976d2); text-decoration: none; }
    .back-link:hover { text-decoration: underline; }

    .progress-card { margin-bottom: 1.5rem; }
    .progress-header { display: flex; justify-content: space-between; margin-bottom: 0.75rem; font-size: 0.9rem; }
    .failed-warning { color: #dc2626; font-size: 0.85rem; margin: 0.5rem 0 0; }

    .summary-cards { display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 1rem; margin-bottom: 1.5rem; }
    .summary-card { text-align: center; }
    .card-label { font-size: 0.8rem; text-transform: uppercase; color: var(--mat-sys-on-surface-variant, #666); margin-bottom: 0.5rem; }
    .card-value { font-size: 1.5rem; font-weight: 500; }
    .card-value.deduction { color: #dc2626; }
    .net-pay { border-left: 4px solid var(--mat-sys-primary, #1976d2); }
    .sub-text { font-size: 0.8rem; font-weight: 400; color: var(--mat-sys-on-surface-variant, #666); display: block; margin-top: 0.25rem; }

    .action-bar { display: flex; gap: 0.75rem; margin-bottom: 1.5rem; }

    .section-card { margin-bottom: 1rem; }
    .full-width { width: 100%; }
    table a { color: var(--mat-sys-primary, #1976d2); text-decoration: none; font-weight: 500; }
    table a:hover { text-decoration: underline; }
  `],
})
export class PayrollRunDashboardComponent implements OnInit, OnDestroy {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private payrollService = inject(PayrollService);
  private dialog = inject(MatDialog);

  runId = '';
  monthNames = ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];

  runSummary = signal<PayrollRunSummary>({
    id: '', year: 0, month: 0, status: 'Draft', startedAt: null, completedAt: null,
    processedBy: '', totalGrossPay: 0, totalDeductions: 0, totalNetPay: 0,
    totalEmployees: 0, calculatedCount: 0, skippedCount: 0, failedCount: 0,
    skippedEmployees: [], failedEmployees: [],
  });
  pollStatus = signal<PayrollRunStatus>({
    id: '', year: 0, month: 0, status: 'Draft', startedAt: null, completedAt: null,
    totalEmployees: 0, calculatedCount: 0, skippedCount: 0, failedCount: 0,
  });
  actionLoading = signal(false);

  private pollingSub: Subscription | null = null;

  showApprove(): boolean { return this.runSummary().status === 'PendingReview'; }
  showFinalize(): boolean { return this.runSummary().status === 'Approved'; }
  showPatchFinalized(): boolean { return this.runSummary().status === 'Finalized'; }
  progressPercent(): number {
    const s = this.pollStatus();
    return s.totalEmployees > 0 ? Math.round((s.calculatedCount / s.totalEmployees) * 100) : 0;
  }

  ngOnInit(): void {
    this.runId = this.route.snapshot.paramMap.get('id') ?? '';
    this.loadSummary();
  }

  ngOnDestroy(): void {
    this.stopPolling();
  }

  loadSummary(): void {
    this.payrollService.getSummary(this.runId).subscribe(summary => {
      this.runSummary.set(summary);
      if (summary.status === 'Processing' || summary.status === 'Draft') {
        this.startPolling();
      }
    });
  }

  startPolling(): void {
    this.stopPolling();
    this.pollingSub = interval(3000).subscribe(() => {
      this.payrollService.getStatus(this.runId).subscribe(status => {
        const prev = this.pollStatus().status;
        this.pollStatus.set(status);
        if (prev !== status.status && (status.status === 'PendingReview' || status.status === 'Approved' || status.status === 'Finalized')) {
          this.stopPolling();
          this.loadSummary();
        }
      });
    });
  }

  stopPolling(): void {
    this.pollingSub?.unsubscribe();
    this.pollingSub = null;
  }

  approve(): void {
    this.actionLoading.set(true);
    this.payrollService.approve(this.runId).subscribe({
      next: () => { this.actionLoading.set(false); this.loadSummary(); },
      error: () => this.actionLoading.set(false),
    });
  }

  finalize(): void {
    this.actionLoading.set(true);
    this.payrollService.finalize(this.runId).subscribe({
      next: () => { this.actionLoading.set(false); this.loadSummary(); },
      error: () => this.actionLoading.set(false),
    });
  }

  openRejectDialog(): void {
    const ref = this.dialog.open(RejectPayrollDialog, { width: '400px' });
    ref.afterClosed().subscribe(reason => {
      if (reason) {
        this.actionLoading.set(true);
        this.payrollService.reject(this.runId, reason).subscribe({
          next: () => { this.actionLoading.set(false); this.loadSummary(); },
          error: () => this.actionLoading.set(false),
        });
      }
    });
  }

  openPatchDialog(): void {
    const ref = this.dialog.open(PatchPayrollDialog, {
      width: '450px',
      data: {},
    });
    const instance = ref.componentInstance;
    instance.skippedEmployeeIds = this.runSummary().skippedEmployees.map(s => s.employeeId);

    ref.afterClosed().subscribe(employeeIds => {
      if (employeeIds && employeeIds.length > 0) {
        this.actionLoading.set(true);
        this.payrollService.patch({ originalRunId: this.runId, employeeIds }).subscribe({
          next: (newRunId) => {
            this.actionLoading.set(false);
            this.router.navigate(['/payroll', newRunId]);
          },
          error: () => this.actionLoading.set(false),
        });
      }
    });
  }

  exportCsv(): void {
    this.payrollService.exportCsv(this.runId).subscribe({
      next: blob => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `payroll-${this.runSummary().month}-${this.runSummary().year}.csv`;
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: () => {},
    });
  }

  exportPayslips(): void {
    this.payrollService.exportPayslips(this.runId).subscribe({
      next: blob => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `payslips-${this.runSummary().month}-${this.runSummary().year}.pdf`;
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: () => {},
    });
  }

  statusClass(status: string): string {
    switch (status) {
      case 'Draft': return 'status-draft';
      case 'Processing': return 'status-processing';
      case 'PendingReview': return 'status-pending';
      case 'Approved': return 'status-approved';
      case 'Finalized': return 'status-finalized';
      default: return '';
    }
  }
}
