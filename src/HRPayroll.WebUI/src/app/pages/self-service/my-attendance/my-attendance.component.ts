import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { SelfServiceService } from '../../../core/services/self-service.service';
import { AttendanceViewerResult, AttendanceViewerItemDto } from '../../../models/attendance';

@Component({
  selector: 'app-my-attendance',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatCardModule, MatTableModule, MatButtonModule, MatIconModule,
    MatFormFieldModule, MatInputModule, MatSelectModule, MatChipsModule,
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>My Attendance</h1>
      </div>

      <mat-card class="filters-card">
        <mat-card-content class="filters">
          <mat-form-field appearance="outline" subscriptSizing="dynamic">
            <mat-label>Month</mat-label>
            <mat-select [(ngModel)]="month" (selectionChange)="loadData()">
              @for (m of months; track m.value) {
                <mat-option [value]="m.value">{{ m.label }}</mat-option>
              }
            </mat-select>
          </mat-form-field>
          <mat-form-field appearance="outline" subscriptSizing="dynamic">
            <mat-label>Year</mat-label>
            <mat-select [(ngModel)]="year" (selectionChange)="loadData()">
              @for (y of years; track y) {
                <mat-option [value]="y">{{ y }}</mat-option>
              }
            </mat-select>
          </mat-form-field>
        </mat-card-content>
      </mat-card>

      @if (data(); as d) {
        <mat-card class="summary-card">
          <mat-card-content class="summary-grid">
            <div class="stat"><span class="stat-value">{{ d.summary.totalPresentDays }}</span><span class="stat-label">Present</span></div>
            <div class="stat"><span class="stat-value">{{ d.summary.totalLateOccurrences }}</span><span class="stat-label">Late</span></div>
            <div class="stat"><span class="stat-value">{{ d.summary.totalAbsentDays }}</span><span class="stat-label">Absent</span></div>
            <div class="stat"><span class="stat-value">{{ d.summary.totalLeaveDays }}</span><span class="stat-label">Leave</span></div>
            <div class="stat"><span class="stat-value">{{ d.summary.totalOvertimeHours }}</span><span class="stat-label">OT Hours</span></div>
          </mat-card-content>
        </mat-card>

        <mat-card>
          <mat-card-content>
            <table mat-table [dataSource]="d.days" class="full-width">
              <ng-container matColumnDef="date"><th mat-header-cell *matHeaderCellDef>Date</th><td mat-cell *matCellDef="let day">{{ day.date }}</td></ng-container>
              <ng-container matColumnDef="status"><th mat-header-cell *matHeaderCellDef>Status</th><td mat-cell *matCellDef="let day"><mat-chip [color]="statusColor(day.status)" selected>{{ day.status }}</mat-chip></td></ng-container>
              <ng-container matColumnDef="punchIn"><th mat-header-cell *matHeaderCellDef>In</th><td mat-cell *matCellDef="let day">{{ day.firstPunchIn || '—' }}</td></ng-container>
              <ng-container matColumnDef="punchOut"><th mat-header-cell *matHeaderCellDef>Out</th><td mat-cell *matCellDef="let day">{{ day.lastPunchOut || '—' }}</td></ng-container>
              <ng-container matColumnDef="worked"><th mat-header-cell *matHeaderCellDef>Worked</th><td mat-cell *matCellDef="let day">{{ day.netWorkedMinutes }}m</td></ng-container>
              <ng-container matColumnDef="dispute"><th mat-header-cell *matHeaderCellDef></th><td mat-cell *matCellDef="let day"><button mat-icon-button (click)="disputeDay(day)" matTooltip="Dispute"><mat-icon>report</mat-icon></button></td></ng-container>
              <tr mat-header-row *matHeaderRowDef="columns"></tr>
              <tr mat-row *matRowDef="let row; columns: columns;"></tr>
            </table>
          </mat-card-content>
        </mat-card>

        @if (selectedDay(); as day) {
          <mat-card class="dispute-card">
            <mat-card-content>
              <h3>Dispute — {{ day.date }}</h3>
              <div class="dispute-form">
                <mat-form-field appearance="outline" subscriptSizing="dynamic">
                  <mat-label>Claimed In</mat-label>
                  <input matInput type="time" [(ngModel)]="claimedIn" />
                </mat-form-field>
                <mat-form-field appearance="outline" subscriptSizing="dynamic">
                  <mat-label>Claimed Out</mat-label>
                  <input matInput type="time" [(ngModel)]="claimedOut" />
                </mat-form-field>
                <mat-form-field appearance="outline" subscriptSizing="dynamic" class="reason-field">
                  <mat-label>Reason</mat-label>
                  <input matInput [(ngModel)]="disputeReason" />
                </mat-form-field>
                <button mat-raised-button color="warn" [disabled]="!disputeReason" (click)="submitDispute(day)">Submit</button>
                <button mat-button (click)="cancelDispute()">Cancel</button>
              </div>
            </mat-card-content>
          </mat-card>
        }
      }
    </div>
  `,
  styles: [`
    .page-container { padding: 1.5rem; }
    .page-header { margin-bottom: 1rem; }
    .page-header h1 { margin: 0; font-size: 1.5rem; font-weight: 400; }
    .filters-card { margin-bottom: 1rem; }
    .filters { display: flex; gap: 1rem; align-items: center; }
    .summary-card { margin-bottom: 1rem; }
    .summary-grid { display: flex; gap: 1.5rem; flex-wrap: wrap; }
    .stat { display: flex; flex-direction: column; align-items: center; }
    .stat-value { font-size: 1.5rem; font-weight: 500; }
    .stat-label { font-size: 0.75rem; text-transform: uppercase; color: var(--mat-sys-on-surface-variant, #666); }
    .full-width { width: 100%; }
    .dispute-card { margin-top: 1rem; }
    .dispute-form { display: flex; gap: 1rem; flex-wrap: wrap; align-items: center; }
    .reason-field { min-width: 200px; flex: 1; }
  `],
})
export class MyAttendanceComponent implements OnInit {
  private service = inject(SelfServiceService);
  data = signal<AttendanceViewerResult | null>(null);
  month = new Date().getMonth() + 1;
  year = new Date().getFullYear();
  months = Array.from({ length: 12 }, (_, i) => ({ value: i + 1, label: new Date(0, i).toLocaleString('en', { month: 'long' }) }));
  years = Array.from({ length: 6 }, (_, i) => this.year + i - 2);
  columns = ['date', 'status', 'punchIn', 'punchOut', 'worked', 'dispute'];
  selectedDay = signal<AttendanceViewerItemDto | null>(null);
  claimedIn = '';
  claimedOut = '';
  disputeReason = '';

  ngOnInit(): void { this.loadData(); }

  loadData(): void {
    this.cancelDispute();
    this.service.getMyAttendance(this.year, this.month).subscribe(r => this.data.set(r));
  }

  disputeDay(day: AttendanceViewerItemDto): void {
    this.selectedDay.set(day);
    this.claimedIn = day.firstPunchIn || '';
    this.claimedOut = day.lastPunchOut || '';
    this.disputeReason = '';
  }

  cancelDispute(): void {
    this.selectedDay.set(null);
    this.claimedIn = '';
    this.claimedOut = '';
    this.disputeReason = '';
  }

  submitDispute(day: AttendanceViewerItemDto): void {
    this.service.disputeAttendance({
      summaryId: day.id,
      claimedPunchIn: this.claimedIn || null,
      claimedPunchOut: this.claimedOut || null,
      reason: this.disputeReason,
    }).subscribe({ next: () => { this.cancelDispute(); this.loadData(); } });
  }

  statusColor(s: string): string {
    const m: Record<string, string> = { OnTime: 'primary', Late: 'warn', EarlyDeparture: 'warn', AbsentUnexcused: 'warn', OnLeave: 'accent', Holiday: 'accent' };
    return m[s] || '';
  }
}
